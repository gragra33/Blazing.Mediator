namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry trace entry.
/// </summary>
public sealed class TelemetryTrace
{
    /// <summary>
    /// Gets or sets the unique identifier for the telemetry trace entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier.
    /// </summary>
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the span identifier.
    /// </summary>
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the operation being traced.
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the trace.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the trace.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the status of the trace (e.g., Success, Error).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the trace.
    /// </summary>
    public Dictionary<string, object> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the type of exception if an error occurred.
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Gets or sets the exception message if an error occurred.
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// Gets or sets the type of request (e.g., HTTP, gRPC).
    /// </summary>
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the handler that processed the request, if available.
    /// </summary>
    public string? HandlerName { get; set; }
}