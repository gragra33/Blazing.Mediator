namespace TypedNotificationHybridExample.Notifications;

// Base notification interfaces for type constraints
public interface IOrderNotification : INotification { }
public interface ICustomerNotification : INotification { }
public interface IInventoryNotification : INotification { }

/// <summary>
/// Notification published when an order is successfully created.
/// Implements IOrderNotification for type-constrained middleware targeting.
/// </summary>
public record OrderCreatedNotification : IOrderNotification
{
    public required string OrderId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required decimal TotalAmount { get; init; }
    public required List<OrderItem> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Notification published when a customer registers in the system.
/// Implements ICustomerNotification for type-constrained middleware targeting.
/// </summary>
public record CustomerRegisteredNotification : ICustomerNotification
{
    public required string CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string RegistrationSource { get; init; }
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Notification published when inventory levels are updated.
/// Implements IInventoryNotification for type-constrained middleware targeting.
/// </summary>
public record InventoryUpdatedNotification : IInventoryNotification
{
    public required string ProductId { get; init; }
    public required string ProductName { get; init; }
    public required int PreviousQuantity { get; init; }
    public required int NewQuantity { get; init; }
    public required string UpdateReason { get; init; }
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
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