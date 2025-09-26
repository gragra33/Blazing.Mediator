namespace TypedNotificationSubscriberExample.Notifications;

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