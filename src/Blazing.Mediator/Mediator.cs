using Blazing.Mediator.Statistics;
using Blazing.Mediator.OpenTelemetry;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Blazing.Mediator;

/// <summary>
/// Implementation of the Mediator pattern that dispatches requests to their corresponding handlers.
/// </summary>
/// <remarks>
/// The Mediator class serves as a centralized request dispatcher that decouples the request sender
/// from the request handler. It uses dependency injection to resolve handlers at runtime and
/// supports both void and typed responses.
/// </remarks>
public sealed class Mediator : IMediator, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMiddlewarePipelineBuilder _pipelineBuilder;
    private readonly INotificationPipelineBuilder _notificationPipelineBuilder;
    private readonly MediatorStatistics? _statistics;
    private readonly MediatorTelemetryOptions? _telemetryOptions;
    private readonly MediatorLogger? _logger;
    private bool _disposed;

    // Thread-safe collections for notification subscribers
    private readonly ConcurrentDictionary<Type, ConcurrentBag<object>> _specificSubscribers = new();
    private readonly ConcurrentBag<INotificationSubscriber> _genericSubscribers = [];

    /// <summary>
    /// The static OpenTelemetry Meter for Mediator metrics.
    /// </summary>
    public static readonly Meter Meter = new("Blazing.Mediator", typeof(Mediator).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    /// <summary>
    /// The static OpenTelemetry ActivitySource for Mediator tracing.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("Blazing.Mediator");

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

    // Pre-create metrics for performance and thread safety
    private static readonly Histogram<double> SendDurationHistogram = Meter.CreateHistogram<double>("mediator.send.duration", unit: "ms", description: "Duration of mediator send operations");
    private static readonly Counter<long> SendSuccessCounter = Meter.CreateCounter<long>("mediator.send.success", description: "Number of successful mediator send operations");
    private static readonly Counter<long> SendFailureCounter = Meter.CreateCounter<long>("mediator.send.failure", description: "Number of failed mediator send operations");
    private static readonly Histogram<double> PublishDurationHistogram = Meter.CreateHistogram<double>("mediator.publish.duration", unit: "ms", description: "Duration of mediator publish operations");
    private static readonly Counter<long> PublishSuccessCounter = Meter.CreateCounter<long>("mediator.publish.success", description: "Number of successful mediator publish operations");
    private static readonly Counter<long> PublishFailureCounter = Meter.CreateCounter<long>("mediator.publish.failure", description: "Number of failed mediator publish operations");
    private static readonly Histogram<double> PublishSubscriberDurationHistogram = Meter.CreateHistogram<double>("mediator.publish.subscriber.duration", unit: "ms", description: "Duration of individual subscriber notification processing");
    private static readonly Counter<long> PublishSubscriberSuccessCounter = Meter.CreateCounter<long>("mediator.publish.subscriber.success", description: "Number of successful subscriber notifications");
    private static readonly Counter<long> PublishSubscriberFailureCounter = Meter.CreateCounter<long>("mediator.publish.subscriber.failure", description: "Number of failed subscriber notifications");

    // Streaming metrics (internal for StreamTelemetryContext access)
    internal static readonly Histogram<double> StreamDurationHistogram = Meter.CreateHistogram<double>("mediator.stream.duration", unit: "ms", description: "Duration of mediator stream operations");
    internal static readonly Counter<long> StreamSuccessCounter = Meter.CreateCounter<long>("mediator.stream.success", description: "Number of successful mediator stream operations");
    internal static readonly Counter<long> StreamFailureCounter = Meter.CreateCounter<long>("mediator.stream.failure", description: "Number of failed mediator stream operations");
    internal static readonly Histogram<double> StreamThroughputHistogram = Meter.CreateHistogram<double>("mediator.stream.throughput", unit: "items/sec", description: "Throughput of mediator stream operations");
    internal static readonly Histogram<double> StreamTtfbHistogram = Meter.CreateHistogram<double>("mediator.stream.ttfb", unit: "ms", description: "Time to first byte for mediator stream operations");

    // Enhanced packet-level streaming metrics
    internal static readonly Counter<long> StreamPacketCounter = Meter.CreateCounter<long>("mediator.stream.packet.count", description: "Number of packets processed in stream operations");
    internal static readonly Histogram<double> StreamPacketProcessingTimeHistogram = Meter.CreateHistogram<double>("mediator.stream.packet.processing_time", unit: "ms", description: "Processing time for individual stream packets");
    internal static readonly Histogram<double> StreamInterPacketTimeHistogram = Meter.CreateHistogram<double>("mediator.stream.inter_packet_time", unit: "ms", description: "Time between consecutive stream packets");
    internal static readonly Histogram<double> StreamPacketJitterHistogram = Meter.CreateHistogram<double>("mediator.stream.packet.jitter", unit: "ms", description: "Jitter in stream packet timing");

    // Health check metrics (internal for StreamTelemetryContext access)
    internal static readonly Counter<long> TelemetryHealthCounter = Meter.CreateCounter<long>("mediator.telemetry.health", description: "Health check counter for telemetry system");

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve handlers.</param>
    /// <param name="pipelineBuilder">The middleware pipeline builder.</param>
    /// <param name="notificationPipelineBuilder">The notification middleware pipeline builder.</param>
    /// <param name="statistics">The statistics service for tracking mediator usage. Can be null if statistics tracking is disabled.</param>
    /// <param name="telemetryOptions">The telemetry options for configuring OpenTelemetry integration. Can be null if telemetry is disabled.</param>
    /// <param name="logger">Optional granular logger for debug-level logging of mediator operations. Can be null if debug logging is disabled.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider, pipelineBuilder, or notificationPipelineBuilder is null.</exception>
    public Mediator(IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder, INotificationPipelineBuilder notificationPipelineBuilder, MediatorStatistics? statistics, MediatorTelemetryOptions? telemetryOptions = null, MediatorLogger? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        _notificationPipelineBuilder = notificationPipelineBuilder ?? throw new ArgumentNullException(nameof(notificationPipelineBuilder));
        _statistics = statistics; // Statistics can be null if tracking is disabled
        _telemetryOptions = telemetryOptions; // Telemetry options can be null if telemetry is disabled
        _logger = logger; // Logger can be null if debug logging is disabled
    }

    /// <summary>
    /// Gets whether telemetry is enabled based on options or static property fallback.
    /// </summary>
    private bool IsTelemetryEnabled => _telemetryOptions?.Enabled ?? TelemetryEnabled;

    /// <summary>
    /// Gets whether packet-level telemetry is enabled based on options or static property fallback.
    /// </summary>
    private bool IsPacketLevelTelemetryEnabled => _telemetryOptions?.PacketLevelTelemetryEnabled ?? PacketLevelTelemetryEnabled;

    /// <summary>
    /// Gets the packet telemetry batch size based on options or static property fallback.
    /// </summary>
    private int GetPacketTelemetryBatchSize => _telemetryOptions?.PacketTelemetryBatchSize ?? PacketTelemetryBatchSize;

    /// <summary>
    /// Gets whether middleware details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureMiddlewareDetails => _telemetryOptions?.CaptureMiddlewareDetails ?? true;

    /// <summary>
    /// Gets whether handler details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureHandlerDetails => _telemetryOptions?.CaptureHandlerDetails ?? true;

    /// <summary>
    /// Gets whether exception details should be captured based on options (default true).
    /// </summary>
    private bool ShouldCaptureExceptionDetails => _telemetryOptions?.CaptureExceptionDetails ?? true;

    /// <summary>
    /// Gets whether health checks are enabled based on options (default true).
    /// </summary>
    private bool AreHealthChecksEnabled => _telemetryOptions?.EnableHealthChecks ?? true;

    /// <summary>
    /// Gets the maximum exception message length based on options (default 200).
    /// </summary>
    private int MaxExceptionMessageLength => _telemetryOptions?.MaxExceptionMessageLength ?? 200;

    /// <summary>
    /// Gets the maximum stack trace lines based on options (default 3).
    /// </summary>
    private int MaxStackTraceLines => _telemetryOptions?.MaxStackTraceLines ?? 3;

    /// <summary>
    /// Gets whether streaming metrics are enabled based on options (default true).
    /// </summary>
    private bool AreStreamingMetricsEnabled => _telemetryOptions?.EnableStreamingMetrics ?? true;

    /// <summary>
    /// Gets whether packet size should be captured based on options (default false).
    /// </summary>
    private bool ShouldCapturePacketSize => _telemetryOptions?.CapturePacketSize ?? false;

    /// <summary>
    /// Gets the sensitive data patterns for filtering telemetry data.
    /// </summary>
    private List<string> SensitiveDataPatterns => _telemetryOptions?.SensitiveDataPatterns ??
        ["password", "token", "secret", "key", "auth", "credential", "connection"];

    /// <summary>
    /// Sends a command request through the middleware pipeline to its corresponding handler.
    /// </summary>
    /// <param name="request">The command request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"Mediator.Send:{request.GetType().Name}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();
        long startMemory = 0;

        // Debug logging: Send operation started
        _logger?.SendOperationStarted(request.GetType().Name, IsTelemetryEnabled);

        // Record initial memory if performance counters are enabled
        if (_statistics != null && HasPerformanceCountersEnabled())
        {
            startMemory = GC.GetTotalMemory(false);
        }

        try
        {
            _statistics?.IncrementCommand(request.GetType().Name);
            Type requestType = request.GetType();
            Type handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

            // Debug logging: Request type classification
            _logger?.SendRequestTypeClassification(requestType.Name, "command");

            // Debug logging: Handler resolution
            _logger?.SendHandlerResolution(handlerType.Name, requestType.Name);

            // Get middleware pipeline information for telemetry
            if (IsTelemetryEnabled && ShouldCaptureMiddlewareDetails && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
            {
                var middlewareInfo = inspector.GetDetailedMiddlewareInfo(_serviceProvider);
                List<string> allMiddleware = middlewareInfo
                    .Where(m => IsMiddlewareApplicable(m.Type, requestType))
                    .OrderBy(m => m.Order)
                    .Select(m => SanitizeMiddlewareName(m.Type.Name))
                    .ToList();

                activity?.SetTag("middleware.pipeline", string.Join(",", allMiddleware));
            }

            async Task FinalHandler()
            {
                // Check for multiple handler registrations
                IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
                object[] handlerArray = handlers.Where(h => h != null).ToArray()!;
                switch (handlerArray)
                {
                    case { Length: 0 }:
                        _logger?.NoHandlerFoundWarning(requestType.Name);
                        throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                    case { Length: > 1 }:
                        _logger?.MultipleHandlersFoundWarning(requestType.Name, string.Join(", ", handlerArray.Select(h => h.GetType().Name)));
                        throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
                }
                object handler = handlerArray[0];

                // Debug logging: Handler found
                _logger?.SendHandlerFound(handler.GetType().Name, requestType.Name);

                MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");
                try
                {
                    if (IsTelemetryEnabled && ShouldCaptureHandlerDetails)
                    {
                        activity?.SetTag("handler.type", SanitizeTypeName(handler.GetType().Name));
                    }

                    Task? task = (Task?)method.Invoke(handler, [request, cancellationToken]);
                    if (task != null)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

            // Execute through middleware pipeline with enhanced tracking
            await ExecuteWithMiddlewareTracking(request, FinalHandler, executedMiddleware, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Debug logging: Send operation completed
            _logger?.SendOperationCompleted(request.GetType().Name, stopwatch.Elapsed.TotalMilliseconds, exception == null);

            // Enhanced statistics recording
            if (_statistics != null)
            {
                var requestTypeName = request.GetType().Name;

                // Record execution time if performance counters are enabled
                if (HasPerformanceCountersEnabled())
                {
                    _statistics.RecordExecutionTime(requestTypeName, stopwatch.ElapsedMilliseconds, exception == null);

                    // Record memory allocation if available
                    if (startMemory > 0)
                    {
                        var endMemory = GC.GetTotalMemory(false);
                        var memoryDelta = endMemory - startMemory;
                        if (memoryDelta > 0)
                        {
                            _statistics.RecordMemoryAllocation(memoryDelta);
                        }
                    }
                }

                // Record detailed analysis if enabled
                if (HasDetailedAnalysisEnabled())
                {
                    _statistics.RecordExecutionPattern(requestTypeName, DateTime.UtcNow);
                }

                // Record middleware execution metrics if enabled
                if (HasMiddlewareMetricsEnabled())
                {
                    foreach (var middlewareName in executedMiddleware)
                    {
                        // Record that middleware was executed (duration would be tracked in pipeline)
                        _statistics.RecordMiddlewareExecution(middlewareName, 0, true);
                    }
                }
            }

            if (IsTelemetryEnabled)
            {
                // Record metrics with appropriate tags
                var tags = new TagList
                {
                    { "request_name", SanitizeTypeName(request.GetType().Name) },
                    { "request_type", "command" }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("middleware.executed", string.Join(",", executedMiddleware));
                }

                SendDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                if (exception == null)
                {
                    SendSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    tags.Add("exception.type", SanitizeTypeName(exception.GetType().Name));
                    if (ShouldCaptureExceptionDetails)
                    {
                        tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
                    }
                    SendFailureCounter.Add(1, tags);

                    // Add exception details to activity if enabled
                    if (ShouldCaptureExceptionDetails)
                    {
                        activity?.SetTag("exception.type", SanitizeTypeName(exception.GetType().Name));
                        activity?.SetTag("exception.message", SanitizeExceptionMessage(exception.Message));

                        // Add stack trace if available and configured
                        var sanitizedStackTrace = SanitizeStackTrace(exception.StackTrace);
                        if (!string.IsNullOrEmpty(sanitizedStackTrace))
                        {
                            activity?.SetTag("exception.stack_trace", sanitizedStackTrace);
                        }
                    }
                    activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(exception.Message));
                }

                // Add activity tags
                activity?.SetTag("request_name", SanitizeTypeName(request.GetType().Name));
                activity?.SetTag("request_type", "command");
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                if (executedMiddleware.Count > 0)
                {
                    activity?.SetTag("middleware.executed", string.Join(",", executedMiddleware));
                }

                // Increment health check counter
                TelemetryHealthCounter.Add(1, new TagList { { "operation", "send" } });
            }
        }
    }

    /// <summary>
    /// Sends a query request through the middleware pipeline to its corresponding handler and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected from the handler</typeparam>
    /// <param name="request">The query request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task containing the response from the handler</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type or the handler returns null</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"Mediator.Send:{request.GetType().Name}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();
        long startMemory = 0;

        // Debug logging: Send operation started
        _logger?.SendOperationStarted(request.GetType().Name, IsTelemetryEnabled);

        // Record initial memory if performance counters are enabled
        if (_statistics != null && HasPerformanceCountersEnabled())
        {
            startMemory = GC.GetTotalMemory(false);
        }

        try
        {
            if (_statistics != null)
            {
                bool isQuery = DetermineIfQuery(request);
                if (isQuery)
                {
                    _statistics.IncrementQuery(request.GetType().Name);
                    // Debug logging: Request type classification
                    _logger?.SendRequestTypeClassification(request.GetType().Name, "query");
                }
                else
                {
                    _statistics.IncrementCommand(request.GetType().Name);
                    // Debug logging: Request type classification
                    _logger?.SendRequestTypeClassification(request.GetType().Name, "command");
                }
            }

            Type requestType = request.GetType();
            Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Debug logging: Handler resolution
            _logger?.SendHandlerResolution(handlerType.Name, requestType.Name);

            // Get middleware pipeline information for telemetry
            if (IsTelemetryEnabled && ShouldCaptureMiddlewareDetails && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
            {
                var middlewareInfo = inspector.GetDetailedMiddlewareInfo(_serviceProvider);
                List<string> allMiddleware = middlewareInfo
                    .Where(m => IsMiddlewareApplicable(m.Type, requestType, typeof(TResponse)))
                    .OrderBy(m => m.Order)
                    .Select(m => SanitizeMiddlewareName(m.Type.Name))
                    .ToList();

                activity?.SetTag("middleware.pipeline", string.Join(",", allMiddleware));
            }

            async Task<TResponse> FinalHandler()
            {
                IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
                object[] handlerArray = handlers.Where(h => h != null).ToArray()!;
                switch (handlerArray)
                {
                    case { Length: 0 }:
                        _logger?.NoHandlerFoundWarning(requestType.Name);
                        throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                    case { Length: > 1 }:
                        _logger?.MultipleHandlersFoundWarning(requestType.Name, string.Join(", ", handlerArray.Select(h => h.GetType().Name)));
                        throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
                }
                object handler = handlerArray[0];

                // Debug logging: Handler found
                _logger?.SendHandlerFound(handler.GetType().Name, requestType.Name);

                MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");
                try
                {
                    if (IsTelemetryEnabled && ShouldCaptureHandlerDetails)
                    {
                        activity?.SetTag("handler.type", SanitizeTypeName(handler.GetType().Name));
                        activity?.SetTag("response.type", SanitizeTypeName(typeof(TResponse).Name));
                    }

                    Task<TResponse>? task = (Task<TResponse>?)method.Invoke(handler, [request, cancellationToken]);
                    if (task != null)
                    {
                        return await task.ConfigureAwait(false);
                    }
                    throw new InvalidOperationException($"Handler for {requestType.Name} returned null");
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

            // Execute through middleware pipeline with enhanced tracking
            return await ExecuteWithMiddlewareTracking(request, FinalHandler, executedMiddleware, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Debug logging: Send operation completed
            _logger?.SendOperationCompleted(request.GetType().Name, stopwatch.Elapsed.TotalMilliseconds, exception == null);

            // Enhanced statistics recording
            if (_statistics != null)
            {
                var requestTypeName = request.GetType().Name;

                // Record execution time if performance counters are enabled
                if (HasPerformanceCountersEnabled())
                {
                    _statistics.RecordExecutionTime(requestTypeName, stopwatch.ElapsedMilliseconds, exception == null);

                    // Record memory allocation if available
                    if (startMemory > 0)
                    {
                        var endMemory = GC.GetTotalMemory(false);
                        var memoryDelta = endMemory - startMemory;
                        if (memoryDelta > 0)
                        {
                            _statistics.RecordMemoryAllocation(memoryDelta);
                        }
                    }
                }

                // Record detailed analysis if enabled
                if (HasDetailedAnalysisEnabled())
                {
                    _statistics.RecordExecutionPattern(requestTypeName, DateTime.UtcNow);
                }

                // Record middleware execution metrics if enabled
                if (HasMiddlewareMetricsEnabled())
                {
                    foreach (var middlewareName in executedMiddleware)
                    {
                        _statistics.RecordMiddlewareExecution(middlewareName, 0, true);
                    }
                }
            }

            if (IsTelemetryEnabled)
            {
                // Determine request type for telemetry
                bool isQuery = DetermineIfQuery(request);

                // Record metrics with appropriate tags
                var tags = new TagList
                {
                    { "request_name", SanitizeTypeName(request.GetType().Name) },
                    { "request_type", isQuery ? "query" : "command" },
                    { "response_type", SanitizeTypeName(typeof(TResponse).Name) }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("middleware.executed", string.Join(",", executedMiddleware));
                }

                SendDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                if (exception == null)
                {
                    SendSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                else
                {
                    tags.Add("exception.type", SanitizeTypeName(exception.GetType().Name));
                    tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
                    SendFailureCounter.Add(1, tags);

                    // Add exception details to activity
                    activity?.SetTag("exception.type", SanitizeTypeName(exception.GetType().Name));
                    activity?.SetTag("exception.message", SanitizeExceptionMessage(exception.Message));
                    activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(exception.Message));
                }

                // Add activity tags
                activity?.SetTag("request_name", SanitizeTypeName(request.GetType().Name));
                activity?.SetTag("request_type", isQuery ? "query" : "command");
                activity?.SetTag("response_type", SanitizeTypeName(typeof(TResponse).Name));
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                if (executedMiddleware.Count > 0)
                {
                    activity?.SetTag("middleware.executed", string.Join(",", executedMiddleware));
                }

                // Increment health check counter
                TelemetryHealthCounter.Add(1, new TagList { { "operation", "send_with_response" } });
            }
        }
    }

    /// <summary>
    /// Sends a stream request through the middleware pipeline to its corresponding handler and returns an async enumerable.
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>An async enumerable of response items</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Debug logging: SendStream operation started
        _logger?.SendStreamOperationStarted(request.GetType().Name);

        _statistics?.IncrementQuery(request.GetType().Name);

        Type requestType = request.GetType();
        Type handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Debug logging: Stream handler resolution
        _logger?.SendStreamHandlerResolution(handlerType.Name, requestType.Name);

        // Create final handler delegate that executes the actual stream handler
        IAsyncEnumerable<TResponse> FinalHandler()
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;

            switch (handlerArray)
            {
                case { Length: 0 }:
                    _logger?.NoHandlerFoundWarning(requestType.Name);
                    throw new InvalidOperationException($"No handler found for stream request type {requestType.Name}");
                case { Length: > 1 }:
                    _logger?.MultipleHandlersFoundWarning(requestType.Name, string.Join(", ", handlerArray.Select(h => h.GetType().Name)));
                    throw new InvalidOperationException($"Multiple handlers found for stream request type {requestType.Name}. Only one handler per request type is allowed.");
            }

            object handler = handlerArray[0];

            // Debug logging: Stream handler found
            _logger?.SendStreamHandlerFound(handler.GetType().Name, requestType.Name);

            MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

            try
            {
                IAsyncEnumerable<TResponse>? result = (IAsyncEnumerable<TResponse>?)method.Invoke(handler, [request, cancellationToken]);
                return result ?? throw new InvalidOperationException($"Handler for {requestType.Name} returned null");
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        // Execute through middleware pipeline using reflection to call the generic method
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecuteStreamPipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType.IsGenericType &&
                m.IsGenericMethodDefinition);

        IAsyncEnumerable<TResponse> baseStream;
        if (executeMethod == null)
        {
            // Fallback to direct execution if pipeline method not found
            baseStream = FinalHandler();
        }
        else
        {
            MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType, typeof(TResponse));
            IAsyncEnumerable<TResponse>? pipelineResult = (IAsyncEnumerable<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (StreamRequestHandlerDelegate<TResponse>)FinalHandler, cancellationToken]);
            baseStream = pipelineResult ?? FinalHandler();
        }

        // Wrap the stream with telemetry instrumentation
        return CreateInstrumentedStream(request, baseStream, cancellationToken);
    }

    /// <summary>
    /// Creates an instrumented stream wrapper that provides comprehensive OpenTelemetry tracking for stream operations.
    /// Includes packet-level metrics for throughput, latency, and performance monitoring.
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request</param>
    /// <param name="baseStream">The underlying stream to instrument</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An instrumented stream that records comprehensive telemetry data</returns>
    private async IAsyncEnumerable<TResponse> CreateInstrumentedStream<TResponse>(
        IStreamRequest<TResponse> request,
        IAsyncEnumerable<TResponse> baseStream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Create the stream context outside try-catch to avoid yield restrictions
        var streamContext = new StreamTelemetryContext<TResponse>(request, _telemetryOptions);

        await foreach (var item in InstrumentedStreamEnumeration(baseStream, streamContext, cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Internal enumeration method that handles the telemetry instrumentation.
    /// Separated to avoid yield return in try-catch restrictions.
    /// </summary>
    private async IAsyncEnumerable<TResponse> InstrumentedStreamEnumeration<TResponse>(
        IAsyncEnumerable<TResponse> baseStream,
        StreamTelemetryContext<TResponse> context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"Mediator.SendStream:{context.RequestTypeName}") : null;

        // Ensure the activity is started and marked as active for proper telemetry propagation
        if (activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
            // Set initial tags immediately to ensure they're captured
            activity.SetTag("request_name", context.RequestTypeName);
            activity.SetTag("request_type", "stream");
            activity.SetTag("response_type", context.ResponseTypeName);
            activity.SetTag("mediator.operation", "SendStream");
            activity.SetTag("otel.library.name", "Blazing.Mediator");
            activity.SetTag("otel.library.version", typeof(Mediator).Assembly.GetName().Version?.ToString() ?? "1.0.0");

            // Add streaming-specific semantic convention tags
            activity.SetTag("stream.type", "response");
            activity.SetTag("stream.packet_level_telemetry", IsPacketLevelTelemetryEnabled);
            activity.SetTag("stream.batch_size", GetPacketTelemetryBatchSize);
        }

        var streamStopwatch = Stopwatch.StartNew();
        var lastItemTime = streamStopwatch.ElapsedMilliseconds;
        Exception? exception = null;

        // Initialize telemetry context
        context.Initialize(activity, _serviceProvider, _pipelineBuilder, _statistics);

        IAsyncEnumerator<TResponse>? enumerator = null;
        try
        {
            enumerator = baseStream.GetAsyncEnumerator(cancellationToken);

            // Add an initial event to mark the start of streaming
            if (activity != null)
            {
                activity.AddEvent(new ActivityEvent("stream_started", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    ["request_name"] = context.RequestTypeName,
                    ["response_type"] = context.ResponseTypeName,
                    ["activity_id"] = activity.Id ?? "unknown",
                    ["stream.operation"] = "start"
                }));
            }

            // Enumerate items with telemetry tracking
            while (true)
            {
                bool hasNext;
                var packetStopwatch = Stopwatch.StartNew();

                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    context.RecordError(activity, ex);
                    throw;
                }

                if (!hasNext) break;

                packetStopwatch.Stop();
                var currentTime = streamStopwatch.ElapsedMilliseconds;
                var item = enumerator.Current;

                // Record packet-level metrics with enhanced telemetry
                context.RecordPacket(currentTime, lastItemTime, item, packetStopwatch.Elapsed.TotalMilliseconds, cancellationToken);
                lastItemTime = currentTime;

                yield return item;
            }

            // Stream completed successfully
            context.RecordSuccess(activity);

            // Add completion event with comprehensive summary
            if (activity != null)
            {
                activity.AddEvent(new ActivityEvent("stream_completed", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    ["items_processed"] = context.ItemCount,
                    ["duration_ms"] = streamStopwatch.ElapsedMilliseconds,
                    ["activity_id"] = activity.Id ?? "unknown",
                    ["stream.operation"] = "complete",
                    ["stream.throughput_items_per_sec"] = context.ItemCount / Math.Max(streamStopwatch.Elapsed.TotalSeconds, 0.001),
                    ["stream.average_inter_packet_time_ms"] = context.AverageInterPacketTime
                }));
            }
        }
        finally
        {
            if (enumerator != null)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            streamStopwatch.Stop();
            context.RecordFinalMetrics(activity, streamStopwatch.Elapsed, exception);
        }
    }

    #region Notification Methods

    /// <summary>
    /// Publishes a notification to all subscribers following the observer pattern.
    /// Publishers blindly send notifications without caring about recipients.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        _logger?.PublishOperationStarted(typeof(TNotification).Name, IsTelemetryEnabled);

        using var activity = TelemetryEnabled ? ActivitySource.StartActivity($"Mediator.Publish.{typeof(TNotification).Name}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        List<string> executedMiddleware = [];
        List<string> allMiddleware = [];
        Exception? exception = null;
        int subscriberCount = 0;
        var subscriberResults = new List<(string SubscriberType, bool Success, double DurationMs, string? ExceptionType, string? ExceptionMessage)>();

        try
        {
            // Track notification statistics
            _statistics?.IncrementNotification(typeof(TNotification).Name);

            // Get all registered notification middleware (types and order)
            var pipelineInspector = _notificationPipelineBuilder as INotificationMiddlewarePipelineInspector;
            var middlewareInfo = pipelineInspector?.GetDetailedMiddlewareInfo(_serviceProvider);
            if (middlewareInfo != null)
            {
                allMiddleware = middlewareInfo.OrderBy(m => m.Order).Select(m => SanitizeMiddlewareName(m.Type.Name)).ToList();
                activity?.SetTag("notification_middleware.pipeline", string.Join(",", allMiddleware));
            }

            // Execute through notification middleware
            async Task SubscriberHandler(TNotification n, CancellationToken ct)
            {
                // Find all subscribers (specific and generic)
                var subscribers = new List<INotificationSubscriber<TNotification>>();
                if (_specificSubscribers.TryGetValue(typeof(TNotification), out var specific))
                {
                    subscribers.AddRange(specific.OfType<INotificationSubscriber<TNotification>>());
                }

                // Add generic subscribers that can handle any notification
                var genericSubscriberList = _genericSubscribers.ToList();

                subscriberCount = subscribers.Count + genericSubscriberList.Count;
                _logger?.PublishSubscriberResolution(subscriberCount, typeof(TNotification).Name);
                var subscriberExceptions = new List<Exception>();

                // Process specific subscribers
                foreach (var subscriber in subscribers)
                {
                    var subType = SanitizeTypeName(subscriber.GetType().Name);
                    _logger?.PublishSubscriberProcessing(subType, typeof(TNotification).Name);
                    var subStopwatch = Stopwatch.StartNew();

                    try
                    {
                        await subscriber.OnNotification(n, ct).ConfigureAwait(false);
                        if (TelemetryEnabled)
                        {
                            var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", subType } };
                            PublishSubscriberSuccessCounter.Add(1, successTags);
                        }

                        subscriberResults.Add((subType, true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
                        _logger?.PublishSubscriberCompleted(subType, typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, true);
                    }
                    catch (Exception ex)
                    {
                        subscriberExceptions.Add(ex);
                        if (TelemetryEnabled)
                        {
                            var failureTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", subType }, { "exception.type", SanitizeTypeName(ex.GetType().Name) }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                            PublishSubscriberFailureCounter.Add(1, failureTags);
                        }

                        subscriberResults.Add((subType, false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
                        _logger?.PublishSubscriberCompleted(subType, typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, false);
                        // Don't throw immediately - continue with other subscribers
                    }
                    finally
                    {
                        subStopwatch.Stop();
                        if (TelemetryEnabled)
                        {
                            var durationTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", subType } };
                            PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
                        }
                    }
                }

                // Process generic subscribers
                foreach (var genericSubscriber in genericSubscriberList)
                {
                    var subType = SanitizeTypeName(genericSubscriber.GetType().Name);
                    _logger?.PublishSubscriberProcessing($"{subType}(Generic)", typeof(TNotification).Name);
                    var subStopwatch = Stopwatch.StartNew();

                    try
                    {
                        await genericSubscriber.OnNotification(n, ct).ConfigureAwait(false);
                        if (TelemetryEnabled)
                        {
                            var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", $"{subType}(Generic)" } };
                            PublishSubscriberSuccessCounter.Add(1, successTags);
                        }

                        subscriberResults.Add(($"{subType}(Generic)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
                        _logger?.PublishSubscriberCompleted($"{subType}(Generic)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, true);
                    }
                    catch (Exception ex)
                    {
                        subscriberExceptions.Add(ex);
                        if (TelemetryEnabled)
                        {
                            var failureTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", $"{subType}(Generic)" }, { "exception.type", SanitizeTypeName(ex.GetType().Name) }, { "exception.message", SanitizeExceptionMessage(ex.Message) } };
                            PublishSubscriberFailureCounter.Add(1, failureTags);
                        }

                        subscriberResults.Add(($"{subType}(Generic)", false, subStopwatch.Elapsed.TotalMilliseconds, SanitizeTypeName(ex.GetType().Name), SanitizeExceptionMessage(ex.Message)));
                        _logger?.PublishSubscriberCompleted($"{subType}(Generic)", typeof(TNotification).Name, subStopwatch.Elapsed.TotalMilliseconds, false);
                        // Don't throw immediately - continue with other subscribers
                    }
                    finally
                    {
                        subStopwatch.Stop();
                        if (TelemetryEnabled)
                        {
                            var durationTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", $"{subType}(Generic)" } };
                            PublishSubscriberDurationHistogram.Record(subStopwatch.Elapsed.TotalMilliseconds, durationTags);
                        }
                    }
                }

                // After all subscribers have been called, throw the first exception if any occurred
                if (subscriberExceptions.Count > 0)
                {
                    throw subscriberExceptions[0];
                }
            }

            // Execute through middleware pipeline
            await _notificationPipelineBuilder.ExecutePipeline(notification, _serviceProvider, SubscriberHandler, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            exception = ex;
            if (!TelemetryEnabled)
            {
                throw;
            }

            activity?.SetStatus(ActivityStatusCode.Error);

            var failureTags = new TagList
            {
                { "notification_name", SanitizeTypeName(typeof(TNotification).Name) },
                { "exception.type", SanitizeTypeName(ex.GetType().Name) },
                { "exception.message", SanitizeExceptionMessage(ex.Message) }
            };
            PublishFailureCounter.Add(1, failureTags);

            activity?.SetTag("exception.type", SanitizeTypeName(ex.GetType().Name));
            activity?.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
            activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(ex.Message));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            if (TelemetryEnabled)
            {
                var tags = new TagList
                {
                    { "notification_name", SanitizeTypeName(typeof(TNotification).Name) },
                    { "subscriber_count", subscriberCount }
                };

                if (executedMiddleware.Count > 0)
                {
                    tags.Add("notification_middleware.executed", string.Join(",", executedMiddleware));
                }

                PublishDurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);

                // Add activity tags
                activity?.SetTag("notification_name", SanitizeTypeName(typeof(TNotification).Name));
                activity?.SetTag("notification_middleware.executed", string.Join(",", executedMiddleware));
                activity?.SetTag("notification_middleware.pipeline", string.Join(",", allMiddleware));
                activity?.SetTag("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                activity?.SetTag("subscriber_count", subscriberCount);

                if (exception == null)
                {
                    PublishSuccessCounter.Add(1, tags);
                    // Ensure successful completion is marked with Ok status
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }

                // Add per-subscriber results as activity events
                foreach (var result in subscriberResults)
                {
                    activity?.AddEvent(new ActivityEvent($"subscriber:{result.SubscriberType}", default, new ActivityTagsCollection
                    {
                        ["subscriber_type"] = result.SubscriberType,
                        ["success"] = result.Success,
                        ["duration_ms"] = result.DurationMs,
                        ["exception_type"] = result.ExceptionType,
                        ["exception_message"] = result.ExceptionMessage
                    }));
                }

                // Log completion
                _logger?.PublishOperationCompleted(typeof(TNotification).Name, stopwatch.Elapsed.TotalMilliseconds, exception == null, subscriberCount);

                // Increment health check counter
                TelemetryHealthCounter.Add(1, new TagList { { "operation", "publish" } });
            }
        }
    }

    /// <summary>
    /// Subscribe to notifications of a specific type.
    /// Subscribers actively choose to listen to notifications they're interested in.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to subscribe to</typeparam>
    /// <param name="subscriber">The subscriber that will receive notifications</param>
    public void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        _specificSubscribers.AddOrUpdate(
            typeof(TNotification),
            [subscriber],
            (_, existing) =>
            {
                existing.Add(subscriber);
                return existing;
            });
    }

    /// <summary>
    /// Subscribe to all notifications (generic/broadcast).
    /// Subscribers actively choose to listen to all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber that will receive all notifications</param>
    public void Subscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        _genericSubscribers.Add(subscriber);
    }

    /// <summary>
    /// Unsubscribe from notifications of a specific type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to unsubscribe from</typeparam>
    /// <param name="subscriber">The subscriber to remove</param>
    public void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        if (!_specificSubscribers.TryGetValue(typeof(TNotification), out var subscribers))
        {
            return;
        }

        // Create new bag without the subscriber
        var newSubscribers = new ConcurrentBag<object>();
        foreach (var existing in from existing in subscribers
                                 where !ReferenceEquals(existing, subscriber)
                                 select existing)
        {
            newSubscribers.Add(existing);
        }

        if (newSubscribers.IsEmpty)
        {
            _specificSubscribers.TryRemove(typeof(TNotification), out _);
        }
        else
        {
            _specificSubscribers.TryUpdate(typeof(TNotification), newSubscribers, subscribers);
        }
    }

    /// <summary>
    /// Unsubscribe from all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove from all notifications</param>
    public void Unsubscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        // Remove from generic subscribers
        var newGenericSubscribers = new ConcurrentBag<INotificationSubscriber>();
        foreach (var existing in _genericSubscribers
                     .Where(existing => !ReferenceEquals(existing, subscriber)))
        {
            newGenericSubscribers.Add(existing);
        }

        // Replace the entire bag
        _genericSubscribers.Clear();
        foreach (var sub in newGenericSubscribers)
        {
            _genericSubscribers.Add(sub);
        }
    }

    #endregion

    #region Telemetry Helper Methods

    /// <summary>
    /// Efficiently determines if a request is a query or command with minimal performance impact.
    /// First checks primary interfaces, then falls back to name-based detection using ReadOnlySpan.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The request to analyze</param>
    /// <returns>True if it's a query, false if it's a command</returns>
    private static bool DetermineIfQuery<TResponse>(IRequest<TResponse> request)
    {
        Type requestType = request.GetType();

        // Fast path: Check if request directly implements IQuery<TResponse>
        if (request is IQuery<TResponse>)
        {
            return true;
        }

        // Fast path: Check if request directly implements ICommand<TResponse>
        if (request is ICommand<TResponse>)
        {
            return false;
        }

        // Fallback: Check type name suffix using ReadOnlySpan for performance
        ReadOnlySpan<char> typeName = requestType.Name.AsSpan();

        // Check for "Query" suffix (case-insensitive)
        if (typeName.Length >= 5 &&
            typeName[^5..].Equals("Query".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check for "Command" suffix (case-insensitive)
        if (typeName.Length >= 7 &&
            typeName[^7..].Equals("Command".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Default to query if no clear indication (maintains backward compatibility)
        return true;
    }

    /// <summary>
    /// Sanitizes type names by removing sensitive information and generic suffixes.
    /// </summary>
    /// <param name="typeName">The type name to sanitize.</param>
    /// <returns>A sanitized type name safe for telemetry.</returns>
    private string SanitizeTypeName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return "unknown";
        }

        // Remove generic type suffix (e.g., "`1", "`2")
        var backtickIndex = typeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            typeName = typeName[..backtickIndex];
        }

        // Remove sensitive patterns based on configuration
        foreach (var pattern in SensitiveDataPatterns)
        {
            if (typeName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                typeName = typeName.Replace(pattern, "***", StringComparison.OrdinalIgnoreCase);
            }
        }

        return typeName;
    }

    /// <summary>
    /// Sanitizes middleware names for telemetry.
    /// </summary>
    /// <param name="middlewareName">The middleware name to sanitize.</param>
    /// <returns>A sanitized middleware name safe for telemetry.</returns>
    private string SanitizeMiddlewareName(string middlewareName)
    {
        return SanitizeTypeName(middlewareName);
    }

    /// <summary>
    /// Sanitizes exception messages by removing sensitive information.
    /// </summary>
    /// <param name="message">The exception message to sanitize.</param>
    /// <returns>A sanitized exception message safe for telemetry.</returns>
    private string SanitizeExceptionMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return "unknown_error";

        // Remove potential sensitive data patterns based on configuration
        var sanitized = message;

        foreach (string pattern in SensitiveDataPatterns.Where(pattern => sanitized.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
        {
            sanitized = $"{pattern}_error";
            break; // Stop at first match to avoid over-sanitization
        }

        // Remove SQL connection strings
        if (sanitized.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            sanitized = "connection_error";
        }

        // Remove file paths
        if (sanitized.Contains(":\\") || sanitized.Contains("/"))
        {
            sanitized = "file_path_error";
        }

        // Limit length based on configuration
        return sanitized.Length > MaxExceptionMessageLength ?
            sanitized[..MaxExceptionMessageLength] + "..." : sanitized;
    }

    /// <summary>
    /// Sanitizes stack traces by removing file paths and limiting content.
    /// </summary>
    /// <param name="stackTrace">The stack trace to sanitize.</param>
    /// <returns>A sanitized stack trace safe for telemetry.</returns>
    private string? SanitizeStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return null;

        // For telemetry, we only want the first few lines without file paths
        var lines = stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sanitizedLines = new List<string>();

        for (int i = 0; i < Math.Min(MaxStackTraceLines, lines.Length); i++)
        {
            var line = lines[i].Trim();

            // Remove file paths
            var inIndex = line.LastIndexOf(" in ", StringComparison.Ordinal);
            if (inIndex > 0)
            {
                line = line[..inIndex];
            }

            sanitizedLines.Add(line);
        }

        return string.Join(" | ", sanitizedLines);
    }

    /// <summary>
    /// Checks if middleware is applicable to the given request and response types.
    /// </summary>
    /// <param name="middlewareType">The middleware type.</param>
    /// <param name="requestType">The request type.</param>
    /// <param name="responseType">The response type (optional).</param>
    /// <returns>True if the middleware is applicable to the request type.</returns>
    private static bool IsMiddlewareApplicable(Type middlewareType, Type requestType, Type? responseType = null)
    {
        try
        {
            if (middlewareType.IsGenericTypeDefinition)
            {
                var genericParams = middlewareType.GetGenericTypeDefinition().GetGenericArguments();

                if (genericParams.Length == 1 && responseType == null)
                {
                    // Single parameter middleware for void requests
                    try
                    {
                        var concreteType = middlewareType.MakeGenericType(requestType);
                        return typeof(IRequestMiddleware<>).MakeGenericType(requestType).IsAssignableFrom(concreteType);
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (genericParams.Length == 2 && responseType != null)
                {
                    // Two parameter middleware for requests with responses
                    try
                    {
                        var concreteType = middlewareType.MakeGenericType(requestType, responseType);
                        return typeof(IRequestMiddleware<,>).MakeGenericType(requestType, responseType).IsAssignableFrom(concreteType);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Non-generic middleware - check if it implements the correct interface
                return responseType == null
                    ? typeof(IRequestMiddleware<>).MakeGenericType(requestType).IsAssignableFrom(middlewareType)
                    : typeof(IRequestMiddleware<,>).MakeGenericType(requestType, responseType).IsAssignableFrom(middlewareType);
            }
        }
        catch
        {
            // If any reflection fails, assume not applicable
        }

        return false;
    }

    /// <summary>
    /// Gets telemetry health status.
    /// </summary>
    /// <returns>True if telemetry is enabled and working.</returns>
    public static bool GetTelemetryHealth()
    {
        try
        {
            if (!TelemetryEnabled)
                return false;

            // Test if we can record a metric
            TelemetryHealthCounter.Add(1, new TagList { { "operation", "health_check" } });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets telemetry health status using instance configuration.
    /// </summary>
    /// <returns>True if telemetry is enabled and working.</returns>
    public bool GetInstanceTelemetryHealth()
    {
        try
        {
            if (!IsTelemetryEnabled || !AreHealthChecksEnabled)
                return false;

            // Test if we can record a metric
            TelemetryHealthCounter.Add(1, new TagList { { "operation", "instance_health_check" } });
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Enhanced Statistics Integration

    /// <summary>
    /// Enhanced middleware execution with detailed tracking for statistics.
    /// </summary>
    private async Task ExecuteWithMiddlewareTracking<TRequest>(
        TRequest request,
        Func<Task> finalHandler,
        List<string> executedMiddleware,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        // Create the proper delegate
        async Task HandlerDelegate() => await finalHandler().ConfigureAwait(false);

        // Execute through middleware pipeline
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecutePipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType == typeof(RequestHandlerDelegate) &&
                m.IsGenericMethodDefinition);

        if (executeMethod == null)
        {
            await finalHandler().ConfigureAwait(false);
            return;
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(typeof(TRequest));
        Task? pipelineTask = (Task?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate)HandlerDelegate, cancellationToken]);
        if (pipelineTask != null)
        {
            await pipelineTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Enhanced middleware execution with detailed tracking for statistics (with response).
    /// </summary>
    private async Task<TResponse> ExecuteWithMiddlewareTracking<TRequest, TResponse>(
        TRequest request,
        Func<Task<TResponse>> finalHandler,
        List<string> executedMiddleware,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        // Create the proper delegate
        async Task<TResponse> HandlerDelegate() => await finalHandler().ConfigureAwait(false);

        // Execute through middleware pipeline
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecutePipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType.IsGenericType &&
                m.IsGenericMethodDefinition);

        if (executeMethod == null)
        {
            return await finalHandler().ConfigureAwait(false);
        }

        MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(typeof(TRequest), typeof(TResponse));
        Task<TResponse>? pipelineTask = (Task<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate<TResponse>)HandlerDelegate, cancellationToken]);
        if (pipelineTask != null)
        {
            return await pipelineTask.ConfigureAwait(false);
        }
        return await finalHandler().ConfigureAwait(false);
    }

    /// <summary>
    /// Helper method to check if performance counters are enabled in statistics options.
    /// </summary>
    private bool HasPerformanceCountersEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is Configuration.StatisticsOptions options && options.EnablePerformanceCounters;
    }

    /// <summary>
    /// Helper method to check if detailed analysis is enabled in statistics options.
    /// </summary>
    private bool HasDetailedAnalysisEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is Configuration.StatisticsOptions options && options.EnableDetailedAnalysis;
    }

    /// <summary>
    /// Helper method to check if middleware metrics are enabled in statistics options.
    /// </summary>
    private bool HasMiddlewareMetricsEnabled()
    {
        return _statistics?.GetType()
            .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(_statistics) is Configuration.StatisticsOptions options && options.EnableMiddlewareMetrics;
    }

    #endregion
}
