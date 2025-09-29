namespace TypedNotificationSubscriberExample.Subscribers;

/// <summary>
/// Inventory notification handler that processes inventory-related notifications.
/// This handler demonstrates focused subscription to inventory events.
/// </summary>
public class InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger) :
    INotificationSubscriber<InventoryUpdatedNotification>
{
    public async Task OnNotification(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(25, cancellationToken); // Simulate inventory processing

        logger.LogInformation("* INVENTORY TRACKING UPDATE");
        logger.LogInformation("   Product: {ProductName} ({ProductId})", notification.ProductName, notification.ProductId);
        logger.LogInformation("   Change: {OldQuantity} ? {NewQuantity} (?{ChangeAmount:+#;-#;0})",
            notification.OldQuantity, notification.NewQuantity, notification.ChangeAmount);

        // Check for low stock alerts
        if (notification.NewQuantity <= 10 && notification.NewQuantity > 0)
        {
            logger.LogWarning("*  LOW STOCK ALERT: {ProductName} has only {NewQuantity} units remaining",
                notification.ProductName, notification.NewQuantity);
        }
        else if (notification.NewQuantity <= 0)
        {
            logger.LogError("* OUT OF STOCK: {ProductName} is now out of stock", notification.ProductName);
        }
    }
}