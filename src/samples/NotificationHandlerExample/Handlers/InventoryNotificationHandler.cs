namespace NotificationHandlerExample.Handlers;

/// <summary>
/// Inventory notification handler that implements INotificationHandler for automatic discovery.
/// This handler manages inventory updates when orders are created.
/// Demonstrates automatic discovery and multiple handlers for the same notification.
/// </summary>
public class InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("[INVENTORY] INVENTORY UPDATE PROCESSING");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);

            // Process each item in the order
            foreach (var item in notification.Items)
            {
                await ProcessInventoryUpdate(item, cancellationToken);
            }

            logger.LogInformation("[+] INVENTORY UPDATE COMPLETED for Order #{OrderId}", notification.OrderId);

            // Simulate checking for low stock alerts
            await CheckForStockAlerts(notification.Items, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] Failed to process inventory update for order {OrderId}", notification.OrderId);
            // Don't rethrow - we don't want to fail the entire notification pipeline for inventory issues
            // This demonstrates graceful error handling in notification handlers
        }
    }

    private async Task ProcessInventoryUpdate(OrderItem item, CancellationToken cancellationToken)
    {
        // Simulate inventory processing delay
        await Task.Delay(50, cancellationToken);

        logger.LogInformation("   [UPDATE] Updating inventory for {ProductName}", item.ProductName);
        logger.LogInformation("      Quantity sold: {Quantity}", item.Quantity);

        // Simulate realistic stock calculation
        var currentStock = Random.Shared.Next(10, 200);
        var newStock = Math.Max(0, currentStock - item.Quantity);

        logger.LogInformation("      Stock: {CurrentStock} -> {NewStock} (change: -{Quantity})", 
            currentStock, newStock, item.Quantity);

        if (newStock == 0)
        {
            logger.LogWarning("   [!] ZERO STOCK: {ProductName}", item.ProductName);
        }
        else if (newStock <= 10)
        {
            logger.LogWarning("   [LOW] LOW STOCK: {ProductName} ({NewStock} remaining)", item.ProductName, newStock);
        }
    }

    private async Task CheckForStockAlerts(List<OrderItem> items, CancellationToken cancellationToken)
    {
        await Task.Delay(25, cancellationToken);

        var alertCount = 0;
        foreach (var item in items)
        {
            // Simulate random stock level scenarios for demonstration
            var stockLevel = Random.Shared.Next(0, 50);

            if (stockLevel <= 10)
            {
                alertCount++;
                if (stockLevel == 0)
                {
                    logger.LogError("[URGENT] OUT OF STOCK ALERT - URGENT");
                    logger.LogError("   Product: {ProductName}", item.ProductName);
                    logger.LogError("   Current Stock: {StockLevel}", stockLevel);
                    logger.LogError("   [ACTION] Action Required: IMMEDIATE REORDER");
                }
                else
                {
                    logger.LogWarning("[WARN] LOW STOCK ALERT");
                    logger.LogWarning("   Product: {ProductName}", item.ProductName);
                    logger.LogWarning("   Current Stock: {StockLevel}", stockLevel);
                    logger.LogWarning("   [TIP] Recommended: Schedule reorder soon");
                }
            }
        }

        if (alertCount == 0)
        {
            logger.LogInformation("   [+] All items have adequate stock levels");
        }
        else
        {
            logger.LogInformation("   [STATS] Stock alerts generated: {AlertCount}", alertCount);
        }
    }
}