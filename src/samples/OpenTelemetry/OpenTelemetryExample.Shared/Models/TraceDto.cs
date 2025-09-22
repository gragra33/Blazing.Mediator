using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a single trace entry.
/// </summary>
public sealed class TraceDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the trace.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier for the span.
    /// </summary>
    [JsonPropertyName("spanId")]
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the parent span, if any.
    /// </summary>
    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the operation being traced.
    /// </summary>
    [JsonPropertyName("operationName")]
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the trace.
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the trace.
    /// </summary>
    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the status of the trace.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the trace.
    /// </summary>
    [JsonPropertyName("tags")]
    public Dictionary<string, object> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the source of the trace.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this trace is a mediator trace.
    /// </summary>
    [JsonPropertyName("isMediatorTrace")]
    public bool IsMediatorTrace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this trace is an application trace.
    /// </summary>
    [JsonPropertyName("isAppTrace")]
    public bool IsAppTrace { get; set; }
}