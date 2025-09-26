namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Customer-specific notification middleware.
/// This middleware ONLY processes notifications that implement ICustomerNotification.
/// </summary>
public class CustomerNotificationMiddleware(ILogger<CustomerNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 60;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process customer notifications
        if (notification is ICustomerNotification customerNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing CUSTOMER notification: {NotificationName} for {CustomerName}",
                notificationName, customerNotification.CustomerName);
        }

        await next(notification, cancellationToken);
    }
}