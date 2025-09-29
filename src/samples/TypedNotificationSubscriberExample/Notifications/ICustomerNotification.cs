namespace TypedNotificationSubscriberExample.Notifications;

/// <summary>
/// Marker interface for customer-related notifications.
/// This enables type-constrained middleware for customer processing.
/// </summary>
public interface ICustomerNotification : INotification
{
    string CustomerEmail { get; }
    string CustomerName { get; }
}