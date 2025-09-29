namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Data transfer object for telemetry log entries.
/// Represents a single log entry with associated metadata for display in the client.
/// </summary>
public class LogDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the log was created.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level (e.g., Information, Warning, Error, Debug).
    /// </summary>
    public string LogLevel { get; set; } = null!;

    /// <summary>
    /// Gets or sets the category/logger name that generated the log.
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Gets or sets the exception details if the log was generated due to an exception.
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Gets or sets the trace ID associated with this log entry for correlation.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the span ID associated with this log entry for correlation.
    /// </summary>
    public string? SpanId { get; set; }

    /// <summary>
    /// Gets or sets the source that generated the log (e.g., Application, Mediator, Controller).
    /// </summary>
    public string Source { get; set; } = null!;

    /// <summary>
    /// Gets or sets additional properties/tags associated with the log entry.
    /// </summary>
    public Dictionary<string, object> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the machine name where the log was generated.
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Gets or sets the process ID that generated the log.
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// Gets or sets the thread ID that generated the log.
    /// </summary>
    public int? ThreadId { get; set; }

    /// <summary>
    /// Gets or sets the event ID associated with the log entry.
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// Gets or sets the scopes associated with the log entry.
    /// </summary>
    public Dictionary<string, object>? Scopes { get; set; }

    /// <summary>
    /// Gets a value indicating whether this log is related to application code.
    /// </summary>
    public bool IsAppLog => Source is "Application" or "Controller" or "Mediator";

    /// <summary>
    /// Gets a value indicating whether this log is from the Blazing.Mediator framework.
    /// </summary>
    public bool IsMediatorLog => Source == "Mediator";

    /// <summary>
    /// Gets a value indicating whether this log contains an exception.
    /// </summary>
    public bool HasException => !string.IsNullOrEmpty(Exception);

    /// <summary>
    /// Gets a human-readable relative time string (e.g., "2 minutes ago").
    /// </summary>
    public string RelativeTime
    {
        get
        {
            var timeSpan = DateTime.UtcNow - Timestamp;
            return timeSpan.TotalSeconds switch
            {
                < 60 => $"{(int)timeSpan.TotalSeconds} seconds ago",
                < 3600 => $"{(int)timeSpan.TotalMinutes} minutes ago",
                < 86400 => $"{(int)timeSpan.TotalHours} hours ago",
                _ => $"{(int)timeSpan.TotalDays} days ago"
            };
        }
    }
}

/// <summary>
/// Data transfer object for recent logs response with pagination.
/// </summary>
public class RecentLogsDto
{
    /// <summary>
    /// Gets or sets the collection of recent log entries.
    /// </summary>
    public IEnumerable<LogDto> Logs { get; set; } = [];

    /// <summary>
    /// Gets or sets the pagination information.
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();

    /// <summary>
    /// Gets or sets the applied filters for the logs.
    /// </summary>
    public LogFilters Filters { get; set; } = new();

    /// <summary>
    /// Gets or sets summary statistics for the current log set.
    /// </summary>
    public LogSummary Summary { get; set; } = new();
}

/// <summary>
/// Represents filters applied to log queries.
/// </summary>
public class LogFilters
{
    /// <summary>
    /// Gets or sets the time window in minutes for filtering logs.
    /// </summary>
    public int TimeWindowMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to show only application logs.
    /// </summary>
    public bool AppOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show only Mediator logs.
    /// </summary>
    public bool MediatorOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show only logs with exceptions.
    /// </summary>
    public bool ErrorsOnly { get; set; }

    /// <summary>
    /// Gets or sets the minimum log level to include.
    /// </summary>
    public string? MinimumLogLevel { get; set; }

    /// <summary>
    /// Gets or sets the search text for filtering log messages.
    /// </summary>
    public string? SearchText { get; set; }
}

/// <summary>
/// Represents summary statistics for a set of logs.
/// </summary>
public class LogSummary
{
    /// <summary>
    /// Gets or sets the total number of logs in the current filter set.
    /// </summary>
    public int TotalLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of error logs.
    /// </summary>
    public int ErrorLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of warning logs.
    /// </summary>
    public int WarningLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of information logs.
    /// </summary>
    public int InfoLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of debug logs.
    /// </summary>
    public int DebugLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of logs with exceptions.
    /// </summary>
    public int LogsWithExceptions { get; set; }

    /// <summary>
    /// Gets or sets the number of application-specific logs.
    /// </summary>
    public int AppLogs { get; set; }

    /// <summary>
    /// Gets or sets the number of Mediator-specific logs.
    /// </summary>
    public int MediatorLogs { get; set; }
}