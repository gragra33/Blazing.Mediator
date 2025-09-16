namespace TypedSimpleNotificationExample.Notifications;

/// <summary>
/// Marker interface for order-related notifications.
/// This enables type-constrained middleware for order processing.
/// </summary>
public interface IOrderNotification : INotification
{
    int OrderId { get; }
    string CustomerEmail { get; }
}

/// <summary>
/// Marker interface for customer-related notifications.
/// This enables type-constrained middleware for customer processing.
/// </summary>
public interface ICustomerNotification : INotification
{
    string CustomerEmail { get; }
    string CustomerName { get; }
}

/// <summary>
/// Marker interface for inventory-related notifications.
/// This enables type-constrained middleware for inventory processing.
/// </summary>
public interface IInventoryNotification : INotification
{
    string ProductId { get; }
    int Quantity { get; }
}