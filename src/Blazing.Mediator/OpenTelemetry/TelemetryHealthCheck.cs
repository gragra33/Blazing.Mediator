namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Health check for Blazing.Mediator telemetry system.
/// </summary>
public sealed class TelemetryHealthCheck
{
    /// <summary>
    /// Checks the health of the telemetry system.
    /// </summary>
    /// <returns>A health check result.</returns>
    public static TelemetryHealthResult CheckHealth()
    {
        try
        {
            var isEnabled = Mediator.TelemetryEnabled;
            var isWorking = Mediator.GetTelemetryHealth();

            return new TelemetryHealthResult
            {
                IsHealthy = isEnabled && isWorking,
                IsEnabled = isEnabled,
                CanRecordMetrics = isWorking,
                MeterName = Mediator.Meter.Name,
                ActivitySourceName = Mediator.ActivitySource.Name,
                Message = isEnabled && isWorking
                    ? "Telemetry is enabled and working correctly"
                    : isEnabled
                        ? "Telemetry is enabled but not working correctly"
                        : "Telemetry is disabled"
            };
        }
        catch (Exception ex)
        {
            return new TelemetryHealthResult
            {
                IsHealthy = false,
                IsEnabled = false,
                CanRecordMetrics = false,
                Message = $"Telemetry health check failed: {ex.Message}"
            };
        }
    }
}