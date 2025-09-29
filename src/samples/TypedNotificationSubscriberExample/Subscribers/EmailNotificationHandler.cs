namespace TypedNotificationSubscriberExample.Subscribers;

/// <summary>
/// Email notification handler that processes order and customer notifications.
/// This handler demonstrates selective subscription to multiple notification types.
/// </summary>
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger) :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<OrderStatusChangedNotification>,
    INotificationSubscriber<CustomerRegisteredNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate email processing

        logger.LogInformation("* ORDER CONFIRMATION EMAIL SENT");
        logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
        logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
        logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
        logger.LogInformation("   Items: {ItemCount} items", notification.Items.Count);
    }

    public async Task OnNotification(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate email processing

        logger.LogInformation("* ORDER STATUS UPDATE EMAIL SENT");
        logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
        logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
        logger.LogInformation("   Status: {OldStatus} ? {NewStatus}", notification.OldStatus, notification.NewStatus);
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken); // Simulate email processing

        logger.LogInformation("* WELCOME EMAIL SENT");
        logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
        logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
        logger.LogInformation("   Registered: {RegisteredAt:yyyy-MM-dd HH:mm:ss}", notification.RegisteredAt);
    }
}