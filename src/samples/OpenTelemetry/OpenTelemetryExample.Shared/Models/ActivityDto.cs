using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a single OpenTelemetry activity.
/// </summary>
public sealed class ActivityDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("operationName")]
    public string OperationName { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public Dictionary<string, object> Tags { get; set; } = new();
}