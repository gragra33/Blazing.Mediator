namespace TypedNotificationSubscriberExample.Middleware;

/// <summary>
/// Inventory-specific notification middleware.
/// This middleware ONLY processes notifications that implement IInventoryNotification.
/// </summary>
public class InventoryNotificationMiddleware(ILogger<InventoryNotificationMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 70;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process inventory notifications
        if (notification is IInventoryNotification inventoryNotification)
        {
            var notificationName = typeof(TNotification).Name;
            logger.LogInformation("* Processing INVENTORY notification: {NotificationName} for Product {ProductId} (Qty: {Quantity})",
                notificationName, inventoryNotification.ProductId, inventoryNotification.Quantity);
        }

        await next(notification, cancellationToken);
    }
}