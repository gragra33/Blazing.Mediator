using Blazing.Mediator.Statistics;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Blazing.Mediator;

/// <summary>
/// Implementation of the Mediator pattern that dispatches requests to their corresponding handlers and publishes notifications to handlers &amp; subscribers.
/// </summary>
/// <remarks>
/// The Mediator class serves as a centralized dispatcher that decouples senders from receivers for both requests (commands, queries, streams) and notifications.
/// It uses dependency injection to resolve handlers and subscribers at runtime, supporting request/response, streaming, and publish/subscribe patterns.
/// </remarks>
public sealed partial class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorDispatcherBase? _dispatcher;                    // Eagerly resolved in ctor; null when source gen is not active
    private readonly IMiddlewarePipelineBuilder? _pipelineBuilder;
    private readonly INotificationPipelineBuilder? _notificationPipelineBuilder;
    private readonly MediatorStatistics? _statistics;
    private readonly TelemetryOptions? _telemetryOptions;
    private readonly MediatorLogger? _logger;
    private readonly Generated.GeneratedMediatorContext? _generatedContext;  // Context for source-generated dispatch
    private readonly ISubscriberTracker? _subscriberTracker;                 // Cached once in ctor; null when stats not configured — fast path for Subscribe/Unsubscribe

    // Thread-safe collections for notification subscribers
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _specificSubscribers = new();
    private readonly ConcurrentBag<INotificationSubscriber> _genericSubscribers = [];

    /// <summary>
    /// The static OpenTelemetry Meter for Mediator metrics.
    /// </summary>
    public static readonly Meter Meter = new(_blazingMediatorName, typeof(Mediator).Assembly.GetName().Version?.ToString() ?? _defaultVersion);

    /// <summary>
    /// The static OpenTelemetry ActivitySource for Mediator tracing.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(_blazingMediatorName);

    /// <summary>
    /// Configuration for enabling/disabling telemetry (metrics/tracing).
    /// </summary>
    public static bool TelemetryEnabled { get; set; } = true;

    /// <summary>
    /// Configuration for enabling/disabling packet-level telemetry for streaming operations.
    /// When enabled, creates child spans for each packet which provides detailed visibility but may impact performance.
    /// </summary>
    public static bool PacketLevelTelemetryEnabled { get; set; } = false;

    /// <summary>
    /// Configuration for packet telemetry batching interval.
    /// Packets will be batched into events every N packets to reduce telemetry overhead.
    /// Set to 1 to disable batching (create event for every packet).
    /// </summary>
    public static int PacketTelemetryBatchSize { get; set; } = 10;

    // All send/publish/streaming metric instruments are centralised in MediatorMetrics (OpenTelemetry/MediatorMetrics.cs)

    // String constants
    private const string _blazingMediatorName = "Blazing.Mediator";
    private const string _defaultVersion = "1.0.0";

    // Activity/operation names
    private const string _mediatorPublishActivityPrefix = "Mediator.Publish.";
    private const string _mediatorSendStreamActivity = "Mediator.SendStream:";
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    /// <param name="statistics">The statistics service for tracking mediator usage. Can be null if statistics tracking is disabled.</param>
    /// <param name="telemetryOptions">The telemetry options for configuring OpenTelemetry integration. Can be null if telemetry is disabled.</param>
    /// <param name="logger">Optional granular logger for debug-level logging of mediator operations. Can be null if debug logging is disabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public Mediator(
        IServiceProvider serviceProvider,
        MediatorStatistics? statistics = null,
        TelemetryOptions? telemetryOptions = null,
        MediatorLogger? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        // Eagerly resolve the source-generated dispatcher so every Send/Publish call is a
        // plain field read — no lazy struct, no Volatile.Read, no DI on the hot path.
        // Returns null when AddMediator() has not been called; Send/Publish fall back to reflection.
        _dispatcher = serviceProvider.GetService<MediatorDispatcherBase>();
        // Pipeline builders are optional — source-gen apps do not register them.
        // Resolved here so no DI injection failure when they are absent.
        _pipelineBuilder = serviceProvider.GetService<IMiddlewarePipelineBuilder>();
        _notificationPipelineBuilder = serviceProvider.GetService<INotificationPipelineBuilder>();
        _statistics = statistics;               // Statistics can be null if tracking is disabled
        _telemetryOptions = telemetryOptions;   // Telemetry options can be null if telemetry is disabled
        _logger = logger;                       // Logger can be null if debug logging is disabled
        _subscriberTracker = serviceProvider.GetService<ISubscriberTracker>(); // Cached once; null when stats not configured
        
        // Initialize generated context with configuration-aware settings.
        // Use direct properties — no reflection overhead.
        _generatedContext = new Generated.GeneratedMediatorContext(
            telemetryOptions: telemetryOptions,
            statisticsOptions: statistics?.Options,
            loggingOptions: logger?.LoggingOptions
        );
    }

    /// <summary>
    /// Gets whether telemetry is enabled based on static property override or options configuration.
    /// The static TelemetryEnabled property takes precedence when explicitly set to false.
    /// </summary>
    private bool IsTelemetryEnabled => TelemetryEnabled && (_telemetryOptions?.Enabled ?? true);

    /// <summary>
    /// Gets whether packet-level telemetry is enabled based on options or static property fallback.
    /// </summary>
    private bool IsPacketLevelTelemetryEnabled => _telemetryOptions?.PacketLevelTelemetryEnabled ?? PacketLevelTelemetryEnabled;

    /// <summary>
    /// Gets the packet telemetry batch size based on options or static property fallback.
    /// </summary>
    private int GetPacketTelemetryBatchSize => _telemetryOptions?.PacketTelemetryBatchSize ?? PacketTelemetryBatchSize;

    /// <summary>
    /// Gets whether exception details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureExceptionDetails => _telemetryOptions?.CaptureExceptionDetails ?? true;

    /// <summary>
    /// Gets the maximum exception message length based on options (default 200).
    /// </summary>
    private int MaxExceptionMessageLength => _telemetryOptions?.MaxExceptionMessageLength ?? 200;

    /// <summary>
    /// Gets the maximum stack trace lines based on options (default 3).
    /// </summary>
    private int MaxStackTraceLines => _telemetryOptions?.MaxStackTraceLines ?? 3;

    /// <summary>
    /// Gets the sensitive data patterns for filtering telemetry data.
    /// </summary>
    private List<string> SensitiveDataPatterns => _telemetryOptions?.SensitiveDataPatterns ??
        ["password", "token", "secret", "key", "auth", "credential", "connection"];
}
