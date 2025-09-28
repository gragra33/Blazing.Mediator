namespace TypedNotificationHybridExample.Handlers;

/// <summary>
/// Email notification handler that implements INotificationHandler for automatic discovery.
/// Handles both order and customer notifications automatically.
/// Part of the AUTOMATIC HANDLERS in the typed hybrid approach.
/// </summary>
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger) 
    : INotificationHandler<OrderCreatedNotification>,
      INotificationHandler<CustomerRegisteredNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(100, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] ORDER CONFIRMATION EMAIL SENT");
            logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);

            foreach (var item in notification.Items)
            {
                logger.LogInformation("   - {ProductName} x{Quantity} @ ${UnitPrice:F2}",
                    item.ProductName, item.Quantity, item.UnitPrice);
            }

            logger.LogInformation("[+] AUTO-HANDLER: Order email notification processed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed to send order confirmation email for order {OrderId}", notification.OrderId);
            throw;
        }
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(80, cancellationToken);

            logger.LogInformation("[AUTO-HANDLER] WELCOME EMAIL SENT");
            logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Customer ID: {CustomerId}", notification.CustomerId);
            logger.LogInformation("   Registration Source: {RegistrationSource}", notification.RegistrationSource);
            logger.LogInformation("   Registered: {RegisteredAt:yyyy-MM-dd HH:mm:ss}", notification.RegisteredAt);

            logger.LogInformation("[+] AUTO-HANDLER: Welcome email notification processed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[-] AUTO-HANDLER: Failed to send welcome email for customer {CustomerId}", notification.CustomerId);
            throw;
        }
    }
}