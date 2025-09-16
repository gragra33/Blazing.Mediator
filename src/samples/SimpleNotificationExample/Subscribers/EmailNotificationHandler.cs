namespace SimpleNotificationExample.Subscribers;

/// <summary>
/// Email notification handler that implements INotificationSubscriber.
/// This is a regular class that handles order notifications by sending email confirmations.
/// This demonstrates the simplest way to subscribe to notifications.
/// </summary>
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email processing delay
            await Task.Delay(100, cancellationToken);

            logger.LogInformation("# ORDER CONFIRMATION EMAIL SENT");
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

            logger.LogInformation("   Created: {CreatedAt:yyyy-MM-dd HH:mm:ss}", notification.CreatedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
            // Don't rethrow - we don't want to fail the entire notification pipeline
        }
    }
}
