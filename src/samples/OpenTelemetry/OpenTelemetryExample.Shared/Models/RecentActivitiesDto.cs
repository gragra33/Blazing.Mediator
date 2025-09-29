using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents recent activity data for the dashboard.
/// </summary>
public sealed class RecentActivitiesDto
{
    /// <summary>
    /// Gets or sets the timestamp of the recent activities.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the list of recent activities.
    /// </summary>
    [JsonPropertyName("activities")]
    public List<ActivityDto> Activities { get; set; } = new();

    /// <summary>
    /// Gets or sets the message associated with the recent activities.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}