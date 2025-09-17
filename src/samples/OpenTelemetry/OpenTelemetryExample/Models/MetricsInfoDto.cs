namespace OpenTelemetryExample.Shared.DTOs;

/// <summary>
/// Metrics information DTO.
/// </summary>
public sealed class MetricsInfoDto
{
    public string MeterName { get; set; } = string.Empty;
    public string ActivitySourceName { get; set; } = string.Empty;
    public bool TelemetryEnabled { get; set; }
    public string Message { get; set; } = string.Empty;
}