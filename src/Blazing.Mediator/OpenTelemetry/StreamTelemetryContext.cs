using Blazing.Mediator.Statistics;
using System.Diagnostics;

namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Telemetry context for streaming operations that tracks comprehensive packet-level metrics.
/// </summary>
/// <typeparam name="TResponse">The type of response items in the stream</typeparam>
internal sealed class StreamTelemetryContext<TResponse>(IStreamRequest<TResponse> request, TelemetryOptions? telemetryOptions)
{
    private readonly IStreamRequest<TResponse> _request = request ?? throw new ArgumentNullException(nameof(request));
    private readonly List<double> _interPacketTimes = new();
    private readonly List<double> _packetProcessingTimes = new();
    private int _itemCount;
    private TimeSpan _timeToFirstByte;
    private bool _firstItemReceived;
    private Activity? _activity; // Store reference to the activity
    private long _totalPacketProcessingTime;

    // Telemetry tag constants
    private const string _requestNameTag = "request_name";
    private const string _requestTypeTag = "request_type";
    private const string _responseTypeTag = "response_type";
    private const string _middlewarePipelineTag = "middleware.pipeline";
    private const string _handlerTypeTag = "handler.type";
    private const string _packetNumberTag = "packet_number";
    private const string _packetTimestampMsTag = "packet.timestamp_ms";
    private const string _packetProcessingTimeMsTag = "packet.processing_time_ms";
    private const string _packetInterPacketTimeMsTag = "packet.inter_packet_time_ms";
    private const string _packetIsFirstTag = "packet.is_first";
    private const string _streamTotalPacketsTag = "stream.total_packets";
    private const string _mediatorOperationTag = "mediator.operation";
    private const string _mediatorRequestTypeTag = "mediator.request_type";
    private const string _otelLibraryNameTag = "otel.library.name";
    private const string _otelLibraryVersionTag = "otel.library.version";
    private const string _packetSizeBytesTag = "packet.size_bytes";
    private const string _packetTypeTag = "packet.type";
    private const string _exceptionTypeTag = "exception.type";
    private const string _exceptionMessageTag = "exception.message";
    private const string _streamItemsCountTag = "stream.items_count";
    private const string _durationMsTag = "duration_ms";
    private const string _streamThroughputItemsPerSecTag = "stream.throughput_items_per_sec";
    private const string _streamTtfbMsTag = "stream.ttfb_ms";
    private const string _streamAvgInterPacketTimeMsTag = "stream.avg_inter_packet_time_ms";
    private const string _streamMinInterPacketTimeMsTag = "stream.min_inter_packet_time_ms";
    private const string _streamMaxInterPacketTimeMsTag = "stream.max_inter_packet_time_ms";
    private const string _streamAvgPacketProcessingTimeMsTag = "stream.avg_packet_processing_time_ms";
    private const string _streamTotalProcessingTimeMsTag = "stream.total_processing_time_ms";
    private const string _streamConsistentThroughputTag = "stream.consistent_throughput";
    private const string _streamJitterMsTag = "stream.jitter_ms";
    private const string _streamPerformanceClassTag = "stream.performance_class";
    private const string _streamPacketCountTag = "stream.packet.count";
    private const string _streamPacketLevelTelemetryEnabledTag = "stream.packet_level_telemetry_enabled";
    private const string _streamBatchSizeTag = "stream.batch_size";
    private const string _operationTag = "operation";

    // Activity event tag constants
    private const string _batchStartTag = "batch_start";
    private const string _batchEndTag = "batch_end";
    private const string _batchSizeTag = "batch_size";
    private const string _avgInterPacketTimeMsTag = "avg_inter_packet_time_ms";
    private const string _avgProcessingTimeMsTag = "avg_processing_time_ms";
    private const string _streamOperationTag = "stream.operation";
    private const string _timestampMsTag = "timestamp_ms";
    private const string _processingTimeMsTag = "processing_time_ms";
    private const string _interPacketTimeMsTag = "inter_packet_time_ms";
    private const string _isFirstPacketTag = "is_first_packet";
    private const string _packetNumberEventTag = "packet_number";

    // String literals and values
    private const string _streamRequestType = "stream";
    private const string _streamPacketRequestType = "stream_packet";
    private const string _sendStreamPacketOperation = "SendStreamPacket";
    private const string _blazingMediatorLibraryName = "Blazing.Mediator";
    private const string _defaultLibraryVersion = "1.0.0";
    private const string _packetBatchOperation = "packet_batch";
    private const string _packetOperation = "packet";
    private const string _sendStreamOperation = "send_stream";
    private const string _excellentPerformance = "excellent";
    private const string _goodPerformance = "good";
    private const string _fairPerformance = "fair";
    private const string _poorPerformance = "poor";
    private const string _commaDelimiter = ",";
    private const string _activityNamePrefix = "Mediator.SendStream:";
    private const string _packetSuffix = ".packet_";
    private const string _streamPacketBatchPrefix = "stream_packet_batch_";
    private const string _streamPacketPrefix = "stream_packet_";

    public string RequestTypeName { get; } = SanitizeTypeName(request.GetType().Name);
    public string ResponseTypeName { get; } = SanitizeTypeName(typeof(TResponse).Name);
    public int ItemCount => _itemCount; // Expose item count for telemetry
    public double AverageInterPacketTime => _interPacketTimes.Count > 0 ? _interPacketTimes.Average() : 0;

    private bool IsTelemetryEnabled => telemetryOptions?.Enabled ?? true;
    private bool IsPacketLevelTelemetryEnabled => telemetryOptions?.PacketLevelTelemetryEnabled ?? false;

    /// <summary>
    /// Initializes the telemetry context with activity and service information.
    /// </summary>
    public void Initialize(Activity? activity, IServiceProvider serviceProvider, IMiddlewarePipelineBuilder pipelineBuilder, MediatorStatistics? statistics)
    {
        if (!IsTelemetryEnabled) return;

        _activity = activity; // Store the activity reference
        statistics?.IncrementQuery(_request.GetType().Name);

        if (activity == null)
        {
            return;
        }

        activity.SetTag(_requestNameTag, RequestTypeName);
        activity.SetTag(_requestTypeTag, _streamRequestType);
        activity.SetTag(_responseTypeTag, ResponseTypeName);

        // Get middleware pipeline information
        if (pipelineBuilder is IMiddlewarePipelineInspector inspector)
        {
            var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);
            var allMiddleware = middlewareInfo
                .Where(m => IsMiddlewareApplicable(m.Type, _request.GetType(), typeof(TResponse)))
                .OrderBy(m => m.Order)
                .Select(m => SanitizeMiddlewareName(m.Type))
                .ToList();

            activity.SetTag(_middlewarePipelineTag, string.Join(_commaDelimiter, allMiddleware));
        }

        // Get handler information
        var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(_request.GetType(), typeof(TResponse));
        var handlers = serviceProvider.GetServices(handlerType);
        var handler = handlers.FirstOrDefault();
        if (handler != null)
        {
            activity.SetTag(_handlerTypeTag, SanitizeTypeName(handler.GetType().Name));
        }
    }

    /// <summary>
    /// Records packet-level metrics for each item in the stream with enhanced telemetry support.
    /// </summary>
    public void RecordPacket(long currentTime, long lastItemTime, TResponse item, double packetProcessingTimeMs, CancellationToken cancellationToken = default)
    {
        if (!IsTelemetryEnabled) return;

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
            { _requestNameTag, RequestTypeName },
            { _responseTypeTag, ResponseTypeName },
            { _packetNumberTag, _itemCount }
        };

        Mediator._streamPacketCounter.Add(1, packetTags);
        Mediator._streamPacketProcessingTimeHistogram.Record(packetProcessingTimeMs, packetTags);

        if (interPacketTime > 0)
        {
            Mediator._streamInterPacketTimeHistogram.Record(interPacketTime, packetTags);
        }

        // Create child span for packet if packet-level telemetry is enabled
        if (IsPacketLevelTelemetryEnabled && _activity != null)
        {
            using var packetActivity = Mediator.ActivitySource.StartActivity($"{_activityNamePrefix}{RequestTypeName}{_packetSuffix}{_itemCount}", ActivityKind.Internal, _activity.Context);
            if (packetActivity != null)
            {
                packetActivity.SetTag(_packetNumberTag, _itemCount);
                packetActivity.SetTag(_packetTimestampMsTag, currentTime);
                packetActivity.SetTag(_packetProcessingTimeMsTag, packetProcessingTimeMs);
                packetActivity.SetTag(_packetInterPacketTimeMsTag, interPacketTime);
                packetActivity.SetTag(_packetIsFirstTag, _itemCount == 1);
                packetActivity.SetTag(_streamTotalPacketsTag, _itemCount);
                packetActivity.SetTag(_requestNameTag, RequestTypeName);
                packetActivity.SetTag(_requestTypeTag, _streamPacketRequestType);
                packetActivity.SetTag(_mediatorOperationTag, _sendStreamPacketOperation);
                packetActivity.SetTag(_mediatorRequestTypeTag, _streamPacketRequestType);
                packetActivity.SetTag(_otelLibraryNameTag, _blazingMediatorLibraryName);
                packetActivity.SetTag(_otelLibraryVersionTag, typeof(Mediator).Assembly.GetName().Version?.ToString() ?? _defaultLibraryVersion);

                // Add packet size if available (attempt to serialize for size estimation)
                try
                {
                    if (item is string str)
                    {
                        packetActivity.SetTag(_packetSizeBytesTag, System.Text.Encoding.UTF8.GetByteCount(str));
                    }
                    else if (item != null)
                    {
                        // Rough estimation based on type
                        var typeName = item.GetType().Name;
                        packetActivity.SetTag(_packetTypeTag, SanitizeTypeName(typeName));
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

                var interPacketTimes = recentInterPacketTimes as double[] ?? recentInterPacketTimes.ToArray();
                _activity.AddEvent(new ActivityEvent($"{_streamPacketBatchPrefix}{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    [_batchStartTag] = batchStart,
                    [_batchEndTag] = _itemCount,
                    [_batchSizeTag] = Math.Min(Mediator.PacketTelemetryBatchSize, _itemCount),
                    [_avgInterPacketTimeMsTag] = interPacketTimes.Any() ? interPacketTimes.Average() : 0,
                    [_avgProcessingTimeMsTag] = _packetProcessingTimes.TakeLast(Mediator.PacketTelemetryBatchSize).Average(),
                    [_streamOperationTag] = _packetBatchOperation
                }));
            }
            else
            {
                // Individual packet event
                _activity.AddEvent(new ActivityEvent($"{_streamPacketPrefix}{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    [_packetNumberEventTag] = _itemCount,
                    [_timestampMsTag] = currentTime,
                    [_processingTimeMsTag] = packetProcessingTimeMs,
                    [_interPacketTimeMsTag] = interPacketTime,
                    [_isFirstPacketTag] = _itemCount == 1,
                    [_streamOperationTag] = _packetOperation
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
        if (!IsTelemetryEnabled) return;

        activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(ex.Message));
        activity?.SetTag(_exceptionTypeTag, SanitizeTypeName(ex.GetType().Name));
        activity?.SetTag(_exceptionMessageTag, SanitizeExceptionMessage(ex.Message));
    }

    /// <summary>
    /// Records comprehensive final metrics for the stream operation.
    /// </summary>
    public void RecordFinalMetrics(Activity? activity, TimeSpan totalDuration, Exception? exception)
    {
        if (!IsTelemetryEnabled) return;

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
            activity.SetTag(_streamItemsCountTag, _itemCount);
            activity.SetTag(_durationMsTag, totalDuration.TotalMilliseconds);
            activity.SetTag(_streamThroughputItemsPerSecTag, throughputItemsPerSec);
            activity.SetTag(_streamTtfbMsTag, _timeToFirstByte.TotalMilliseconds);
            activity.SetTag(_streamAvgInterPacketTimeMsTag, avgInterPacketTime);
            activity.SetTag(_streamMinInterPacketTimeMsTag, minInterPacketTime);
            activity.SetTag(_streamMaxInterPacketTimeMsTag, maxInterPacketTime);
            activity.SetTag(_streamAvgPacketProcessingTimeMsTag, avgPacketProcessingTime);
            activity.SetTag(_streamTotalProcessingTimeMsTag, _totalPacketProcessingTime);

            // Advanced streaming metrics
            if (_itemCount > 1)
            {
                var isConsistentThroughput = (maxInterPacketTime - minInterPacketTime) < (avgInterPacketTime * 0.5);
                activity.SetTag(_streamConsistentThroughputTag, isConsistentThroughput);
                activity.SetTag(_streamJitterMsTag, jitter);

                // Performance classification using configurable thresholds
                if (telemetryOptions?.EnableStreamingPerformanceClassification == true)
                {
                    var excellentThreshold = telemetryOptions.ExcellentPerformanceThreshold;
                    var goodThreshold = telemetryOptions.GoodPerformanceThreshold;
                    var fairThreshold = telemetryOptions.FairPerformanceThreshold;

                    var performance = jitter < avgInterPacketTime * excellentThreshold ? _excellentPerformance :
                        jitter < avgInterPacketTime * goodThreshold ? _goodPerformance :
                        jitter < avgInterPacketTime * fairThreshold ? _fairPerformance : _poorPerformance;
                    activity.SetTag(_streamPerformanceClassTag, performance);
                }
                else
                {
                    // Fallback to default thresholds when classification is disabled
                    var performance = jitter < avgInterPacketTime * 0.1 ? _excellentPerformance :
                        jitter < avgInterPacketTime * 0.3 ? _goodPerformance :
                        jitter < avgInterPacketTime * 0.5 ? _fairPerformance : _poorPerformance;
                    activity.SetTag(_streamPerformanceClassTag, performance);
                }
            }

            // OpenTelemetry semantic conventions
            activity.SetTag(_streamPacketCountTag, _itemCount);
            activity.SetTag(_streamPacketLevelTelemetryEnabledTag, IsPacketLevelTelemetryEnabled);
            activity.SetTag(_streamBatchSizeTag, telemetryOptions?.PacketTelemetryBatchSize ?? 10);
        }

        // Record OpenTelemetry metrics
        var tags = new TagList
        {
            { _requestNameTag, RequestTypeName },
            { _requestTypeTag, _streamRequestType },
            { _responseTypeTag, ResponseTypeName },
            { _streamItemsCountTag, _itemCount }
        };

        // Enhanced streaming metrics
        Mediator._streamDurationHistogram.Record(totalDuration.TotalMilliseconds, tags);
        Mediator._streamThroughputHistogram.Record(throughputItemsPerSec, tags);

        if (_timeToFirstByte.TotalMilliseconds > 0)
        {
            Mediator._streamTtfbHistogram.Record(_timeToFirstByte.TotalMilliseconds, tags);
        }

        if (jitter > 0)
        {
            Mediator._streamPacketJitterHistogram.Record(jitter, tags);
        }

        if (exception == null)
        {
            Mediator._streamSuccessCounter.Add(1, tags);
        }
        else
        {
            tags.Add(_exceptionTypeTag, SanitizeTypeName(exception.GetType().Name));
            tags.Add(_exceptionMessageTag, SanitizeExceptionMessage(exception.Message));
            Mediator._streamFailureCounter.Add(1, tags);
        }

        // Health check counter
        Mediator._telemetryHealthCounter.Add(1, new TagList { { _operationTag, _sendStreamOperation } });
    }

    /// <summary>
    /// Helper methods for telemetry context (duplicated from Mediator for encapsulation).
    /// </summary>
    private static string SanitizeTypeName(string typeName) => typeName.Replace('`', '_').Replace('<', '_').Replace('>', '_').Replace('+', '.');
    private static string SanitizeMiddlewareName(Type middlewareType) => PipelineUtilities.FormatTypeName(middlewareType);
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