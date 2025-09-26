namespace TypedNotificationHandlerExample.Notifications;

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

/// <summary>
/// Notification published when a customer registers.
/// Implements ICustomerNotification for type-constrained middleware.
/// </summary>
public class CustomerRegisteredNotification(
    string customerEmail,
    string customerName,
    DateTime registeredAt)
    : ICustomerNotification
{
    public string CustomerEmail { get; } = customerEmail;
    public string CustomerName { get; } = customerName;
    public DateTime RegisteredAt { get; } = registeredAt;
}

/// <summary>
/// Notification published when inventory levels change.
/// Implements IInventoryNotification for type-constrained middleware.
/// </summary>
public class InventoryUpdatedNotification(
    string productId,
    string productName,
    int oldQuantity,
    int newQuantity,
    int changeAmount,
    DateTime updatedAt)
    : IInventoryNotification
{
    public string ProductId { get; } = productId;
    public string ProductName { get; } = productName;
    public int OldQuantity { get; } = oldQuantity;
    public int NewQuantity { get; } = newQuantity;
    public int Quantity { get; } = changeAmount; // For IInventoryNotification
    public int ChangeAmount { get; } = changeAmount;
    public DateTime UpdatedAt { get; } = updatedAt;
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