namespace SimpleNotificationExample.Subscribers;

/// <summary>
/// Simple inventory handler that implements INotificationSubscriber.
/// This is a regular scoped class that handles order notifications for inventory management.
/// Unlike the background service version, this is a simple scoped service that processes notifications on-demand.
/// </summary>
public class InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("- INVENTORY UPDATE PROCESSING");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);

            // Process each item in the order
            foreach (var item in notification.Items)
            {
                await ProcessInventoryUpdate(item, cancellationToken);
            }

            logger.LogInformation("- INVENTORY UPDATE COMPLETED for Order #{OrderId}", notification.OrderId);

            // Simulate checking for low stock alerts
            await CheckForStockAlerts(notification.Items, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "! Failed to process inventory update for order {OrderId}", notification.OrderId);
            // Don't rethrow - we don't want to fail the entire notification pipeline
        }
    }

    private async Task ProcessInventoryUpdate(OrderItem item, CancellationToken cancellationToken)
    {
        // Simulate inventory processing delay
        await Task.Delay(50, cancellationToken);

        logger.LogInformation("   - Updating inventory for {ProductName} (ID: {ProductId})", 
            item.ProductName, item.ProductId);
        logger.LogInformation("      Quantity sold: {Quantity}", item.Quantity);
        
        // Simulate stock calculation
        var currentStock = Random.Shared.Next(0, 100);
        var newStock = Math.Max(0, currentStock - item.Quantity);
        
        logger.LogInformation("      Stock before: {CurrentStock}, after: {NewStock}", currentStock, newStock);
    }

    private async Task CheckForStockAlerts(List<OrderItem> items, CancellationToken cancellationToken)
    {
        await Task.Delay(25, cancellationToken);

        foreach (var item in items)
        {
            // Simulate random low stock scenarios for demonstration
            var stockLevel = Random.Shared.Next(0, 50);
            
            if (stockLevel <= 10)
            {
                if (stockLevel == 0)
                {
                    logger.LogError("! OUT OF STOCK ALERT - URGENT");
                    logger.LogError("   Product: {ProductName} (ID: {ProductId})", item.ProductName, item.ProductId);
                    logger.LogError("   Current Stock: {StockLevel}", stockLevel);
                    logger.LogError("   Action Required: IMMEDIATE REORDER");
                }
                else
                {
                    logger.LogWarning("*  LOW STOCK ALERT");
                    logger.LogWarning("   Product: {ProductName} (ID: {ProductId})", item.ProductName, item.ProductId);
                    logger.LogWarning("   Current Stock: {StockLevel}", stockLevel);
                    logger.LogWarning("   Recommended Action: Schedule reorder");
                }
            }
        }
    }
}
