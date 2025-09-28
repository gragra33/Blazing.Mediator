namespace TypedNotificationHybridExample.Middleware;

/// <summary>
/// Inventory-specific notification middleware that only processes IInventoryNotification types.
/// Demonstrates type-constrained middleware in the hybrid pattern.
/// </summary>
public class InventoryNotificationMiddleware(ILogger<InventoryNotificationMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 300; // Execute after customer middleware

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Runtime check for IInventoryNotification interface
        if (notification is IInventoryNotification)
        {
            logger.LogInformation(">>> [INVENTORY-MIDDLEWARE] Processing inventory notification");
            
            if (notification is InventoryUpdatedNotification inventoryUpdated)
            {
                logger.LogInformation("   Product ID: {ProductId}", inventoryUpdated.ProductId);
                logger.LogInformation("   Product Name: {ProductName}", inventoryUpdated.ProductName);
                logger.LogInformation("   Quantity Change: {PreviousQuantity} ? {NewQuantity}", inventoryUpdated.PreviousQuantity, inventoryUpdated.NewQuantity);
                logger.LogInformation("   Reason: {UpdateReason}", inventoryUpdated.UpdateReason);
                
                // Inventory validation logic
                if (inventoryUpdated.NewQuantity < 0)
                {
                    logger.LogWarning("   VALIDATION WARNING: Inventory quantity is negative");
                }
                
                if (inventoryUpdated.NewQuantity <= 5)
                {
                    logger.LogWarning("   LOW STOCK ALERT: Product {ProductName} is running low ({NewQuantity} remaining)", 
                        inventoryUpdated.ProductName, inventoryUpdated.NewQuantity);
                }
            }

            var startTime = DateTime.UtcNow;
            try
            {
                await next(notification, cancellationToken);
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogInformation(">>> [INVENTORY-MIDDLEWARE] Completed successfully in {Duration:F2}ms", duration);
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogError(ex, ">>> [INVENTORY-MIDDLEWARE] Failed after {Duration:F2}ms: {ErrorMessage}", duration, ex.Message);
                throw;
            }
        }
        else
        {
            // Not an inventory notification, pass through without processing
            await next(notification, cancellationToken);
        }
    }
}