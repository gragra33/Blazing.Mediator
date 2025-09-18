namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry metric entry.
/// </summary>
public sealed class TelemetryMetric
{
    public int Id { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string RequestName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Command or Query
    public double Duration { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public string? HandlerName { get; set; }
}