namespace OpenTelemetryExample.Application.Services;

public static partial class OpenTelemetryActivityProcessorLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error processing activity: {OperationName}")]
    public static partial void LogErrorProcessingActivity(this ILogger logger, Exception exception, string operationName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Error processing telemetry batch with {Count} items")]
    public static partial void LogErrorProcessingBatch(this ILogger logger, Exception exception, int count);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to process telemetry batch with {TraceCount} traces in {Duration}ms")]
    public static partial void LogFailedProcessBatch(this ILogger logger, Exception exception, int traceCount, long duration);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Successfully processed telemetry batch: {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics in {Duration}ms")]
    public static partial void LogProcessedBatch(this ILogger logger, int traceCount, int activityCount, int metricCount, long duration);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Queued streaming batch for processing with {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics")]
    public static partial void LogQueuedStreamingBatch(this ILogger logger, int traceCount, int activityCount, int metricCount);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Queued regular batch for processing with {TraceCount} traces, {ActivityCount} activities, {MetricCount} metrics")]
    public static partial void LogQueuedRegularBatch(this ILogger logger, int traceCount, int activityCount, int metricCount);

    [LoggerMessage(EventId = 7, Level = LogLevel.Trace, Message = "Added streaming telemetry to batch: {OperationName} (batch size: {BatchSize})")]
    public static partial void LogAddedStreamingTelemetry(this ILogger logger, string operationName, int batchSize);

    [LoggerMessage(EventId = 8, Level = LogLevel.Trace, Message = "Added regular telemetry to batch: {OperationName} (batch size: {BatchSize})")]
    public static partial void LogAddedRegularTelemetry(this ILogger logger, string operationName, int batchSize);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "OpenTelemetryActivityProcessor initialized with batching: Streaming({StreamingBatchSize}/{StreamingTimeoutMs}ms), Regular({RegularBatchSize}/{RegularTimeoutMs}ms)")]
    public static partial void LogInitialized(this ILogger logger, int streamingBatchSize, int streamingTimeoutMs, int regularBatchSize, int regularTimeoutMs);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "OpenTelemetryActivityProcessor disposed successfully")]
    public static partial void LogDisposed(this ILogger logger);
}
