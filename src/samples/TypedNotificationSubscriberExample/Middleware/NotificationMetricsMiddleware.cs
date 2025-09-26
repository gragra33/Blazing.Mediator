namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Metrics tracking middleware for notifications.
/// This middleware tracks notification performance metrics.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    : INotificationMiddleware
{
    private static readonly Dictionary<string, NotificationMetrics> _metrics = new();
    private static readonly object _lock = new();

    public int Order => 100;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: true);
        }
        catch (Exception)
        {
            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: false);
            throw;
        }
    }

    private void UpdateMetrics(string notificationName, TimeSpan duration, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(notificationName))
            {
                _metrics[notificationName] = new NotificationMetrics();
            }

            var metrics = _metrics[notificationName];
            metrics.TotalCount++;
            metrics.TotalDuration += duration;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            // Log metrics periodically
            if (metrics.TotalCount % 5 == 0) // Every 5 notifications
            {
                logger.LogInformation("* Metrics for {NotificationName}: " +
                                      "Total: {Total}, Success: {Success}, Failures: {Failures}, Avg: {AvgDuration:F1}ms",
                    notificationName, metrics.TotalCount, metrics.SuccessCount, metrics.FailureCount,
                    metrics.AverageDuration.TotalMilliseconds);
            }
        }
    }

    public static Dictionary<string, NotificationMetrics> GetMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, NotificationMetrics>(_metrics);
        }
    }
}