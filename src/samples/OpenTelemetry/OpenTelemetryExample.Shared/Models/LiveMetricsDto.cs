using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents live telemetry metrics data for the dashboard.
/// </summary>
public sealed class LiveMetricsDto
{
    /// <summary>
    /// Gets or sets the timestamp when the metrics were captured.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the system metrics data.
    /// </summary>
    [JsonPropertyName("metrics")]
    public MetricsData Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of command performance metrics.
    /// </summary>
    [JsonPropertyName("commands")]
    public List<CommandPerformanceDto> Commands { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of query performance metrics.
    /// </summary>
    [JsonPropertyName("queries")]
    public List<QueryPerformanceDto> Queries { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional message related to the metrics.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}