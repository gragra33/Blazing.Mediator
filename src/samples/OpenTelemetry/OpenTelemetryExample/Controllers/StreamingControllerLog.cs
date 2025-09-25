namespace OpenTelemetryExample.Controllers;

public static partial class StreamingControllerLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Streaming health check failed")]
    public static partial void LogStreamingHealthCheckFailed(this ILogger logger, Exception exception);
}
