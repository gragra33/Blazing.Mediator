namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Error handling middleware for notifications.
/// This middleware handles exceptions in notification processing.
/// </summary>
public class NotificationErrorHandlingMiddleware(ILogger<NotificationErrorHandlingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => int.MinValue; // Execute first

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        try
        {
            await next(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogError(ex, "! Error in notification pipeline for {NotificationName}: {Message}",
                notificationName, ex.Message);

            // Don't rethrow - we want notifications to be resilient
            // In a real system, you might want to store failed notifications for retry
        }
    }
}