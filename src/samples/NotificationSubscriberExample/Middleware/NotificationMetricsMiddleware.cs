using Blazing.Mediator.Abstractions;

namespace NotificationSubscriberExample.Middleware;

/// <summary>
/// Middleware that tracks metrics and performance for notification processing.
/// This demonstrates how to add monitoring and performance tracking to notifications.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 15; // Execute after logging

    private static readonly Dictionary<string, MetricData> _metrics = new();
    private static readonly object _lock = new();

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
            UpdateMetrics(notificationName, duration.TotalMilliseconds, true);

            logger.LogInformation("$ Metrics updated for {NotificationType}: {Duration}ms",
                notificationName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration.TotalMilliseconds, false);

            logger.LogWarning("$ Metrics updated for failed {NotificationType}: {Duration}ms - {Error}",
                notificationName, duration.TotalMilliseconds, ex.Message);
            throw;
        }
    }

    private static void UpdateMetrics(string notificationName, double durationMs, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(notificationName, out var metrics))
            {
                metrics = new MetricData();
                _metrics[notificationName] = metrics;
            }

            metrics.TotalCount++;
            metrics.TotalDurationMs += durationMs;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            if (durationMs < metrics.MinDurationMs || metrics.MinDurationMs == 0)
                metrics.MinDurationMs = durationMs;

            if (durationMs > metrics.MaxDurationMs)
                metrics.MaxDurationMs = durationMs;
        }
    }

    public static Dictionary<string, MetricData> GetMetrics()
    {
        lock (_lock)
        {
            return new Dictionary<string, MetricData>(_metrics);
        }
    }

    public class MetricData
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double TotalDurationMs { get; set; }
        public double MinDurationMs { get; set; }
        public double MaxDurationMs { get; set; }
        public double AverageDurationMs => TotalCount > 0 ? TotalDurationMs / TotalCount : 0;
    }
}
