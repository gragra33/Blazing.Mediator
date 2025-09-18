namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// Entity representing a telemetry trace entry.
/// </summary>
public sealed class TelemetryTrace
{
    public int Id { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public string SpanId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, object> Tags { get; set; } = new();
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string? HandlerName { get; set; }
}