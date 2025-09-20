using OpenTelemetry.Metrics;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Custom OpenTelemetry metrics reader that captures raw metrics data and stores it in the database.
/// This reader periodically collects metrics from all registered meters.
/// </summary>
public sealed class OpenTelemetryMetricsReader : BaseExportingMetricReader
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OpenTelemetryMetricsReader> _logger;
    private readonly Timer _collectTimer;

    public OpenTelemetryMetricsReader(IServiceProvider serviceProvider, ILogger<OpenTelemetryMetricsReader> logger)
        : base(new OpenTelemetryMetricsExporter(serviceProvider))
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Collect metrics every 10 seconds
        _collectTimer = new Timer(async _ => await CollectMetricsAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        _logger.LogInformation("OpenTelemetryMetricsReader initialized with 10-second collection interval");
    }

    private async Task CollectMetricsAsync()
    {
        try
        {
            // Trigger collection of metrics
            Collect(10000); // 10 seconds timeout in milliseconds
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during metrics collection");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _collectTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}