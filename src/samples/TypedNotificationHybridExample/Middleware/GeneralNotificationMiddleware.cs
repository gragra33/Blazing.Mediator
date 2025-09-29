namespace TypedNotificationHybridExample.Middleware;

/// <summary>
/// General notification middleware that processes all notifications.
/// Demonstrates general-purpose middleware in the hybrid pattern.
/// </summary>
public class GeneralNotificationMiddleware(ILogger<GeneralNotificationMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 400; // Execute last

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

        logger.LogInformation(">>> [GENERAL-MIDDLEWARE] Processing notification #{Count} of type {NotificationType}", 
            _notificationCounts[notificationType], notificationType);

        // Add interfaces implemented
        var interfaces = typeof(TNotification).GetInterfaces()
            .Where(i => i != typeof(INotification))
            .Select(i => i.Name)
            .ToList();
            
        if (interfaces.Count > 0)
        {
            logger.LogInformation("   Interfaces: {Interfaces}", string.Join(", ", interfaces));
        }

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
            
            logger.LogInformation(">>> [GENERAL-MIDDLEWARE] Completed - Execution: {ExecutionTime:F2}ms, Average: {AverageTime:F2}ms", 
                executionTime, avgTime);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            logger.LogError(ex, ">>> [GENERAL-MIDDLEWARE] Failed after {Duration:F2}ms for {NotificationType}: {ErrorMessage}", 
                duration, notificationType, ex.Message);
            throw;
        }
    }
}