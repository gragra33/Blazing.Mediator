namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Marker interface for inventory-related notifications.
/// This enables type-constrained middleware for inventory processing.
/// </summary>
public interface IInventoryNotification : INotification
{
    string ProductId { get; }
    int Quantity { get; }
}