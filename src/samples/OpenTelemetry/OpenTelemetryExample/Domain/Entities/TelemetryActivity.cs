namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry activity entry.
/// </summary>
public sealed class TelemetryActivity
{
    /// <summary>
    /// Gets or sets the unique identifier for the telemetry activity entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the activity.
    /// </summary>
    public string ActivityId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent activity identifier, if this is a child activity.
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the name of the operation associated with the activity.
    /// </summary>
    public string OperationName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the activity.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the activity.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the status of the activity.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the kind of the activity (e.g., client, server).
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags associated with the activity.
    /// </summary>
    public Dictionary<string, object> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the type of request associated with the activity.
    /// </summary>
    public string RequestType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the handler that processed the activity, if any.
    /// </summary>
    public string? HandlerName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the activity was successful.
    /// </summary>
    public bool IsSuccess { get; set; }
}