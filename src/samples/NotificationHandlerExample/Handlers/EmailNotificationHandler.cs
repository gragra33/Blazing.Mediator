namespace NotificationHandlerExample.Handlers;

/// <summary>
/// Email notification handler that implements INotificationHandler for automatic discovery.
/// This handler will be automatically discovered and registered during service registration.
/// Unlike subscribers, this handler doesn't need manual subscription - it's invoked automatically.
/// </summary>
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email processing delay
            await Task.Delay(100, cancellationToken);

            logger.LogInformation("[EMAIL] ORDER CONFIRMATION EMAIL SENT");
            logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);

            foreach (var item in notification.Items)
            {
                logger.LogInformation("   - {ProductName} x{Quantity} @ ${UnitPrice:F2} = ${TotalPrice:F2}",
                    item.ProductName, item.Quantity, item.UnitPrice, item.TotalPrice);
            }

            logger.LogInformation("   Created: {CreatedAt:yyyy-MM-dd HH:mm:ss}", notification.CreatedAt);
            logger.LogInformation("[+] Email notification processed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] Failed to send order confirmation email for order {OrderId}", notification.OrderId);
            // Exception handling - decide whether to rethrow or continue
            throw; // Re-throw to demonstrate error handling in pipeline
        }
    }
}