namespace OpenTelemetryExample.Application.Middleware;

public static partial class PerformanceMiddlewareLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Request {RequestType} completed in {Duration}ms")]
    public static partial void LogCompleted(this ILogger logger, string requestType, long duration);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Request {RequestType} failed after {Duration}ms")]
    public static partial void LogFailed(this ILogger logger, string requestType, long duration);
}
