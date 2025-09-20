using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OpenTelemetryExample.Application.HealthChecks;

/// <summary>
/// Health check for Blazing.Mediator telemetry system.
/// </summary>
public sealed class MediatorTelemetryHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var health = Blazing.Mediator.OpenTelemetry.MediatorTelemetryHealthCheck.CheckHealth();

            var data = new Dictionary<string, object>
            {
                ["is_enabled"] = health.IsEnabled,
                ["can_record_metrics"] = health.CanRecordMetrics,
                ["meter_name"] = health.MeterName ?? "unknown",
                ["activity_source_name"] = health.ActivitySourceName ?? "unknown"
            };

            return Task.FromResult(health.IsHealthy
                ? HealthCheckResult.Healthy(health.Message, data)
                : HealthCheckResult.Unhealthy(health.Message, data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}"));
        }
    }
}