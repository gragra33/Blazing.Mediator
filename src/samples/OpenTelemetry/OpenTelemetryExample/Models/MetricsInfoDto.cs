namespace OpenTelemetryExample.Shared.DTOs;

/// <summary>
/// Metrics information DTO.
/// </summary>
public sealed class MetricsInfoDto
{
    /// <summary>
    /// Gets or sets the name of the meter.
    /// </summary>
    public string MeterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the activity source.
    /// </summary>
    public string ActivitySourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether telemetry is enabled.
    /// </summary>
    public bool TelemetryEnabled { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the metrics information.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}