namespace OpenTelemetryExample.Controllers;

public static partial class LogsControllerLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error retrieving recent logs")]
    public static partial void LogErrorRetrievingRecentLogs(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Error retrieving log with ID: {LogId}")]
    public static partial void LogErrorRetrievingLogById(this ILogger logger, Exception exception, int logId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Error retrieving logs summary")]
    public static partial void LogErrorRetrievingLogsSummary(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Unexpected error during test logging")]
    public static partial void LogUnexpectedErrorDuringTestLogging(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Error during direct database logging test")]
    public static partial void LogErrorDuringDirectDatabaseLoggingTest(this ILogger logger, Exception exception);
}
