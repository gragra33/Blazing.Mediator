using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents recent trace data for the dashboard.
/// </summary>
public sealed class RecentTracesDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("traces")]
    public List<TraceDto> Traces { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("totalTracesInTimeframe")]
    public int TotalTracesInTimeframe { get; set; }
}