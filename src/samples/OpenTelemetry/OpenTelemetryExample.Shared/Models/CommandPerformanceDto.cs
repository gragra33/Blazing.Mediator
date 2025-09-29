using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents command performance metrics.
/// </summary>
public sealed class CommandPerformanceDto
{
    /// <summary>
    /// Gets or sets the name of the command.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times the command was executed.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the average duration of the command execution in milliseconds.
    /// </summary>
    [JsonPropertyName("avgDuration")]
    public double AvgDuration { get; set; }
}