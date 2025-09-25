namespace NotificationHandlerExample.Middleware;

/// <summary>
/// Notification logging middleware that logs the start and completion of notification processing.
/// Demonstrates automatic middleware discovery and notification pipeline integration.
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 100; // Execute early in the pipeline

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;
        
        logger.LogInformation("[START] Notification Pipeline Started: {NotificationType} at {StartTime:HH:mm:ss.fff}", 
            notificationType, startTime);

        try
        {
            // Execute the next middleware or handlers
            await next(notification, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("[OK] Notification Pipeline Completed: {NotificationType} in {Duration:F2}ms", 
                notificationType, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "[ERROR] Notification Pipeline Failed: {NotificationType} after {Duration:F2}ms", 
                notificationType, duration.TotalMilliseconds);
            throw; // Re-throw to maintain error handling
        }
    }
}