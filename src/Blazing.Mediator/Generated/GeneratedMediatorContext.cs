//------------------------------------------------------------------------------
// Runtime wrapper for configuration context used by source-generated code.
//------------------------------------------------------------------------------

#nullable enable

namespace Blazing.Mediator.Generated;

/// <summary>
/// Runtime configuration context for source-generated dispatch code.
/// Provides cached access to configuration flags for zero-allocation performance.
/// </summary>
/// <remarks>
/// This class is instantiated once per mediator instance and caches frequently
/// accessed configuration flags to avoid repeated property lookups.
/// </remarks>
internal sealed class GeneratedMediatorContext
{
    /// <summary>
    /// Initializes a new instance with the specified configuration.
    /// </summary>
    /// <param name="configuration">The mediator configuration to wrap.</param>
    public GeneratedMediatorContext(MediatorConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        
        Configuration = configuration;
        
        // Cache telemetry flags (runtime toggleable)
        IsTelemetryEnabled = configuration.TelemetryOptions?.Enabled ?? false;
        CaptureHandlerDetails = configuration.TelemetryOptions?.CaptureHandlerDetails ?? false;
        RequestMiddlewareCaptureMode = configuration.TelemetryOptions?.MiddlewareCaptureMode ?? MiddlewareCaptureMode.None;
        CaptureExceptionDetails = configuration.TelemetryOptions?.CaptureExceptionDetails ?? false;
        CaptureNotificationHandlerDetails = configuration.TelemetryOptions?.CaptureNotificationHandlerDetails ?? false;
        CaptureNotificationMiddlewareDetails = configuration.TelemetryOptions?.CaptureNotificationMiddlewareDetails ?? false;
        CreateHandlerChildSpans = configuration.TelemetryOptions?.CreateHandlerChildSpans ?? false;
        CaptureSubscriberMetrics = configuration.TelemetryOptions?.CaptureSubscriberMetrics ?? false;
        PacketLevelTelemetryEnabled = configuration.TelemetryOptions?.PacketLevelTelemetryEnabled ?? false;
        EnableStreamingMetrics = configuration.TelemetryOptions?.EnableStreamingMetrics ?? false;
        
        // Cache statistics flags (runtime toggleable)
        IsStatisticsEnabled = configuration.StatisticsOptions?.IsEnabled ?? false;
        EnableRequestMetrics = configuration.StatisticsOptions?.EnableRequestMetrics ?? false;
        EnableNotificationMetrics = configuration.StatisticsOptions?.EnableNotificationMetrics ?? false;
        EnableMiddlewareMetrics = configuration.StatisticsOptions?.EnableMiddlewareMetrics ?? false;
        EnablePerformanceCounters = configuration.StatisticsOptions?.EnablePerformanceCounters ?? false;
        EnableDetailedAnalysis = configuration.StatisticsOptions?.EnableDetailedAnalysis ?? false;
        
        // Cache logging flags (runtime toggleable)
        IsLoggingEnabled = configuration.LoggingOptions != null;
        EnableSend = configuration.LoggingOptions?.EnableSend ?? false;
        EnablePublish = configuration.LoggingOptions?.EnablePublish ?? false;
        EnableSendStream = configuration.LoggingOptions?.EnableSendStream ?? false;
        EnableRequestMiddleware = configuration.LoggingOptions?.EnableRequestMiddleware ?? false;
        EnableNotificationMiddleware = configuration.LoggingOptions?.EnableNotificationMiddleware ?? false;
        EnableDetailedHandlerInfo = configuration.LoggingOptions?.EnableDetailedHandlerInfo ?? false;
        EnableMiddlewareExecutionOrder = configuration.LoggingOptions?.EnableMiddlewareExecutionOrder ?? false;
        EnablePerformanceTiming = configuration.LoggingOptions?.EnablePerformanceTiming ?? false;
        EnableSubscriberDetails = configuration.LoggingOptions?.EnableSubscriberDetails ?? false;
        EnableConstraintLogging = configuration.LoggingOptions?.EnableConstraintLogging ?? false;
        
        // Discovery options (build-time frozen, but stored for hybrid middleware)
        DiscoverMiddleware = configuration.DiscoverMiddleware;
        DiscoverNotificationMiddleware = configuration.DiscoverNotificationMiddleware;
        DiscoverConstrainedMiddleware = configuration.DiscoverConstrainedMiddleware;
        DiscoverNotificationHandlers = configuration.DiscoverNotificationHandlers;
    }
    
    /// <summary>
    /// Initializes a new instance with individual option objects.
    /// Used when MediatorConfiguration is not available at runtime (e.g., in Mediator constructor).
    /// </summary>
    /// <param name="telemetryOptions">Optional telemetry configuration</param>
    /// <param name="statisticsOptions">Optional statistics configuration</param>
    /// <param name="loggingOptions">Optional logging configuration</param>
    public GeneratedMediatorContext(
        TelemetryOptions? telemetryOptions = null,
        StatisticsOptions? statisticsOptions = null,
        LoggingOptions? loggingOptions = null)
    {
        Configuration = null;  // Not available when using this constructor
        
        // Cache telemetry flags (runtime toggleable)
        IsTelemetryEnabled = telemetryOptions?.Enabled ?? false;
        CaptureHandlerDetails = telemetryOptions?.CaptureHandlerDetails ?? false;
        RequestMiddlewareCaptureMode = telemetryOptions?.MiddlewareCaptureMode ?? MiddlewareCaptureMode.None;
        CaptureExceptionDetails = telemetryOptions?.CaptureExceptionDetails ?? false;
        CaptureNotificationHandlerDetails = telemetryOptions?.CaptureNotificationHandlerDetails ?? false;
        CaptureNotificationMiddlewareDetails = telemetryOptions?.CaptureNotificationMiddlewareDetails ?? false;
        CreateHandlerChildSpans = telemetryOptions?.CreateHandlerChildSpans ?? false;
        CaptureSubscriberMetrics = telemetryOptions?.CaptureSubscriberMetrics ?? false;
        PacketLevelTelemetryEnabled = telemetryOptions?.PacketLevelTelemetryEnabled ?? false;
        EnableStreamingMetrics = telemetryOptions?.EnableStreamingMetrics ?? false;
        
        // Cache statistics flags (runtime toggleable)
        IsStatisticsEnabled = statisticsOptions?.IsEnabled ?? false;
        EnableRequestMetrics = statisticsOptions?.EnableRequestMetrics ?? false;
        EnableNotificationMetrics = statisticsOptions?.EnableNotificationMetrics ?? false;
        EnableMiddlewareMetrics = statisticsOptions?.EnableMiddlewareMetrics ?? false;
        EnablePerformanceCounters = statisticsOptions?.EnablePerformanceCounters ?? false;
        EnableDetailedAnalysis = statisticsOptions?.EnableDetailedAnalysis ?? false;
        
        // Cache logging flags (runtime toggleable)
        IsLoggingEnabled = loggingOptions != null;
        EnableSend = loggingOptions?.EnableSend ?? false;
        EnablePublish = loggingOptions?.EnablePublish ?? false;
        EnableSendStream = loggingOptions?.EnableSendStream ?? false;
        EnableRequestMiddleware = loggingOptions?.EnableRequestMiddleware ?? false;
        EnableNotificationMiddleware = loggingOptions?.EnableNotificationMiddleware ?? false;
        EnableDetailedHandlerInfo = loggingOptions?.EnableDetailedHandlerInfo ?? false;
        EnableMiddlewareExecutionOrder = loggingOptions?.EnableMiddlewareExecutionOrder ?? false;
        EnablePerformanceTiming = loggingOptions?.EnablePerformanceTiming ?? false;
        EnableSubscriberDetails = loggingOptions?.EnableSubscriberDetails ?? false;
        EnableConstraintLogging = loggingOptions?.EnableConstraintLogging ?? false;
        
        // Discovery options default to false (build-time frozen behavior not available in this constructor)
        DiscoverMiddleware = false;
        DiscoverNotificationMiddleware = false;
        DiscoverConstrainedMiddleware = false;
        DiscoverNotificationHandlers = false;
    }
    
    /// <summary>
    /// Gets the underlying configuration object for advanced scenarios.
    /// May be null if constructed with individual option objects.
    /// </summary>
    public MediatorConfiguration? Configuration { get; }
    
    #region Telemetry Flags (Runtime Toggleable)
    
    /// <summary>
    /// Gets whether OpenTelemetry tracking is enabled.
    /// Master switch - check this first before any telemetry operations.
    /// </summary>
    public bool IsTelemetryEnabled { get; }
    
    /// <summary>
    /// Gets whether to capture detailed handler information in telemetry.
    /// </summary>
    public bool CaptureHandlerDetails { get; }
    
    /// <summary>
    /// Controls what middleware information is captured on request telemetry spans.
    /// </summary>
    public MiddlewareCaptureMode RequestMiddlewareCaptureMode { get; }
    
    /// <summary>
    /// Gets whether to capture exception details in telemetry.
    /// </summary>
    public bool CaptureExceptionDetails { get; }
    
    /// <summary>
    /// Gets whether to capture notification handler details in telemetry.
    /// </summary>
    public bool CaptureNotificationHandlerDetails { get; }
    
    /// <summary>
    /// Gets whether to capture notification middleware details in telemetry.
    /// </summary>
    public bool CaptureNotificationMiddlewareDetails { get; }
    
    /// <summary>
    /// Gets whether to create child spans for individual handlers.
    /// </summary>
    public bool CreateHandlerChildSpans { get; }
    
    /// <summary>
    /// Gets whether to capture subscriber metrics in telemetry.
    /// </summary>
    public bool CaptureSubscriberMetrics { get; }
    
    /// <summary>
    /// Gets whether packet-level telemetry is enabled for streaming requests.
    /// </summary>
    public bool PacketLevelTelemetryEnabled { get; }
    
    /// <summary>
    /// Gets whether streaming metrics are enabled.
    /// </summary>
    public bool EnableStreamingMetrics { get; }
    
    #endregion
    
    #region Statistics Flags (Runtime Toggleable)
    
    /// <summary>
    /// Gets whether statistics tracking is enabled.
    /// Master switch - check this first before any statistics operations.
    /// </summary>
    public bool IsStatisticsEnabled { get; }
    
    /// <summary>
    /// Gets whether to track request (query/command) metrics.
    /// </summary>
    public bool EnableRequestMetrics { get; }
    
    /// <summary>
    /// Gets whether to track notification metrics.
    /// </summary>
    public bool EnableNotificationMetrics { get; }
    
    /// <summary>
    /// Gets whether to track middleware execution metrics.
    /// </summary>
    public bool EnableMiddlewareMetrics { get; }
    
    /// <summary>
    /// Gets whether to enable performance counters.
    /// </summary>
    public bool EnablePerformanceCounters { get; }
    
    /// <summary>
    /// Gets whether to enable detailed analysis features.
    /// </summary>
    public bool EnableDetailedAnalysis { get; }
    
    #endregion
    
    #region Logging Flags (Runtime Toggleable)
    
    /// <summary>
    /// Gets whether logging is enabled at all.
    /// Master switch - check this first before any logging operations.
    /// </summary>
    public bool IsLoggingEnabled { get; }
    
    /// <summary>
    /// Gets whether to log Send operations.
    /// </summary>
    public bool EnableSend { get; }
    
    /// <summary>
    /// Gets whether to log Publish operations.
    /// </summary>
    public bool EnablePublish { get; }
    
    /// <summary>
    /// Gets whether to log SendStream operations.
    /// </summary>
    public bool EnableSendStream { get; }
    
    /// <summary>
    /// Gets whether to log request middleware execution.
    /// </summary>
    public bool EnableRequestMiddleware { get; }
    
    /// <summary>
    /// Gets whether to log notification middleware execution.
    /// </summary>
    public bool EnableNotificationMiddleware { get; }
    
    /// <summary>
    /// Gets whether to log detailed handler information.
    /// </summary>
    public bool EnableDetailedHandlerInfo { get; }
    
    /// <summary>
    /// Gets whether to log middleware execution order.
    /// </summary>
    public bool EnableMiddlewareExecutionOrder { get; }
    
    /// <summary>
    /// Gets whether to log performance timing information.
    /// </summary>
    public bool EnablePerformanceTiming { get; }
    
    /// <summary>
    /// Gets whether to log subscriber details for notifications.
    /// </summary>
    public bool EnableSubscriberDetails { get; }
    
    /// <summary>
    /// Gets whether to log middleware constraint validation.
    /// </summary>
    public bool EnableConstraintLogging { get; }
    
    #endregion
    
    #region Discovery Flags (Build-Time Frozen)
    
    /// <summary>
    /// Gets whether middleware discovery was enabled at build time.
    /// NOTE: Changing this at runtime has no effect on generated code.
    /// </summary>
    public bool DiscoverMiddleware { get; }
    
    /// <summary>
    /// Gets whether notification middleware discovery was enabled at build time.
    /// NOTE: Changing this at runtime has no effect on generated code.
    /// </summary>
    public bool DiscoverNotificationMiddleware { get; }
    
    /// <summary>
    /// Gets whether constrained middleware discovery was enabled at build time.
    /// NOTE: Changing this at runtime has no effect on generated code.
    /// </summary>
    public bool DiscoverConstrainedMiddleware { get; }
    
    /// <summary>
    /// Gets whether notification handler discovery was enabled at build time.
    /// NOTE: Changing this at runtime has no effect on generated code.
    /// </summary>
    public bool DiscoverNotificationHandlers { get; }
    
    #endregion
}
