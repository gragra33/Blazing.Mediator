using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents live telemetry metrics data for the dashboard.
/// </summary>
public sealed class LiveMetricsDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("metrics")]
    public MetricsData Metrics { get; set; } = new();

    [JsonPropertyName("commands")]
    public List<CommandPerformanceDto> Commands { get; set; } = new();

    [JsonPropertyName("queries")]
    public List<QueryPerformanceDto> Queries { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}