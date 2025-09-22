using System.Diagnostics;
using Blazing.Mediator.Statistics;

namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Telemetry context for streaming operations that tracks comprehensive packet-level metrics.
/// </summary>
/// <typeparam name="TResponse">The type of response items in the stream</typeparam>
internal sealed class StreamTelemetryContext<TResponse>(IStreamRequest<TResponse> request, MediatorTelemetryOptions? telemetryOptions)
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
        if (IsPacketLevelTelemetryEnabled && _activity != null)
        {
            using var packetActivity = Mediator.ActivitySource.StartActivity($"Mediator.SendStream:{RequestTypeName}.packet_{_itemCount}", ActivityKind.Internal, _activity.Context);
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

                var interPacketTimes = recentInterPacketTimes as double[] ?? recentInterPacketTimes.ToArray();
                _activity.AddEvent(new ActivityEvent($"stream_packet_batch_{_itemCount}", DateTimeOffset.UtcNow, new ActivityTagsCollection
                {
                    ["batch_start"] = batchStart,
                    ["batch_end"] = _itemCount,
                    ["batch_size"] = Math.Min(Mediator.PacketTelemetryBatchSize, _itemCount),
                    ["avg_inter_packet_time_ms"] = interPacketTimes.Any() ? interPacketTimes.Average() : 0,
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
        if (!IsTelemetryEnabled) return;

        activity?.SetStatus(ActivityStatusCode.Error, SanitizeExceptionMessage(ex.Message));
        activity?.SetTag("exception.type", SanitizeTypeName(ex.GetType().Name));
        activity?.SetTag("exception.message", SanitizeExceptionMessage(ex.Message));
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
                
                // Performance classification using configurable thresholds
                if (telemetryOptions?.EnableStreamingPerformanceClassification == true)
                {
                    var excellentThreshold = telemetryOptions.ExcellentPerformanceThreshold;
                    var goodThreshold = telemetryOptions.GoodPerformanceThreshold;
                    var fairThreshold = telemetryOptions.FairPerformanceThreshold;
                    
                    var performance = jitter < avgInterPacketTime * excellentThreshold ? "excellent" :
                        jitter < avgInterPacketTime * goodThreshold ? "good" :
                        jitter < avgInterPacketTime * fairThreshold ? "fair" : "poor";
                    activity.SetTag("stream.performance_class", performance);
                }
                else
                {
                    // Fallback to default thresholds when classification is disabled
                    var performance = jitter < avgInterPacketTime * 0.1 ? "excellent" :
                        jitter < avgInterPacketTime * 0.3 ? "good" :
                        jitter < avgInterPacketTime * 0.5 ? "fair" : "poor";
                    activity.SetTag("stream.performance_class", performance);
                }
            }
            
            // OpenTelemetry semantic conventions
            activity.SetTag("stream.packet.count", _itemCount);
            activity.SetTag("stream.packet_level_telemetry_enabled", IsPacketLevelTelemetryEnabled);
            activity.SetTag("stream.batch_size", telemetryOptions?.PacketTelemetryBatchSize ?? 10);
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