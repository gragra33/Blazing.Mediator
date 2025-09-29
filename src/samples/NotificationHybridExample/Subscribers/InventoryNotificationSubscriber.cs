namespace NotificationHybridExample.Subscribers;

/// <summary>
/// Inventory notification subscriber that implements INotificationSubscriber.
/// This subscriber handles stock updates and inventory management.
/// Part of the MANUAL SUBSCRIBERS in the hybrid approach - requires manual subscription.
/// </summary>
public class InventoryNotificationSubscriber(ILogger<InventoryNotificationSubscriber> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate inventory processing delay
            await Task.Delay(120, cancellationToken);

            logger.LogInformation("[MANUAL-SUBSCRIBER] INVENTORY UPDATE PROCESSING");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);
            
            // Process each item for inventory updates
            foreach (var item in notification.Items)
            {
                logger.LogInformation("   Processing item: {ProductName}", item.ProductName);
                logger.LogInformation("     - Quantity ordered: {Quantity}", item.Quantity);
                logger.LogInformation("     - Updating stock levels...");
                logger.LogInformation("     - Checking reorder thresholds...");
                
                // Simulate low stock alert
                if (item.Quantity > 5)
                {
                    logger.LogWarning("     - LOW STOCK ALERT: {ProductName} quantity is getting low!", item.ProductName);
                }
            }

            logger.LogInformation("   Stock Location: Warehouse-A (Primary)");
            logger.LogInformation("   Reserve Status: Items reserved for order #{OrderId}", notification.OrderId);
            logger.LogInformation("   Inventory System: Updated successfully");
            logger.LogInformation("[+] MANUAL-SUBSCRIBER: Inventory processing completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] MANUAL-SUBSCRIBER: Failed to update inventory for order {OrderId}", notification.OrderId);
            // Don't rethrow - we don't want to fail the entire notification pipeline for subscribers
        }
    }
}