namespace NotificationHybridExample.Middleware;

/// <summary>
/// Notification metrics middleware for hybrid pattern demonstration.
/// Tracks and reports metrics for all notifications.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 300; // Execute after validation

    private static readonly Dictionary<string, int> _notificationCounts = new();
    private static readonly Dictionary<string, List<double>> _executionTimes = new();

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        // Track notification count
        _notificationCounts.TryGetValue(notificationType, out var count);
        _notificationCounts[notificationType] = count + 1;

        logger.LogInformation(">>> [METRICS] Processing notification #{Count} of type {NotificationType}", 
            _notificationCounts[notificationType], notificationType);

        try
        {
            await next(notification, cancellationToken);
            
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            // Track execution time
            if (!_executionTimes.ContainsKey(notificationType))
                _executionTimes[notificationType] = new List<double>();
            
            _executionTimes[notificationType].Add(executionTime);
            
            // Calculate average
            var avgTime = _executionTimes[notificationType].Average();
            
            logger.LogInformation(">>> [METRICS] Notification completed - Execution: {ExecutionTime:F2}ms, Average: {AverageTime:F2}ms", 
                executionTime, avgTime);
        }
        catch (Exception)
        {
            logger.LogWarning(">>> [METRICS] Notification failed for {NotificationType}", notificationType);
            throw;
        }
    }
}