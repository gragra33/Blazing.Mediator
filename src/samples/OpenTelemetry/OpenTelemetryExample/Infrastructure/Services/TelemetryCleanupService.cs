using OpenTelemetryExample.Infrastructure.Telemetry;

namespace OpenTelemetryExample.Infrastructure.Services;

/// <summary>
/// Background service responsible for cleaning up OpenTelemetry resources.
/// Ensures proper disposal of ActivitySource instances during application shutdown.
/// </summary>
public sealed class TelemetryCleanupService : IHostedService
{
    /// <summary>
    /// Starts the telemetry cleanup service.
    /// No action required on startup.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed task</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // No startup actions required
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the telemetry cleanup service and disposes ActivitySource instances.
    /// This ensures proper cleanup of OpenTelemetry resources during application shutdown.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completed task</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Dispose all ActivitySource instances to prevent resource leaks
        ApplicationActivitySources.Dispose();
        return Task.CompletedTask;
    }
}