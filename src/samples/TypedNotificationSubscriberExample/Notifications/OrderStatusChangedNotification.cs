namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Notification published when an order status changes.
/// Implements IOrderNotification for type-constrained middleware.
/// </summary>
public class OrderStatusChangedNotification(
    int orderId,
    string customerEmail,
    string oldStatus,
    string newStatus,
    DateTime changedAt)
    : IOrderNotification
{
    public int OrderId { get; } = orderId;
    public string CustomerEmail { get; } = customerEmail;
    public string OldStatus { get; } = oldStatus;
    public string NewStatus { get; } = newStatus;
    public DateTime ChangedAt { get; } = changedAt;
}