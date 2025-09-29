namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Marker interface for order-related notifications.
/// This enables type-constrained middleware for order processing.
/// </summary>
public interface IOrderNotification : INotification
{
    int OrderId { get; }
    string CustomerEmail { get; }
}