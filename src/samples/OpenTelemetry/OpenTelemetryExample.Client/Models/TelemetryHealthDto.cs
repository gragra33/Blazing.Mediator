namespace OpenTelemetryExample.Client.Models;

public sealed class TelemetryHealthDto
{
    public bool IsHealthy { get; set; }
    public bool IsEnabled { get; set; }
    public bool CanRecordMetrics { get; set; }
    public string MeterName { get; set; } = string.Empty;
    public string ActivitySourceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}