using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a single OpenTelemetry activity.
/// </summary>
public sealed class ActivityDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the activity.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the parent activity, if any.
    /// </summary>
    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the operation associated with the activity.
    /// </summary>
    [JsonPropertyName("operationName")]
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the activity.
    /// </summary>
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the activity.
    /// </summary>
    [JsonPropertyName("duration")]
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the status of the activity.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind of the activity (e.g., internal, server, client).
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the activity.
    /// </summary>
    [JsonPropertyName("tags")]
    public Dictionary<string, object> Tags { get; set; } = new();
}