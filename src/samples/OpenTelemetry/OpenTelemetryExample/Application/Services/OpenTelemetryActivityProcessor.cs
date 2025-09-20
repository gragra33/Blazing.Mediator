using OpenTelemetry;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Custom OpenTelemetry activity processor that captures raw Activity data and stores it in the database.
/// This processor receives all Activity data from the OpenTelemetry SDK before it's exported.
/// Implements efficient batching for streaming telemetry to prevent database overload.
/// </summary>
public sealed class OpenTelemetryActivityProcessor : BaseProcessor<Activity>, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpenTelemetryActivityProcessor> _logger;
    private readonly TelemetryBatchingOptions _options;
    private readonly ConcurrentQueue<TelemetryBatch> _batchQueue = new();
    private readonly Timer _batchTimer;
    private readonly object _batchLock = new();
    private volatile bool _disposed;

    // Current batch tracking
    private TelemetryBatch _currentStreamingBatch = new();
    private TelemetryBatch _currentRegularBatch = new();
    private DateTime _lastStreamingFlush = DateTime.UtcNow;
    private DateTime _lastRegularFlush = DateTime.UtcNow;

    public OpenTelemetryActivityProcessor(
        IServiceProvider serviceProvider, 
        ILogger<OpenTelemetryActivityProcessor> logger,
        TelemetryBatchingOptions options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
        
        // Initialize the batch processing timer with configured interval
        _batchTimer = new Timer(ProcessBatches, null, 
            TimeSpan.FromMilliseconds(_options.ProcessingIntervalMs), 
            TimeSpan.FromMilliseconds(_options.ProcessingIntervalMs));

        _logger.LogInformation("OpenTelemetryActivityProcessor initialized with batching: Streaming({StreamingBatchSize}/{StreamingTimeoutMs}ms), Regular({RegularBatchSize}/{RegularTimeoutMs}ms)",
            _options.StreamingBatchSize, _options.StreamingBatchTimeoutMs,
            _options.RegularBatchSize, _options.RegularBatchTimeoutMs);
    }

    public override void OnEnd(Activity activity)
    {
        if (_disposed) return;

        try
        {
            // Don't capture our own telemetry endpoints to prevent feedback loops
            if (ShouldSkipActivity(activity))
            {
                return;
            }

            var telemetryTrace = new TelemetryTrace
            {
                TraceId = activity.TraceId.ToString(),
                SpanId = activity.SpanId.ToString(),
                ParentId = !activity.ParentSpanId.Equals(default) ? activity.ParentSpanId.ToString() : null,
                OperationName = activity.OperationName,
                StartTime = activity.StartTimeUtc,
                Duration = activity.Duration,
                Status = activity.Status.ToString(),
                Tags = ExtractTags(activity),
                ExceptionType = GetExceptionType(activity),
                ExceptionMessage = GetExceptionMessage(activity),
                RequestType = GetRequestType(activity),
                HandlerName = GetHandlerName(activity)
            };

            var telemetryActivity = new TelemetryActivity
            {
                ActivityId = activity.Id ?? Guid.NewGuid().ToString(),
                ParentId = activity.ParentId,
                OperationName = activity.OperationName,
                StartTime = activity.StartTimeUtc,
                Duration = activity.Duration,
                Status = activity.Status.ToString(),
                Kind = activity.Kind.ToString(),
                Tags = ExtractTags(activity),
                RequestType = GetRequestType(activity),
                HandlerName = GetHandlerName(activity),
                IsSuccess = activity.Status != ActivityStatusCode.Error
            };

            var telemetryMetric = new TelemetryMetric
            {
                RequestType = telemetryActivity.RequestType,
                RequestName = ExtractRequestName(activity),
                Category = DetermineCategory(activity),
                Duration = activity.Duration.TotalMilliseconds,
                IsSuccess = activity.Status != ActivityStatusCode.Error,
                ErrorMessage = GetExceptionMessage(activity),
                Timestamp = activity.StartTimeUtc,
                HandlerName = telemetryActivity.HandlerName,
                Tags = ExtractTags(activity)
            };

            // Determine if this is streaming telemetry
            bool isStreamingTelemetry = IsStreamingTelemetry(activity);

            // Add to appropriate batch
            lock (_batchLock)
            {
                if (isStreamingTelemetry)
                {
                    _currentStreamingBatch.Traces.Add(telemetryTrace);
                    _currentStreamingBatch.Activities.Add(telemetryActivity);
                    _currentStreamingBatch.Metrics.Add(telemetryMetric);

                    if (_options.EnableDetailedLogging)
                    {
                        _logger.LogTrace("Added streaming telemetry to batch: {OperationName} (batch size: {BatchSize})",
                            activity.OperationName, _currentStreamingBatch.Traces.Count);
                    }
                }
                else
                {
                    _currentRegularBatch.Traces.Add(telemetryTrace);
                    _currentRegularBatch.Activities.Add(telemetryActivity);
                    _currentRegularBatch.Metrics.Add(telemetryMetric);

                    if (_options.EnableDetailedLogging)
                    {
                        _logger.LogTrace("Added regular telemetry to batch: {OperationName} (batch size: {BatchSize})",
                            activity.OperationName, _currentRegularBatch.Traces.Count);
                    }
                }
            }

            // Check if we need to flush batches
            CheckAndFlushBatches();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing activity: {OperationName}", activity.OperationName);
        }
    }

    private void CheckAndFlushBatches()
    {
        var now = DateTime.UtcNow;
        bool shouldFlushStreaming = false;
        bool shouldFlushRegular = false;

        lock (_batchLock)
        {
            // Check streaming batch
            if (_currentStreamingBatch.Traces.Count >= _options.StreamingBatchSize ||
                (now - _lastStreamingFlush).TotalMilliseconds >= _options.StreamingBatchTimeoutMs)
            {
                shouldFlushStreaming = true;
            }

            // Check regular batch
            if (_currentRegularBatch.Traces.Count >= _options.RegularBatchSize ||
                (now - _lastRegularFlush).TotalMilliseconds >= _options.RegularBatchTimeoutMs)
            {
                shouldFlushRegular = true;
            }
        }

        if (shouldFlushStreaming)
        {
            FlushStreamingBatch();
        }

        if (shouldFlushRegular)
        {
            FlushRegularBatch();
        }
    }

    private void FlushStreamingBatch()
    {
        TelemetryBatch batchToProcess;

        lock (_batchLock)
        {
            if (_currentStreamingBatch.Traces.Count == 0) return;

            batchToProcess = _currentStreamingBatch;
            _currentStreamingBatch = new TelemetryBatch();
            _lastStreamingFlush = DateTime.UtcNow;
        }

        if (batchToProcess.Traces.Count > 0)
        {
            _batchQueue.Enqueue(batchToProcess);
            _logger.LogDebug("Queued streaming batch for processing with {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics",
                batchToProcess.Traces.Count, batchToProcess.Activities.Count, batchToProcess.Metrics.Count);
        }
    }

    private void FlushRegularBatch()
    {
        TelemetryBatch batchToProcess;

        lock (_batchLock)
        {
            if (_currentRegularBatch.Traces.Count == 0) return;

            batchToProcess = _currentRegularBatch;
            _currentRegularBatch = new TelemetryBatch();
            _lastRegularFlush = DateTime.UtcNow;
        }

        if (batchToProcess.Traces.Count > 0)
        {
            _batchQueue.Enqueue(batchToProcess);
            _logger.LogDebug("Queued regular batch for processing with {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics",
                batchToProcess.Traces.Count, batchToProcess.Activities.Count, batchToProcess.Metrics.Count);
        }
    }

    private void ProcessBatches(object? state)
    {
        if (_disposed) return;

        while (_batchQueue.TryDequeue(out var batch))
        {
            try
            {
                ProcessBatch(batch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing telemetry batch with {Count} items", batch.Traces.Count);
            }
        }

        // Also check for timeout-based flushes
        CheckAndFlushBatches();
    }

    private void ProcessBatch(TelemetryBatch batch)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Use a scope to get the DbContext since this is a singleton processor
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Perform bulk insert operations
            if (batch.Traces.Count > 0)
            {
                context.TelemetryTraces.AddRange(batch.Traces);
            }

            if (batch.Activities.Count > 0)
            {
                context.TelemetryActivities.AddRange(batch.Activities);
            }

            if (batch.Metrics.Count > 0)
            {
                context.TelemetryMetrics.AddRange(batch.Metrics);
            }

            // Save all changes in a single transaction
            context.SaveChanges();

            stopwatch.Stop();

            _logger.LogInformation("Successfully processed telemetry batch: {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics in {Duration}ms",
                batch.Traces.Count, batch.Activities.Count, batch.Metrics.Count, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process telemetry batch with {TraceCount} traces in {Duration}ms",
                batch.Traces.Count, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static bool IsStreamingTelemetry(Activity activity)
    {
        var operationName = activity.OperationName?.ToLowerInvariant() ?? "";
        var requestType = GetRequestType(activity).ToLowerInvariant();

        // Check if this is streaming-related telemetry
        return operationName.Contains("sendstream") ||
               operationName.Contains("stream_packet") ||
               requestType.Contains("stream") ||
               requestType.Contains("stream_packet") ||
               operationName.Contains("mediator.sendstream");
    }

    private static bool ShouldSkipActivity(Activity activity)
    {
        var operationName = activity.OperationName?.ToLowerInvariant() ?? "";

        // Skip telemetry endpoints to prevent feedback loops
        if (operationName.Contains("/telemetry") ||
            operationName.Contains("/debug") ||
            operationName.Contains("otlp") ||
            operationName.Contains("healthcheck"))
        {
            return true;
        }

        // Reduce minimum duration filter to capture packet-level spans
        // Only skip extremely short activities (less than 0.1ms) unless they're mediator operations
        if (activity.Duration.TotalMilliseconds < 0.1 && 
            !operationName.Contains("mediator", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static Dictionary<string, object> ExtractTags(Activity activity)
    {
        var tags = new Dictionary<string, object>();

        // Add standard OpenTelemetry tags
        foreach (var tag in activity.Tags)
        {
            tags[tag.Key] = tag.Value ?? "";
        }

        // Add baggage
        foreach (var baggage in activity.Baggage)
        {
            tags[$"baggage.{baggage.Key}"] = baggage.Value ?? "";
        }

        // Add some computed tags
        tags["activity.kind"] = activity.Kind.ToString();
        tags["activity.status"] = activity.Status.ToString();
        tags["activity.duration_ms"] = activity.Duration.TotalMilliseconds;

        return tags;
    }

    private static string? GetExceptionType(Activity activity)
    {
        // Look for exception information in tags
        var exceptionType = activity.Tags.FirstOrDefault(t =>
            t.Key.Equals("exception.type", StringComparison.OrdinalIgnoreCase)).Value;

        return exceptionType;
    }

    private static string? GetExceptionMessage(Activity activity)
    {
        // Look for exception message in tags
        var exceptionMessage = activity.Tags.FirstOrDefault(t =>
            t.Key.Equals("exception.message", StringComparison.OrdinalIgnoreCase)).Value;

        return exceptionMessage;
    }

    private static string GetRequestType(Activity activity)
    {
        // Look for mediator-specific tags first
        var requestType = activity.Tags.FirstOrDefault(t =>
            t.Key.Equals("mediator.request_type", StringComparison.OrdinalIgnoreCase) ||
            t.Key.Equals("request_type", StringComparison.OrdinalIgnoreCase)).Value;

        if (!string.IsNullOrEmpty(requestType))
        {
            return requestType;
        }

        // Check if this is a streaming packet
        if (activity.OperationName.Contains("packet_", StringComparison.OrdinalIgnoreCase))
        {
            return "stream_packet";
        }

        // Check if this is a streaming operation
        if (activity.OperationName.Contains("SendStream", StringComparison.OrdinalIgnoreCase))
        {
            return "stream";
        }

        // Fallback to operation name
        return activity.OperationName;
    }

    private static string? GetHandlerName(Activity activity)
    {
        // Look for mediator handler information
        var handlerName = activity.Tags.FirstOrDefault(t =>
            t.Key.Equals("mediator.handler", StringComparison.OrdinalIgnoreCase) ||
            t.Key.Equals("handler.type", StringComparison.OrdinalIgnoreCase)).Value;

        if (!string.IsNullOrEmpty(handlerName))
        {
            return handlerName;
        }

        // Try to infer handler name from request type
        var requestType = GetRequestType(activity);
        if (requestType.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
        {
            return requestType.Replace("Query", "Handler");
        }
        if (requestType.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
        {
            return requestType.Replace("Command", "Handler");
        }

        return null;
    }

    private static string ExtractRequestName(Activity activity)
    {
        // Look for request_name tag first
        var requestName = activity.Tags.FirstOrDefault(t =>
            t.Key.Equals("request_name", StringComparison.OrdinalIgnoreCase)).Value;

        if (!string.IsNullOrEmpty(requestName))
        {
            return requestName;
        }

        var requestType = GetRequestType(activity);

        // For packet activities, extract the request name from the operation name
        if (activity.OperationName.StartsWith("Mediator.SendStream:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = activity.OperationName.Split(':');
            if (parts.Length > 1)
            {
                var requestPart = parts[1];
                var packetIndex = requestPart.IndexOf(".packet_", StringComparison.OrdinalIgnoreCase);
                if (packetIndex > 0)
                {
                    return requestPart[..packetIndex];
                }
                return requestPart;
            }
        }

        // Extract just the class name without namespace
        var lastDot = requestType.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < requestType.Length - 1)
        {
            return requestType[(lastDot + 1)..];
        }

        return requestType;
    }

    private static string DetermineCategory(Activity activity)
    {
        var requestType = GetRequestType(activity);

        // Check for streaming operations
        if (requestType.Equals("stream_packet", StringComparison.OrdinalIgnoreCase))
            return "StreamPacket";
        if (requestType.Equals("stream", StringComparison.OrdinalIgnoreCase))
            return "Stream";

        // Check for standard mediator operations
        if (requestType.EndsWith("Query", StringComparison.OrdinalIgnoreCase))
            return "Query";
        if (requestType.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
            return "Command";
        if (activity.OperationName.Contains("HTTP", StringComparison.OrdinalIgnoreCase))
            return "HTTP";

        return "Activity";
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        // Flush any remaining batches
        FlushStreamingBatch();
        FlushRegularBatch();

        // Process any remaining batches in the queue
        while (_batchQueue.TryDequeue(out var batch))
        {
            try
            {
                ProcessBatch(batch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing final batch during disposal");
            }
        }

        _batchTimer?.Dispose();
        _logger.LogInformation("OpenTelemetryActivityProcessor disposed successfully");
    }
}

/// <summary>
/// Represents a batch of telemetry data for efficient database operations.
/// </summary>
internal sealed class TelemetryBatch
{
    public List<TelemetryTrace> Traces { get; } = new();
    public List<TelemetryActivity> Activities { get; } = new();
    public List<TelemetryMetric> Metrics { get; } = new();
}