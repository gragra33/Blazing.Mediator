namespace TypedSimpleNotificationExample.Subscribers;

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

/// <summary>
/// Inventory notification handler that processes inventory-related notifications.
/// This handler demonstrates focused subscription to inventory events.
/// </summary>
public class InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger) :
    INotificationSubscriber<InventoryUpdatedNotification>
{
    public async Task OnNotification(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(25, cancellationToken); // Simulate inventory processing

        logger.LogInformation("* INVENTORY TRACKING UPDATE");
        logger.LogInformation("   Product: {ProductName} ({ProductId})", notification.ProductName, notification.ProductId);
        logger.LogInformation("   Change: {OldQuantity} ? {NewQuantity} (?{ChangeAmount:+#;-#;0})", 
            notification.OldQuantity, notification.NewQuantity, notification.ChangeAmount);

        // Check for low stock alerts
        if (notification.NewQuantity <= 10 && notification.NewQuantity > 0)
        {
            logger.LogWarning("*  LOW STOCK ALERT: {ProductName} has only {NewQuantity} units remaining", 
                notification.ProductName, notification.NewQuantity);
        }
        else if (notification.NewQuantity <= 0)
        {
            logger.LogError("* OUT OF STOCK: {ProductName} is now out of stock", notification.ProductName);
        }
    }
}

/// <summary>
/// Business operations handler that processes order notifications for business logic.
/// This handler demonstrates cross-cutting business concerns.
/// </summary>
public class BusinessOperationsHandler(ILogger<BusinessOperationsHandler> logger) :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<CustomerRegisteredNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken); // Simulate business processing

        logger.LogInformation("* BUSINESS OPERATIONS UPDATE");
        logger.LogInformation("   Order #{OrderId} processed for business metrics", notification.OrderId);
        logger.LogInformation("   Revenue: ${TotalAmount:F2} recorded", notification.TotalAmount);
        
        // Calculate some business metrics
        var averageItemValue = notification.TotalAmount / notification.Items.Count;
        logger.LogInformation("   Average item value: ${AverageValue:F2}", averageItemValue);
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(30, cancellationToken); // Simulate business processing

        logger.LogInformation("* NEW CUSTOMER ONBOARDING");
        logger.LogInformation("   Customer: {CustomerName} ({CustomerEmail})", 
            notification.CustomerName, notification.CustomerEmail);
        logger.LogInformation("   Customer database updated for analytics");
    }
}

/// <summary>
/// Audit handler that processes all notifications for audit purposes.
/// This handler demonstrates universal notification monitoring.
/// </summary>
public class AuditNotificationHandler(ILogger<AuditNotificationHandler> logger) :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<OrderStatusChangedNotification>,
    INotificationSubscriber<CustomerRegisteredNotification>,
    INotificationSubscriber<InventoryUpdatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("ORDER_CREATED", $"Order {notification.OrderId} created for {notification.CustomerEmail}", cancellationToken);
    }

    public async Task OnNotification(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("ORDER_STATUS_CHANGED", $"Order {notification.OrderId} status: {notification.OldStatus} ? {notification.NewStatus}", cancellationToken);
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("CUSTOMER_REGISTERED", $"Customer {notification.CustomerName} registered with email {notification.CustomerEmail}", cancellationToken);
    }

    public async Task OnNotification(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("INVENTORY_UPDATED", $"Product {notification.ProductId} inventory: {notification.OldQuantity} ? {notification.NewQuantity}", cancellationToken);
    }

    private async Task LogAuditEvent(string eventType, string details, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken); // Simulate audit logging

        logger.LogInformation("* AUDIT LOG: {EventType} - {Details} at {Timestamp:yyyy-MM-dd HH:mm:ss.fff}",
            eventType, details, DateTime.UtcNow);
    }
}