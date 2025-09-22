namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Represents a telemetry log entry stored in the database.
/// This entity captures log messages sent to OpenTelemetry for analysis and monitoring.
/// </summary>
public class TelemetryLog
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
    /// Stored as JSON for flexible querying and analysis.
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
    /// Stored as JSON for complex scope information.
    /// </summary>
    public Dictionary<string, object>? Scopes { get; set; }
}