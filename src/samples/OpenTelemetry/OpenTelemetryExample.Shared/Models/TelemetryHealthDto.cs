using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents the health status of the telemetry system.
/// </summary>
public sealed class TelemetryHealthDto
{
    [JsonPropertyName("isHealthy")]
    public bool IsHealthy { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("canRecordMetrics")]
    public bool CanRecordMetrics { get; set; }

    [JsonPropertyName("meterName")]
    public string MeterName { get; set; } = string.Empty;

    [JsonPropertyName("activitySourceName")]
    public string ActivitySourceName { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}