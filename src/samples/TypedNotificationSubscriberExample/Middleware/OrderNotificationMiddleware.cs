using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using TypedNotificationSubscriberExample.Notifications;

namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Order-specific notification middleware.
/// This middleware ONLY processes notifications that implement IOrderNotification.
/// </summary>
public class OrderNotificationMiddleware(ILogger<OrderNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 50;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Runtime check for IOrderNotification interface
        if (notification is IOrderNotification orderNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing ORDER notification: {NotificationName} for Order {OrderId}",
                notificationName, orderNotification.OrderId);
        }

        await next(notification, cancellationToken);
    }
}