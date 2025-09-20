using Blazing.Mediator.Statistics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Blazing.Mediator;

/// <summary>
/// Implementation of the Mediator pattern that dispatches requests to their corresponding handlers.
/// </summary>
/// <remarks>
/// The Mediator class serves as a centralized request dispatcher that decouples the request sender 
/// from the request handler. It uses dependency injection to resolve handlers at runtime and 
/// supports both void and typed responses.
/// </remarks>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMiddlewarePipelineBuilder _pipelineBuilder;
    private readonly INotificationPipelineBuilder _notificationPipelineBuilder;
    private readonly MediatorStatistics? _statistics;

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
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider, pipelineBuilder, or notificationPipelineBuilder is null.</exception>
    public Mediator(IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder, INotificationPipelineBuilder notificationPipelineBuilder, MediatorStatistics? statistics)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
        _notificationPipelineBuilder = notificationPipelineBuilder ?? throw new ArgumentNullException(nameof(notificationPipelineBuilder));
        _statistics = statistics; // Statistics can be null if tracking is disabled
    }

    /// <summary>
    /// Sends a command request through the middleware pipeline to its corresponding handler.
    /// </summary>
    /// <param name="request">The command request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = TelemetryEnabled ? ActivitySource.StartActivity($"Mediator.Send:{request.GetType().Name}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();

        try
        {
            _statistics?.IncrementCommand(request.GetType().Name);
            Type requestType = request.GetType();
            Type handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);

            // Get middleware pipeline information for telemetry
            if (TelemetryEnabled && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
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
                        throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                    case { Length: > 1 }:
                        throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
                }
                object handler = handlerArray[0];
                MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");
                try
                {
                    if (TelemetryEnabled)
                    {
                        activity?.SetTag("handler.type", SanitizeTypeName(handler.GetType().Name));
                    }

                    Task? task = (Task?)method.Invoke(handler, [request, cancellationToken]);
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

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
                await FinalHandler();
                return;
            }
            MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType);
            Task? pipelineTask = (Task?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate)FinalHandler, cancellationToken]);
            if (pipelineTask != null)
            {
                await pipelineTask;
            }
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
            if (TelemetryEnabled)
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
                    tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
                    SendFailureCounter.Add(1, tags);

                    // Add exception details to activity
                    activity?.SetTag("exception.type", SanitizeTypeName(exception.GetType().Name));
                    activity?.SetTag("exception.message", SanitizeExceptionMessage(exception.Message));
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
        using var activity = TelemetryEnabled ? ActivitySource.StartActivity($"Mediator.Send:{request.GetType().Name}") : null;
        activity?.SetStatus(ActivityStatusCode.Ok);

        var stopwatch = Stopwatch.StartNew();
        Exception? exception = null;
        var executedMiddleware = new List<string>();

        try
        {
            if (_statistics != null)
            {
                bool isQuery = DetermineIfQuery(request);
                if (isQuery)
                {
                    _statistics.IncrementQuery(request.GetType().Name);
                }
                else
                {
                    _statistics.IncrementCommand(request.GetType().Name);
                }
            }

            Type requestType = request.GetType();
            Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Get middleware pipeline information for telemetry
            if (TelemetryEnabled && _pipelineBuilder is IMiddlewarePipelineInspector inspector)
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
                        throw new InvalidOperationException($"No handler found for request type {requestType.Name}");
                    case { Length: > 1 }:
                        throw new InvalidOperationException($"Multiple handlers found for request type {requestType.Name}. Only one handler per request type is allowed.");
                }
                object handler = handlerArray[0];
                MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");
                try
                {
                    if (TelemetryEnabled)
                    {
                        activity?.SetTag("handler.type", SanitizeTypeName(handler.GetType().Name));
                        activity?.SetTag("response.type", SanitizeTypeName(typeof(TResponse).Name));
                    }

                    Task<TResponse>? task = (Task<TResponse>?)method.Invoke(handler, [request, cancellationToken]);
                    if (task != null)
                    {
                        return await task;
                    }
                    throw new InvalidOperationException($"Handler for {requestType.Name} returned null");
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
            }

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
                return await FinalHandler();
            }
            MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType, typeof(TResponse));
            Task<TResponse>? pipelineTask = (Task<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (RequestHandlerDelegate<TResponse>)FinalHandler, cancellationToken]);
            if (pipelineTask != null)
            {
                return await pipelineTask;
            }
            return await FinalHandler();
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
            if (TelemetryEnabled)
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
        
        _statistics?.IncrementQuery(request.GetType().Name);
        
        Type requestType = request.GetType();
        Type handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Create final handler delegate that executes the actual stream handler
        IAsyncEnumerable<TResponse> FinalHandler()
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;

            switch (handlerArray)
            {
                case { Length: 0 }:
                    throw new InvalidOperationException($"No handler found for stream request type {requestType.Name}");
                case { Length: > 1 }:
                    throw new InvalidOperationException($"Multiple handlers found for stream request type {requestType.Name}. Only one handler per request type is allowed.");
            }

            object handler = handlerArray[0];
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
        var streamContext = new StreamTelemetryContext<TResponse>(request);
        
        await foreach (var item in InstrumentedStreamEnumeration(baseStream, streamContext, cancellationToken))
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
        using var activity = TelemetryEnabled ? ActivitySource.StartActivity($"Mediator.SendStream:{context.RequestTypeName}") : null;
        
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
            activity.SetTag("stream.packet_level_telemetry", PacketLevelTelemetryEnabled);
            activity.SetTag("stream.batch_size", PacketTelemetryBatchSize);
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
                    hasNext = await enumerator.MoveNextAsync();
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
                await enumerator.DisposeAsync();
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

            // Execute through notification middleware pipeline
            async Task SubscriberHandler(TNotification n, CancellationToken ct)
            {
                // Find all subscribers (specific and generic)
                var subscribers = new List<INotificationSubscriber<TNotification>>();
                if (_specificSubscribers.TryGetValue(typeof(TNotification), out var specific))
                {
                    subscribers.AddRange(specific.OfType<INotificationSubscriber<TNotification>>());
                }

                // Add generic subscribers that can handle any notification
                var genericSubscriberList = new List<INotificationSubscriber>();
                foreach (var genericSubscriber in _genericSubscribers)
                {
                    genericSubscriberList.Add(genericSubscriber);
                }

                subscriberCount = subscribers.Count + genericSubscriberList.Count;
                var subscriberExceptions = new List<Exception>();

                // Process specific subscribers
                foreach (var subscriber in subscribers)
                {
                    var subType = SanitizeTypeName(subscriber.GetType().Name);
                    var subStopwatch = Stopwatch.StartNew();

                    try
                    {
                        await subscriber.OnNotification(n, ct);
                        if (TelemetryEnabled)
                        {
                            var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", subType } };
                            PublishSubscriberSuccessCounter.Add(1, successTags);
                        }

                        subscriberResults.Add((subType, true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
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
                    var subStopwatch = Stopwatch.StartNew();

                    try
                    {
                        await genericSubscriber.OnNotification(n, ct);
                        if (TelemetryEnabled)
                        {
                            var successTags = new TagList { { "notification_name", SanitizeTypeName(typeof(TNotification).Name) }, { "subscriber_type", $"{subType}(Generic)" } };
                            PublishSubscriberSuccessCounter.Add(1, successTags);
                        }

                        subscriberResults.Add(($"{subType}(Generic)", true, subStopwatch.Elapsed.TotalMilliseconds, null, null));
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
            await _notificationPipelineBuilder.ExecutePipeline(notification, _serviceProvider, SubscriberHandler, cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            if (TelemetryEnabled)
            {
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
            }
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

    /// <summary>
    /// Safely invokes a subscriber method, catching and logging any exceptions.
    /// Ensures that exceptions in one subscriber don't affect other subscribers.
    /// </summary>
    /// <param name="subscriberAction">The subscriber action to invoke</param>
    /// <returns>A task representing the operation</returns>
    private static async Task SafeInvokeSubscriber(Func<Task> subscriberAction)
    {
        try
        {
            await subscriberAction();
        }
        catch (Exception ex)
        {
            // Log the exception but don't let it propagate to other subscribers
            Console.WriteLine($"Exception in notification subscriber: {ex.Message}");
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
    private static string SanitizeTypeName(string typeName)
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

        // Remove sensitive patterns
        typeName = typeName.Replace("Password", "***")
                          .Replace("Secret", "***")
                          .Replace("Token", "***")
                          .Replace("Key", "***")
                          .Replace("Auth", "***")
                          .Replace("SensitiveData", "***");

        return typeName;
    }

    /// <summary>
    /// Sanitizes middleware names for telemetry.
    /// </summary>
    /// <param name="middlewareName">The middleware name to sanitize.</param>
    /// <returns>A sanitized middleware name safe for telemetry.</returns>
    private static string SanitizeMiddlewareName(string middlewareName)
    {
        return SanitizeTypeName(middlewareName);
    }

    /// <summary>
    /// Sanitizes exception messages by removing sensitive information.
    /// </summary>
    /// <param name="message">The exception message to sanitize.</param>
    /// <returns>A sanitized exception message safe for telemetry.</returns>
    private static string SanitizeExceptionMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return "unknown_error";

        // Remove potential sensitive data patterns
        var sanitized = message;

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

        // Remove tokens, passwords, keys
        var sensitivePatterns = new[] { "password", "token", "secret", "key", "auth" };
        foreach (var pattern in sensitivePatterns)
        {
            if (sanitized.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                sanitized = "sensitive_data_error";
                break;
            }
        }

        // Limit length to prevent log flooding
        return sanitized.Length > 200 ? sanitized[..200] + "..." : sanitized;
    }

    /// <summary>
    /// Sanitizes stack traces by removing file paths and limiting content.
    /// </summary>
    /// <param name="stackTrace">The stack trace to sanitize.</param>
    /// <returns>A sanitized stack trace safe for telemetry.</returns>
    private static string? SanitizeStackTrace(string? stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return null;

        // For telemetry, we only want the first few lines without file paths
        var lines = stackTrace.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var sanitizedLines = new List<string>();

        for (int i = 0; i < Math.Min(3, lines.Length); i++) // Only first 3 lines
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
    /// Checks if middleware is applicable to a request type.
    /// </summary>
    /// <param name="middlewareType">The middleware type.</param>
    /// <param name="requestType">The request type.</param>
    /// <param name="responseType">The response type (optional).</param>
    /// <returns>True if the middleware is applicable to the request type.</returns>
    private static bool IsMiddlewareApplicable(Type middlewareType, Type requestType, Type? responseType = null)
    {
        try
        {
            // Handle generic middleware
            if (middlewareType.IsGenericTypeDefinition)
            {
                var genericParams = middlewareType.GetGenericArguments();

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

    #endregion
}

/// <summary>
/// Telemetry context for streaming operations that tracks comprehensive packet-level metrics.
/// </summary>
/// <typeparam name="TResponse">The type of response items in the stream</typeparam>
internal sealed class StreamTelemetryContext<TResponse>(IStreamRequest<TResponse> request)
{
    private readonly IStreamRequest<TResponse> _request = request ?? throw new ArgumentNullException(nameof(request));
    private readonly List<double> _interPacketTimes = new();
    private readonly List<double> _packetProcessingTimes = new();
    private int _itemCount;
    private TimeSpan _timeToFirstByte;
    private bool _firstItemReceived;
    private Activity? _activity; // Store reference to the activity
    private long _totalPacketProcessingTime;

    public string RequestTypeName { get; } = SanitizeTypeName(request.GetType().Name);
    public string ResponseTypeName { get; } = SanitizeTypeName(typeof(TResponse).Name);
    public int ItemCount => _itemCount; // Expose item count for telemetry
    public double AverageInterPacketTime => _interPacketTimes.Count > 0 ? _interPacketTimes.Average() : 0;

    /// <summary>
    /// Initializes the telemetry context with activity and service information.
    /// </summary>
    public void Initialize(Activity? activity, IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder, MediatorStatistics? statistics)
    {
        if (!Mediator.TelemetryEnabled) return;

        _activity = activity; // Store the activity reference
        statistics?.IncrementQuery(_request.GetType().Name);

        if (activity != null)
        {
            activity.SetTag("request_name", RequestTypeName);
            activity.SetTag("request_type", "stream");
            activity.SetTag("response_type", ResponseTypeName);

            // Get middleware pipeline information
            if (pipelineBuilder is IMiddlewarePipelineInspector inspector)
            {
                var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);
                var allMiddleware = middlewareInfo
                    .Where(m => IsMiddlewareApplicable(m.Type, _request.GetType(), typeof(TResponse)))
                    .OrderBy(m => m.Order)
                    .Select(m => SanitizeMiddlewareName(m.Type.Name))
                    .ToList();

                activity.SetTag("middleware.pipeline", string.Join(",", allMiddleware));
            }

            // Get handler information
            var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(_request.GetType(), typeof(TResponse));
            var handlers = serviceProvider.GetServices(handlerType);
            var handler = handlers.FirstOrDefault();
            if (handler != null)
            {
                activity.SetTag("handler.type", SanitizeTypeName(handler.GetType().Name));
            }
        }
    }

    /// <summary>
    /// Records packet-level metrics for each item in the stream with enhanced telemetry support.
    /// </summary>
    public void RecordPacket(long currentTime, long lastItemTime, TResponse item, double packetProcessingTimeMs, CancellationToken cancellationToken = default)
    {
        if (!Mediator.TelemetryEnabled) return;

        _itemCount++;
        double interPacketTime = 0;

        // Record Time to First Byte (TTFB)
        if (!_firstItemReceived)
        {
            _timeToFirstByte = TimeSpan.FromMilliseconds(currentTime);
            _firstItemReceived = true;
        }
        else
        {
            // Record inter-packet timing
            interPacketTime = currentTime - lastItemTime;
            _interPacketTimes.Add(interPacketTime);
        }

        // Track packet processing time
        _packetProcessingTimes.Add(packetProcessingTimeMs);
        _totalPacketProcessingTime += (long)packetProcessingTimeMs;

        // Record packet-level metrics
        var packetTags = new TagList
        {
            { "request_name", RequestTypeName },
            { "response_type", ResponseTypeName },
            { "packet_number", _itemCount }
        };

        Mediator.StreamPacketCounter.Add(1, packetTags);
        Mediator.StreamPacketProcessingTimeHistogram.Record(packetProcessingTimeMs, packetTags);
        
        if (interPacketTime > 0)
        {
            Mediator.StreamInterPacketTimeHistogram.Record(interPacketTime, packetTags);
        }

        // Create child span for packet if packet-level telemetry is enabled
        if (Mediator.PacketLevelTelemetryEnabled && _activity != null)
        {
            using var packetActivity = Mediator.ActivitySource.StartActivity($"Mediator.SendStream:{RequestTypeName}.packet_{_itemCount}");
            if (packetActivity != null)
            {
                packetActivity.SetTag("packet.number", _itemCount);
                packetActivity.SetTag("packet.timestamp_ms", currentTime);
                packetActivity.SetTag("packet.processing_time_ms", packetProcessingTimeMs);
                packetActivity.SetTag("packet.inter_packet_time_ms", interPacketTime);
                packetActivity.SetTag("packet.is_first", _itemCount == 1);
                packetActivity.SetTag("stream.total_packets", _itemCount);
                packetActivity.SetTag("request_name", RequestTypeName);
                packetActivity.SetTag("request_type", "stream_packet");
                packetActivity.SetTag("mediator.operation", "SendStreamPacket");
                packetActivity.SetTag("mediator.request_type", "stream_packet");
                packetActivity.SetTag("otel.library.name", "Blazing.Mediator");
                packetActivity.SetTag("otel.library.version", typeof(Mediator).Assembly.GetName().Version?.ToString() ?? "1.0.0");
                
                // Add packet size if available (attempt to serialize for size estimation)
                try
                {
                    if (item is string str)
                    {
                        packetActivity.SetTag("packet.size_bytes", System.Text.Encoding.UTF8.GetByteCount(str));
                    }
                    else if (item != null)
                    {
                        // Rough estimation based on type
                        var typeName = item.GetType().Name;
                        packetActivity.SetTag("packet.type", SanitizeTypeName(typeName));
                    }
                }
                catch
                {
                    // Ignore errors in size calculation
                }
                
                // Ensure the packet span has a minimum duration to avoid being filtered out
                Task.Delay(1, cancellationToken).Wait(cancellationToken);
                packetActivity.SetStatus(ActivityStatusCode.Ok);
            }
        }
        
        // Add batched activity events for better performance
        var shouldCreateEvent = Mediator.PacketTelemetryBatchSize <= 1 || 
                               (_itemCount % Mediator.PacketTelemetryBatchSize == 0) || 
                               _itemCount == 1;
        
        if (shouldCreateEvent && _activity != null)
        {
            if (Mediator.PacketTelemetryBatchSize > 1 && _itemCount > 1)
            {
                // Batched event
                var batchStart = Math.Max(1, _itemCount - Mediator.PacketTelemetryBatchSize + 1);
                var recentInterPacketTimes = _interPacketTimes.TakeLast(Mediator.PacketTelemetryBatchSize - 1);
                
                _activity.AddEvent(new ActivityEvent($"stream_packet_batch_{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    ["batch_start"] = batchStart,
                    ["batch_end"] = _itemCount,
                    ["batch_size"] = Math.Min(Mediator.PacketTelemetryBatchSize, _itemCount),
                    ["avg_inter_packet_time_ms"] = recentInterPacketTimes.Any() ? recentInterPacketTimes.Average() : 0,
                    ["avg_processing_time_ms"] = _packetProcessingTimes.TakeLast(Mediator.PacketTelemetryBatchSize).Average(),
                    ["stream.operation"] = "packet_batch"
                }));
            }
            else
            {
                // Individual packet event
                _activity.AddEvent(new ActivityEvent($"stream_packet_{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    ["packet_number"] = _itemCount,
                    ["timestamp_ms"] = currentTime,
                    ["processing_time_ms"] = packetProcessingTimeMs,
                    ["inter_packet_time_ms"] = interPacketTime,
                    ["is_first_packet"] = _itemCount == 1,
                    ["stream.operation"] = "packet"
                }));
            }
        }
    }

    /// <summary>
    /// Records successful completion of the stream.
    /// </summary>
    public void RecordSuccess(Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// Records error information for the stream.
    /// </summary>
    public void RecordError(Activity? activity, Exception ex)
    {
        if (!Mediator.TelemetryEnabled) return;

        activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(ex.Message));
        activity?.SetTag("exception.type", SanitizeTypeName(ex.GetType().Name));
        activity?.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
    }

    /// <summary>
    /// Records comprehensive final metrics for the stream operation.
    /// </summary>
    public void RecordFinalMetrics(Activity? activity, TimeSpan totalDuration, Exception? exception)
    {
        if (!Mediator.TelemetryEnabled) return;

        // Calculate streaming performance metrics
        var totalSeconds = totalDuration.TotalSeconds;
        var throughputItemsPerSec = totalSeconds > 0 ? _itemCount / totalSeconds : 0;
        var avgInterPacketTime = _interPacketTimes.Count > 0 ? _interPacketTimes.Average() : 0;
        var minInterPacketTime = _interPacketTimes.Count > 0 ? _interPacketTimes.Min() : 0;
        var maxInterPacketTime = _interPacketTimes.Count > 0 ? _interPacketTimes.Max() : 0;
        var avgPacketProcessingTime = _packetProcessingTimes.Count > 0 ? _packetProcessingTimes.Average() : 0;

        // Calculate jitter (standard deviation of inter-packet times)
        double jitter = 0;
        if (_interPacketTimes.Count > 1)
        {
            var variance = _interPacketTimes.Select(x => Math.Pow(x - avgInterPacketTime, 2)).Average();
            jitter = Math.Sqrt(variance);
        }

        // Set comprehensive activity tags
        if (activity != null)
        {
            activity.SetTag("stream.items_count", _itemCount);
            activity.SetTag("duration_ms", totalDuration.TotalMilliseconds);
            activity.SetTag("stream.throughput_items_per_sec", throughputItemsPerSec);
            activity.SetTag("stream.ttfb_ms", _timeToFirstByte.TotalMilliseconds);
            activity.SetTag("stream.avg_inter_packet_time_ms", avgInterPacketTime);
            activity.SetTag("stream.min_inter_packet_time_ms", minInterPacketTime);
            activity.SetTag("stream.max_inter_packet_time_ms", maxInterPacketTime);
            activity.SetTag("stream.avg_packet_processing_time_ms", avgPacketProcessingTime);
            activity.SetTag("stream.total_processing_time_ms", _totalPacketProcessingTime);
            
            // Advanced streaming metrics
            if (_itemCount > 1)
            {
                var isConsistentThroughput = (maxInterPacketTime - minInterPacketTime) < (avgInterPacketTime * 0.5);
                activity.SetTag("stream.consistent_throughput", isConsistentThroughput);
                activity.SetTag("stream.jitter_ms", jitter);
                
                // Performance classification
                var performance = jitter < avgInterPacketTime * 0.1 ? "excellent" :
                                jitter < avgInterPacketTime * 0.3 ? "good" :
                                jitter < avgInterPacketTime * 0.5 ? "fair" : "poor";
                activity.SetTag("stream.performance_class", performance);
            }
            
            // OpenTelemetry semantic conventions
            activity.SetTag("stream.packet.count", _itemCount);
            activity.SetTag("stream.packet_level_telemetry_enabled", Mediator.PacketLevelTelemetryEnabled);
            activity.SetTag("stream.batch_size", Mediator.PacketTelemetryBatchSize);
        }

        // Record OpenTelemetry metrics
        var tags = new TagList
        {
            { "request_name", RequestTypeName },
            { "request_type", "stream" },
            { "response_type", ResponseTypeName },
            { "stream.items_count", _itemCount }
        };

        // Enhanced streaming metrics
        Mediator.StreamDurationHistogram.Record(totalDuration.TotalMilliseconds, tags);
        Mediator.StreamThroughputHistogram.Record(throughputItemsPerSec, tags);
        
        if (_timeToFirstByte.TotalMilliseconds > 0)
        {
            Mediator.StreamTtfbHistogram.Record(_timeToFirstByte.TotalMilliseconds, tags);
        }
        
        if (jitter > 0)
        {
            Mediator.StreamPacketJitterHistogram.Record(jitter, tags);
        }

        if (exception == null)
        {
            Mediator.StreamSuccessCounter.Add(1, tags);
        }
        else
        {
            tags.Add("exception.type", SanitizeTypeName(exception.GetType().Name));
            tags.Add("exception.message", SanitizeExceptionMessage(exception.Message));
            Mediator.StreamFailureCounter.Add(1, tags);
        }

        // Health check counter
        Mediator.TelemetryHealthCounter.Add(1, new TagList { { "operation", "send_stream" } });
    }

    /// <summary>
    /// Helper methods for telemetry context (duplicated from Mediator for encapsulation).
    /// </summary>
    private static string SanitizeTypeName(string typeName) => typeName.Replace('`', '_').Replace('<', '_').Replace('>', '_').Replace('+', '.');
    private static string SanitizeMiddlewareName(string middlewareName) => middlewareName.Replace("Middleware", "").Replace("middleware", "");
    private static string SanitizeExceptionMessage(string message) => message.Length > 500 ? message[..497] + "..." : message;

    /// <summary>
    /// Checks if middleware is applicable to the given request and response types.
    /// </summary>
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
}
