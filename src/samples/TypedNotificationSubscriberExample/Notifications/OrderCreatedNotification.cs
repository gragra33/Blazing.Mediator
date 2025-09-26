namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Notification published when an order is created.
/// Implements IOrderNotification for type-constrained middleware.
/// </summary>
public class OrderCreatedNotification(
    int orderId,
    string customerEmail,
    string customerName,
    decimal totalAmount,
    List<OrderItem> items,
    DateTime createdAt)
    : IOrderNotification
{
    public int OrderId { get; } = orderId;
    public string CustomerEmail { get; } = customerEmail;
    public string CustomerName { get; } = customerName;
    public decimal TotalAmount { get; } = totalAmount;
    public List<OrderItem> Items { get; } = items;
    public DateTime CreatedAt { get; } = createdAt;
}