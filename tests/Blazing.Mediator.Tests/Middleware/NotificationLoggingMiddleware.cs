using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Test notification logging middleware for unit tests.
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware>? logger = null)
    : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        logger?.LogInformation("Processing notification: {NotificationType}", typeof(TNotification).Name);
        
        await next(notification, cancellationToken);
        
        logger?.LogInformation("Completed notification: {NotificationType}", typeof(TNotification).Name);
    }
}