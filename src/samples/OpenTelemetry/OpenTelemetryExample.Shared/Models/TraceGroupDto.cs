using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a group of related traces organized by TraceId and hierarchy.
/// </summary>
public sealed class TraceGroupDto
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; } = string.Empty;

    [JsonPropertyName("rootTrace")]
    public TraceDto RootTrace { get; set; } = new();

    [JsonPropertyName("childTraces")]
    public List<HierarchicalTraceDto> ChildTraces { get; set; } = new();

    [JsonPropertyName("totalDuration")]
    public TimeSpan TotalDuration { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("traceCount")]
    public int TraceCount { get; set; }

    [JsonPropertyName("isExpanded")]
    public bool IsExpanded { get; set; } = true;
}