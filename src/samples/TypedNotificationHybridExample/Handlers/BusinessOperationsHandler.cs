namespace TypedNotificationHybridExample.Handlers;

/// <summary>
/// Business operations handler that implements INotificationHandler for automatic discovery.
/// Handles multiple notification types with different business logic.
/// Part of the AUTOMATIC HANDLERS in the typed hybrid approach.
/// </summary>
public class BusinessOperationsHandler(ILogger<BusinessOperationsHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>,
      INotificationHandler<CustomerRegisteredNotification>,
      INotificationHandler<InventoryUpdatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(120, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] BUSINESS OPERATIONS - ORDER PROCESSING");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Business Rule: Order value analysis");
            
            // Apply business rules based on order value
            if (notification.TotalAmount > 200)
            {
                logger.LogInformation("   - HIGH VALUE ORDER: Flagged for priority processing");
                logger.LogInformation("   - Business Action: Assign premium support representative");
            }
            else if (notification.TotalAmount > 100)
            {
                logger.LogInformation("   - MEDIUM VALUE ORDER: Standard processing");
                logger.LogInformation("   - Business Action: Standard fulfillment queue");
            }
            else
            {
                logger.LogInformation("   - STANDARD ORDER: Automated processing");
                logger.LogInformation("   - Business Action: Batch fulfillment queue");
            }

            logger.LogInformation("   - Revenue Impact: +${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("[+] AUTO-HANDLER: Order business operations completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed business operations for order {OrderId}", notification.OrderId);
            throw;
        }
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(90, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] BUSINESS OPERATIONS - CUSTOMER ONBOARDING");
            logger.LogInformation("   Customer: {CustomerName} ({CustomerId})", notification.CustomerName, notification.CustomerId);
            
            // Business logic for customer onboarding
            logger.LogInformation("   - Customer Lifecycle: New customer onboarding initiated");
            logger.LogInformation("   - Business Action: Create customer profile and preferences");
            logger.LogInformation("   - Marketing Action: Add to new customer welcome campaign");
            
            // Registration source specific logic
            switch (notification.RegistrationSource.ToLower())
            {
                case "website":
                    logger.LogInformation("   - Channel: Web registration - assign web specialist");
                    break;
                case "mobile":
                    logger.LogInformation("   - Channel: Mobile registration - enable mobile notifications");
                    break;
                case "referral":
                    logger.LogInformation("   - Channel: Referral registration - process referral rewards");
                    break;
                default:
                    logger.LogInformation("   - Channel: {Source} registration - standard processing", notification.RegistrationSource);
                    break;
            }

            logger.LogInformation("[+] AUTO-HANDLER: Customer business operations completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed business operations for customer {CustomerId}", notification.CustomerId);
            throw;
        }
    }

    public async Task Handle(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(75, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] BUSINESS OPERATIONS - INVENTORY ANALYSIS");
            logger.LogInformation("   Product: {ProductName} ({ProductId})", notification.ProductName, notification.ProductId);
            logger.LogInformation("   Change: {PreviousQuantity} ? {NewQuantity} ({Reason})", 
                notification.PreviousQuantity, notification.NewQuantity, notification.UpdateReason);

            // Business logic for inventory changes
            var quantityChange = notification.NewQuantity - notification.PreviousQuantity;
            
            if (quantityChange > 0)
            {
                logger.LogInformation("   - Stock Increase: +{Increase} units", quantityChange);
                logger.LogInformation("   - Business Action: Update product availability status");
            }
            else if (quantityChange < 0)
            {
                logger.LogInformation("   - Stock Decrease: {Decrease} units", Math.Abs(quantityChange));
                
                if (notification.NewQuantity <= 5)
                {
                    logger.LogWarning("   - LOW STOCK ALERT: Reorder threshold reached!");
                    logger.LogInformation("   - Business Action: Trigger automatic reorder process");
                }
            }

            if (notification.NewQuantity == 0)
            {
                logger.LogWarning("   - OUT OF STOCK: Product unavailable");
                logger.LogInformation("   - Business Action: Update website and notify customers");
            }

            logger.LogInformation("[+] AUTO-HANDLER: Inventory business operations completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed business operations for product {ProductId}", notification.ProductId);
            throw;
        }
    }
}