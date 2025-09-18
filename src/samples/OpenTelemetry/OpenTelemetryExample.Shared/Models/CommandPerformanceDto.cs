using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents command performance metrics.
/// </summary>
public sealed class CommandPerformanceDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("avgDuration")]
    public double AvgDuration { get; set; }
}