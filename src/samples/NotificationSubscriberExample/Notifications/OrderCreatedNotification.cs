namespace NotificationSubscriberExample.Notifications;

/// <summary>
/// Notification published when an order is created.
/// This demonstrates a real-world scenario where multiple subscribers
/// need to react to a single business event.
/// </summary>
public class OrderCreatedNotification(
    int orderId,
    string customerEmail,
    string customerName,
    decimal totalAmount,
    List<OrderItem> items,
    DateTime createdAt)
    : INotification
{
    public int OrderId { get; } = orderId;
    public string CustomerEmail { get; } = customerEmail;
    public string CustomerName { get; } = customerName;
    public decimal TotalAmount { get; } = totalAmount;
    public List<OrderItem> Items { get; } = items;
    public DateTime CreatedAt { get; } = createdAt;
}

/// <summary>
/// Represents an item in an order.
/// </summary>
public class OrderItem(int productId, string productName, int quantity, decimal unitPrice)
{
    public int ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public int Quantity { get; } = quantity;
    public decimal UnitPrice { get; } = unitPrice;
}
