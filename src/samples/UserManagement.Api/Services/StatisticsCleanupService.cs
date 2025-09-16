namespace UserManagement.Api.Services;

/// <summary>
/// Background service that periodically cleans up inactive session statistics.
/// </summary>
public class StatisticsCleanupService(ILogger<StatisticsCleanupService> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(2);

    /// <summary>
    /// Executes the background cleanup task.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Statistics cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupInactiveSessions();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during statistics cleanup");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
            }
        }

        logger.LogInformation("Statistics cleanup service stopped");
    }

    private async Task CleanupInactiveSessions()
    {
        using var scope = serviceProvider.CreateScope();
        var statisticsTracker = scope.ServiceProvider.GetRequiredService<MediatorStatisticsTracker>();

        var beforeCount = statisticsTracker.GetAllSessionStatistics().Count;
        statisticsTracker.CleanupInactiveSessions(_sessionTimeout);
        var afterCount = statisticsTracker.GetAllSessionStatistics().Count;

        var removedCount = beforeCount - afterCount;
        if (removedCount > 0)
        {
            logger.LogInformation("Cleaned up {RemovedCount} inactive sessions (timeout: {Timeout})", 
                removedCount, _sessionTimeout);
        }

        await Task.CompletedTask;
    }
}