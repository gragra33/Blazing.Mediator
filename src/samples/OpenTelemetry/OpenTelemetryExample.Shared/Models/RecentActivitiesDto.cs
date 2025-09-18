using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents recent activity data for the dashboard.
/// </summary>
public sealed class RecentActivitiesDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("activities")]
    public List<ActivityDto> Activities { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}