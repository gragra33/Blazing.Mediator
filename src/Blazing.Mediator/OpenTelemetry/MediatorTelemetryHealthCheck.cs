namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Health check for Blazing.Mediator telemetry system.
/// </summary>
public sealed class MediatorTelemetryHealthCheck
{
    /// <summary>
    /// Checks the health of the telemetry system.
    /// </summary>
    /// <returns>A health check result.</returns>
    public static MediatorTelemetryHealthResult CheckHealth()
    {
        try
        {
            var isEnabled = Mediator.TelemetryEnabled;
            var isWorking = Mediator.GetTelemetryHealth();
            
            return new MediatorTelemetryHealthResult
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
            return new MediatorTelemetryHealthResult
            {
                IsHealthy = false,
                IsEnabled = false,
                CanRecordMetrics = false,
                Message = $"Telemetry health check failed: {ex.Message}"
            };
        }
    }
}