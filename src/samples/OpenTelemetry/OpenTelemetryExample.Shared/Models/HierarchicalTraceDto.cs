using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a trace with its children in a hierarchical structure.
/// </summary>
public sealed class HierarchicalTraceDto
{
    [JsonPropertyName("trace")]
    public TraceDto Trace { get; set; } = new();

    [JsonPropertyName("children")]
    public List<HierarchicalTraceDto> Children { get; set; } = new();

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("isExpanded")]
    public bool IsExpanded { get; set; } = true;
}