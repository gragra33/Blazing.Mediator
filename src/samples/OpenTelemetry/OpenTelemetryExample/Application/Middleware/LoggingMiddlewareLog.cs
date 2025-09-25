namespace OpenTelemetryExample.Application.Middleware;

public static partial class LoggingMiddlewareLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Starting processing of {RequestType}")]
    public static partial void LogStartProcessing(this ILogger logger, string requestType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Request details for {RequestType}: {Request}")]
    public static partial void LogRequestDetails(this ILogger logger, string requestType, object request);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Successfully completed {RequestType} in {ElapsedMs}ms")]
    public static partial void LogCompleted(this ILogger logger, string requestType, long elapsedMs);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to process {RequestType} after {ElapsedMs}ms: {ErrorMessage}")]
    public static partial void LogFailed(this ILogger logger, Exception exception, string requestType, long elapsedMs, string errorMessage);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Starting processing of {RequestType} expecting {ResponseType}")]
    public static partial void LogStartProcessingWithResponse(this ILogger logger, string requestType, string responseType);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Successfully completed {RequestType} -> {ResponseType} in {ElapsedMs}ms")]
    public static partial void LogCompletedWithResponse(this ILogger logger, string requestType, string responseType, long elapsedMs);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Request details for {RequestType}: {Request}")]
    public static partial void LogRequestDetailsWithResponse(this ILogger logger, string requestType, object request);

    [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Response for {RequestType}: {Response}")]
    public static partial void LogResponse(this ILogger logger, string requestType, object response);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error, Message = "Failed to process {RequestType} -> {ResponseType} after {ElapsedMs}ms: {ErrorMessage}")]
    public static partial void LogFailedWithResponse(this ILogger logger, Exception exception, string requestType, string responseType, long elapsedMs, string errorMessage);
}
