using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents the health status of the telemetry system.
/// </summary>
public sealed class TelemetryHealthDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the telemetry system is healthy.
    /// </summary>
    [JsonPropertyName("isHealthy")]
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether telemetry is enabled.
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metrics can be recorded.
    /// </summary>
    [JsonPropertyName("canRecordMetrics")]
    public bool CanRecordMetrics { get; set; }

    /// <summary>
    /// Gets or sets the name of the meter used for telemetry.
    /// </summary>
    [JsonPropertyName("meterName")]
    public string MeterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the activity source for telemetry.
    /// </summary>
    [JsonPropertyName("activitySourceName")]
    public string ActivitySourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message describing the telemetry health status.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}