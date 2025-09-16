namespace ECommerce.Api.Services;

/// <summary>
/// Background service that periodically cleans up inactive session statistics.
/// </summary>
public class StatisticsCleanupService : BackgroundService
{
    private readonly MediatorStatisticsTracker _statisticsTracker;
    private readonly ILogger<StatisticsCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _sessionMaxAge = TimeSpan.FromHours(2);

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsCleanupService"/> class.
    /// </summary>
    /// <param name="statisticsTracker">The mediator statistics tracker for cleaning up inactive sessions.</param>
    /// <param name="logger">The logger for recording cleanup service events.</param>
    public StatisticsCleanupService(MediatorStatisticsTracker statisticsTracker, ILogger<StatisticsCleanupService> logger)
    {
        _statisticsTracker = statisticsTracker;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <summary>
    /// Executes the background cleanup service that periodically removes inactive session statistics.
    /// Runs continuously until cancellation is requested, performing cleanup operations at regular intervals.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that signals when the service should stop.</param>
    /// <returns>A task that represents the asynchronous execution of the cleanup service.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Statistics cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _statisticsTracker.CleanupInactiveSessions(_sessionMaxAge);
                _logger.LogDebug("Session statistics cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during session statistics cleanup");
            }

            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Statistics cleanup service stopped");
    }
}