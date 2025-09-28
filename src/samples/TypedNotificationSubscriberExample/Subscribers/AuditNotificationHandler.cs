namespace TypedNotificationSubscriberExample.Subscribers;

/// <summary>
/// Audit notification subscriber that maintains comprehensive audit trails.
/// This subscriber demonstrates manual subscription and logging of all notification activities
/// for compliance and monitoring purposes using the subscriber pattern.
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
        await LogAuditEvent("ORDER_STATUS_CHANGED", $"Order {notification.OrderId} status: {notification.OldStatus} -> {notification.NewStatus}", cancellationToken);
    }

    public async Task OnNotification(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("CUSTOMER_REGISTERED", $"Customer {notification.CustomerName} registered with email {notification.CustomerEmail}", cancellationToken);
    }

    public async Task OnNotification(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        await LogAuditEvent("INVENTORY_UPDATED", $"Product {notification.ProductId} inventory: {notification.OldQuantity} -> {notification.NewQuantity}", cancellationToken);
    }

    private async Task LogAuditEvent(string eventType, string details, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken); // Simulate audit logging

        logger.LogInformation(">> AUDIT LOG: {EventType} - {Details} at {Timestamp:yyyy-MM-dd HH:mm:ss.fff}",
            eventType, details, DateTime.UtcNow);
    }
}