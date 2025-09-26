namespace TypedNotificationSubscriberExample.Notifications;

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