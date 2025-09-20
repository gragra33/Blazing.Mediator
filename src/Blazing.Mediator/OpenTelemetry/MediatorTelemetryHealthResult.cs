namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Result of a telemetry health check.
/// </summary>
public sealed class MediatorTelemetryHealthResult
{
    /// <summary>
    /// Gets or sets whether the telemetry system is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets whether telemetry is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether metrics can be recorded.
    /// </summary>
    public bool CanRecordMetrics { get; set; }

    /// <summary>
    /// Gets or sets the name of the OpenTelemetry meter.
    /// </summary>
    public string? MeterName { get; set; }

    /// <summary>
    /// Gets or sets the name of the OpenTelemetry activity source.
    /// </summary>
    public string? ActivitySourceName { get; set; }

    /// <summary>
    /// Gets or sets a descriptive message about the health status.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}