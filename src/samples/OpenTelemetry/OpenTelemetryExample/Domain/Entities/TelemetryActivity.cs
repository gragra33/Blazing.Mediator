namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry activity entry.
/// </summary>
public sealed class TelemetryActivity
{
    public int Id { get; set; }
    public string ActivityId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public Dictionary<string, object> Tags { get; set; } = new();
    public string RequestType { get; set; } = string.Empty;
    public string? HandlerName { get; set; }
    public bool IsSuccess { get; set; }
}