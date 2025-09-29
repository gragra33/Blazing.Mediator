using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a trace with its children in a hierarchical structure.
/// </summary>
public sealed class HierarchicalTraceDto
{
    /// <summary>
    /// Gets or sets the trace data for this node.
    /// </summary>
    [JsonPropertyName("trace")]
    public TraceDto Trace { get; set; } = new();

    /// <summary>
    /// Gets or sets the child traces of this node.
    /// </summary>
    [JsonPropertyName("children")]
    public List<HierarchicalTraceDto> Children { get; set; } = new();

    /// <summary>
    /// Gets or sets the hierarchical level of this node in the trace tree.
    /// </summary>
    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this node is expanded in the UI.
    /// </summary>
    [JsonPropertyName("isExpanded")]
    public bool IsExpanded { get; set; } = true;
}