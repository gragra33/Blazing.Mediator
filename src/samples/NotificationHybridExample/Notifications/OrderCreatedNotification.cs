namespace NotificationHybridExample.Notifications;

/// <summary>
/// Notification published when an order is successfully created.
/// This notification will be handled by both automatic handlers and manual subscribers to demonstrate hybrid approach.
/// </summary>
public record OrderCreatedNotification : INotification
{
    public required string OrderId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal TotalAmount { get; init; }
    public required List<OrderItem> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Represents an item in an order
/// </summary>
public record OrderItem
{
    public required string ProductName { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal TotalPrice => Quantity * UnitPrice;
}