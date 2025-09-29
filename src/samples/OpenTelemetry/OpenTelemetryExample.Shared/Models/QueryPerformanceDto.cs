using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents query performance metrics.
/// </summary>
public sealed class QueryPerformanceDto
{
    /// <summary>
    /// Gets or sets the name of the query.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of times the query was executed.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the average duration of the query execution in milliseconds.
    /// </summary>
    [JsonPropertyName("avgDuration")]
    public double AvgDuration { get; set; }
}