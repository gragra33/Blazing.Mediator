namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// General notification logging middleware for all notifications.
/// This middleware logs all notification processing.
/// </summary>
public class GeneralNotificationLoggingMiddleware(ILogger<GeneralNotificationLoggingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        logger.LogInformation("* Processing notification: {NotificationName}", notificationName);

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("- Notification completed: {NotificationName} in {Duration:F1}ms",
                notificationName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "! Notification failed: {NotificationName} after {Duration:F1}ms",
                notificationName, duration.TotalMilliseconds);
            throw;
        }
    }
}