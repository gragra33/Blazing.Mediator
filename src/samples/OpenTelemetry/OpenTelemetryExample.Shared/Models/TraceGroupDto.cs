using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a group of related traces organized by TraceId and hierarchy.
/// </summary>
public sealed class TraceGroupDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the trace group.
    /// </summary>
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root trace of the group.
    /// </summary>
    [JsonPropertyName("rootTrace")]
    public TraceDto RootTrace { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of child traces in hierarchical structure.
    /// </summary>
    [JsonPropertyName("childTraces")]
    public List<HierarchicalTraceDto> ChildTraces { get; set; } = new();

    /// <summary>
    /// Gets or sets the total duration of all traces in the group.
    /// </summary>
    [JsonPropertyName("totalDuration")]
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// Gets or sets the status of the trace group.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the trace group.
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the total number of traces in the group.
    /// </summary>
    [JsonPropertyName("traceCount")]
    public int TraceCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the trace group is expanded in the UI.
    /// </summary>
    [JsonPropertyName("isExpanded")]
    public bool IsExpanded { get; set; } = true;
}