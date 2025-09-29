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
    private const string RequestNameTag = "request_name";
    private const string RequestTypeTag = "request_type";
    private const string ResponseTypeTag = "response_type";
    private const string MiddlewarePipelineTag = "middleware.pipeline";
    private const string HandlerTypeTag = "handler.type";
    private const string PacketNumberTag = "packet_number";
    private const string PacketTimestampMsTag = "packet.timestamp_ms";
    private const string PacketProcessingTimeMsTag = "packet.processing_time_ms";
    private const string PacketInterPacketTimeMsTag = "packet.inter_packet_time_ms";
    private const string PacketIsFirstTag = "packet.is_first";
    private const string StreamTotalPacketsTag = "stream.total_packets";
    private const string MediatorOperationTag = "mediator.operation";
    private const string MediatorRequestTypeTag = "mediator.request_type";
    private const string OtelLibraryNameTag = "otel.library.name";
    private const string OtelLibraryVersionTag = "otel.library.version";
    private const string PacketSizeBytesTag = "packet.size_bytes";
    private const string PacketTypeTag = "packet.type";
    private const string ExceptionTypeTag = "exception.type";
    private const string ExceptionMessageTag = "exception.message";
    private const string StreamItemsCountTag = "stream.items_count";
    private const string DurationMsTag = "duration_ms";
    private const string StreamThroughputItemsPerSecTag = "stream.throughput_items_per_sec";
    private const string StreamTtfbMsTag = "stream.ttfb_ms";
    private const string StreamAvgInterPacketTimeMsTag = "stream.avg_inter_packet_time_ms";
    private const string StreamMinInterPacketTimeMsTag = "stream.min_inter_packet_time_ms";
    private const string StreamMaxInterPacketTimeMsTag = "stream.max_inter_packet_time_ms";
    private const string StreamAvgPacketProcessingTimeMsTag = "stream.avg_packet_processing_time_ms";
    private const string StreamTotalProcessingTimeMsTag = "stream.total_processing_time_ms";
    private const string StreamConsistentThroughputTag = "stream.consistent_throughput";
    private const string StreamJitterMsTag = "stream.jitter_ms";
    private const string StreamPerformanceClassTag = "stream.performance_class";
    private const string StreamPacketCountTag = "stream.packet.count";
    private const string StreamPacketLevelTelemetryEnabledTag = "stream.packet_level_telemetry_enabled";
    private const string StreamBatchSizeTag = "stream.batch_size";
    private const string OperationTag = "operation";

    // Activity event tag constants
    private const string BatchStartTag = "batch_start";
    private const string BatchEndTag = "batch_end";
    private const string BatchSizeTag = "batch_size";
    private const string AvgInterPacketTimeMsTag = "avg_inter_packet_time_ms";
    private const string AvgProcessingTimeMsTag = "avg_processing_time_ms";
    private const string StreamOperationTag = "stream.operation";
    private const string TimestampMsTag = "timestamp_ms";
    private const string ProcessingTimeMsTag = "processing_time_ms";
    private const string InterPacketTimeMsTag = "inter_packet_time_ms";
    private const string IsFirstPacketTag = "is_first_packet";
    private const string PacketNumberEventTag = "packet_number";

    // String literals and values
    private const string StreamRequestType = "stream";
    private const string StreamPacketRequestType = "stream_packet";
    private const string SendStreamPacketOperation = "SendStreamPacket";
    private const string BlazingMediatorLibraryName = "Blazing.Mediator";
    private const string DefaultLibraryVersion = "1.0.0";
    private const string PacketBatchOperation = "packet_batch";
    private const string PacketOperation = "packet";
    private const string SendStreamOperation = "send_stream";
    private const string ExcellentPerformance = "excellent";
    private const string GoodPerformance = "good";
    private const string FairPerformance = "fair";
    private const string PoorPerformance = "poor";
    private const string CommaDelimiter = ",";
    private const string ActivityNamePrefix = "Mediator.SendStream:";
    private const string PacketSuffix = ".packet_";
    private const string StreamPacketBatchPrefix = "stream_packet_batch_";
    private const string StreamPacketPrefix = "stream_packet_";

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

        activity.SetTag(RequestNameTag, RequestTypeName);
        activity.SetTag(RequestTypeTag, StreamRequestType);
        activity.SetTag(ResponseTypeTag, ResponseTypeName);

        // Get middleware pipeline information
        if (pipelineBuilder is IMiddlewarePipelineInspector inspector)
        {
            var middlewareInfo = inspector.GetDetailedMiddlewareInfo(serviceProvider);
            var allMiddleware = middlewareInfo
                .Where(m => IsMiddlewareApplicable(m.Type, _request.GetType(), typeof(TResponse)))
                .OrderBy(m => m.Order)
                .Select(m => SanitizeMiddlewareName(m.Type.Name))
                .ToList();

            activity.SetTag(MiddlewarePipelineTag, string.Join(CommaDelimiter, allMiddleware));
        }

        // Get handler information
        var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(_request.GetType(), typeof(TResponse));
        var handlers = serviceProvider.GetServices(handlerType);
        var handler = handlers.FirstOrDefault();
        if (handler != null)
        {
            activity.SetTag(HandlerTypeTag, SanitizeTypeName(handler.GetType().Name));
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
            { RequestNameTag, RequestTypeName },
            { ResponseTypeTag, ResponseTypeName },
            { PacketNumberTag, _itemCount }
        };

        Mediator.StreamPacketCounter.Add(1, packetTags);
        Mediator.StreamPacketProcessingTimeHistogram.Record(packetProcessingTimeMs, packetTags);

        if (interPacketTime > 0)
        {
            Mediator.StreamInterPacketTimeHistogram.Record(interPacketTime, packetTags);
        }

        // Create child span for packet if packet-level telemetry is enabled
        if (IsPacketLevelTelemetryEnabled && _activity != null)
        {
            using var packetActivity = Mediator.ActivitySource.StartActivity($"{ActivityNamePrefix}{RequestTypeName}{PacketSuffix}{_itemCount}", ActivityKind.Internal, _activity.Context);
            if (packetActivity != null)
            {
                packetActivity.SetTag(PacketNumberTag, _itemCount);
                packetActivity.SetTag(PacketTimestampMsTag, currentTime);
                packetActivity.SetTag(PacketProcessingTimeMsTag, packetProcessingTimeMs);
                packetActivity.SetTag(PacketInterPacketTimeMsTag, interPacketTime);
                packetActivity.SetTag(PacketIsFirstTag, _itemCount == 1);
                packetActivity.SetTag(StreamTotalPacketsTag, _itemCount);
                packetActivity.SetTag(RequestNameTag, RequestTypeName);
                packetActivity.SetTag(RequestTypeTag, StreamPacketRequestType);
                packetActivity.SetTag(MediatorOperationTag, SendStreamPacketOperation);
                packetActivity.SetTag(MediatorRequestTypeTag, StreamPacketRequestType);
                packetActivity.SetTag(OtelLibraryNameTag, BlazingMediatorLibraryName);
                packetActivity.SetTag(OtelLibraryVersionTag, typeof(Mediator).Assembly.GetName().Version?.ToString() ?? DefaultLibraryVersion);

                // Add packet size if available (attempt to serialize for size estimation)
                try
                {
                    if (item is string str)
                    {
                        packetActivity.SetTag(PacketSizeBytesTag, System.Text.Encoding.UTF8.GetByteCount(str));
                    }
                    else if (item != null)
                    {
                        // Rough estimation based on type
                        var typeName = item.GetType().Name;
                        packetActivity.SetTag(PacketTypeTag, SanitizeTypeName(typeName));
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
                _activity.AddEvent(new ActivityEvent($"{StreamPacketBatchPrefix}{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    [BatchStartTag] = batchStart,
                    [BatchEndTag] = _itemCount,
                    [BatchSizeTag] = Math.Min(Mediator.PacketTelemetryBatchSize, _itemCount),
                    [AvgInterPacketTimeMsTag] = interPacketTimes.Any() ? interPacketTimes.Average() : 0,
                    [AvgProcessingTimeMsTag] = _packetProcessingTimes.TakeLast(Mediator.PacketTelemetryBatchSize).Average(),
                    [StreamOperationTag] = PacketBatchOperation
                }));
            }
            else
            {
                // Individual packet event
                _activity.AddEvent(new ActivityEvent($"{StreamPacketPrefix}{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    [PacketNumberEventTag] = _itemCount,
                    [TimestampMsTag] = currentTime,
                    [ProcessingTimeMsTag] = packetProcessingTimeMs,
                    [InterPacketTimeMsTag] = interPacketTime,
                    [IsFirstPacketTag] = _itemCount == 1,
                    [StreamOperationTag] = PacketOperation
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
        activity?.SetTag(ExceptionTypeTag, SanitizeTypeName(ex.GetType().Name));
        activity?.SetTag(ExceptionMessageTag, SanitizeExceptionMessage(ex.Message));
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
            activity.SetTag(StreamItemsCountTag, _itemCount);
            activity.SetTag(DurationMsTag, totalDuration.TotalMilliseconds);
            activity.SetTag(StreamThroughputItemsPerSecTag, throughputItemsPerSec);
            activity.SetTag(StreamTtfbMsTag, _timeToFirstByte.TotalMilliseconds);
            activity.SetTag(StreamAvgInterPacketTimeMsTag, avgInterPacketTime);
            activity.SetTag(StreamMinInterPacketTimeMsTag, minInterPacketTime);
            activity.SetTag(StreamMaxInterPacketTimeMsTag, maxInterPacketTime);
            activity.SetTag(StreamAvgPacketProcessingTimeMsTag, avgPacketProcessingTime);
            activity.SetTag(StreamTotalProcessingTimeMsTag, _totalPacketProcessingTime);

            // Advanced streaming metrics
            if (_itemCount > 1)
            {
                var isConsistentThroughput = (maxInterPacketTime - minInterPacketTime) < (avgInterPacketTime * 0.5);
                activity.SetTag(StreamConsistentThroughputTag, isConsistentThroughput);
                activity.SetTag(StreamJitterMsTag, jitter);

                // Performance classification using configurable thresholds
                if (telemetryOptions?.EnableStreamingPerformanceClassification == true)
                {
                    var excellentThreshold = telemetryOptions.ExcellentPerformanceThreshold;
                    var goodThreshold = telemetryOptions.GoodPerformanceThreshold;
                    var fairThreshold = telemetryOptions.FairPerformanceThreshold;

                    var performance = jitter < avgInterPacketTime * excellentThreshold ? ExcellentPerformance :
                        jitter < avgInterPacketTime * goodThreshold ? GoodPerformance :
                        jitter < avgInterPacketTime * fairThreshold ? FairPerformance : PoorPerformance;
                    activity.SetTag(StreamPerformanceClassTag, performance);
                }
                else
                {
                    // Fallback to default thresholds when classification is disabled
                    var performance = jitter < avgInterPacketTime * 0.1 ? ExcellentPerformance :
                        jitter < avgInterPacketTime * 0.3 ? GoodPerformance :
                        jitter < avgInterPacketTime * 0.5 ? FairPerformance : PoorPerformance;
                    activity.SetTag(StreamPerformanceClassTag, performance);
                }
            }

            // OpenTelemetry semantic conventions
            activity.SetTag(StreamPacketCountTag, _itemCount);
            activity.SetTag(StreamPacketLevelTelemetryEnabledTag, IsPacketLevelTelemetryEnabled);
            activity.SetTag(StreamBatchSizeTag, telemetryOptions?.PacketTelemetryBatchSize ?? 10);
        }

        // Record OpenTelemetry metrics
        var tags = new TagList
        {
            { RequestNameTag, RequestTypeName },
            { RequestTypeTag, StreamRequestType },
            { ResponseTypeTag, ResponseTypeName },
            { StreamItemsCountTag, _itemCount }
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
            tags.Add(ExceptionTypeTag, SanitizeTypeName(exception.GetType().Name));
            tags.Add(ExceptionMessageTag, SanitizeExceptionMessage(exception.Message));
            Mediator.StreamFailureCounter.Add(1, tags);
        }

        // Health check counter
        Mediator.TelemetryHealthCounter.Add(1, new TagList { { OperationTag, SendStreamOperation } });
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