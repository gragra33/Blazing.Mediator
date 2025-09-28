namespace NotificationHybridExample.Handlers;

/// <summary>
/// Shipping notification handler that implements INotificationHandler for automatic discovery.
/// This handler manages order fulfillment and shipping logistics.
/// Part of the AUTOMATIC HANDLERS in the hybrid approach.
/// </summary>
public class ShippingNotificationHandler(ILogger<ShippingNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate shipping processing delay
            await Task.Delay(150, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] SHIPPING PROCESSING INITIATED");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Shipping Address Required: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Order Value: ${TotalAmount:F2}", notification.TotalAmount);

            // Simulate shipping logic based on order total
            var shippingType = notification.TotalAmount > 100 ? "Express" : "Standard";
            var estimatedDays = notification.TotalAmount > 100 ? 1 : 3;

            logger.LogInformation("   Shipping Type: {ShippingType} ({EstimatedDays} business days)", 
                shippingType, estimatedDays);

            // Log items for shipping
            logger.LogInformation("   Items to ship:");
            foreach (var item in notification.Items)
            {
                logger.LogInformation("     - {ProductName} x{Quantity}", item.ProductName, item.Quantity);
            }

            logger.LogInformation("   Fulfillment Center: Assigned automatically based on location");
            logger.LogInformation("   Tracking Number: TRACK-{OrderId}-{RandomSuffix}", 
                notification.OrderId, Random.Shared.Next(1000, 9999));

            logger.LogInformation("[+] AUTO-HANDLER: Shipping processing completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed to process shipping for order {OrderId}", notification.OrderId);
            throw; // Re-throw to demonstrate error handling in pipeline
        }
    }
}