using Blazing.Mediator.Abstractions;

namespace SimpleNotificationExample.Middleware;

/// <summary>
/// Middleware that logs notification processing.
/// This demonstrates how to add cross-cutting concerns to notification processing.
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        logger.LogInformation("* Publishing notification: {NotificationName}", notificationName);

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("# Notification completed: {NotificationName} in {Duration}ms",
                notificationName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "! Notification failed: {NotificationName} after {Duration}ms",
                notificationName, duration.TotalMilliseconds);
            throw;
        }
    }
}
