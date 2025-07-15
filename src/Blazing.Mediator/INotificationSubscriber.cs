namespace Blazing.Mediator;

/// <summary>
/// Interface for objects that want to receive specific types of notifications.
/// Subscribers actively choose to subscribe/unsubscribe to notifications they're interested in.
/// </summary>
/// <typeparam name="TNotification">The type of notification to subscribe to</typeparam>
public interface INotificationSubscriber<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Called when a notification of the subscribed type is published.
    /// </summary>
    /// <param name="notification">The notification that was published</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task OnNotification(TNotification notification, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for objects that want to receive all notifications (generic/broadcast).
/// Subscribers actively choose to subscribe/unsubscribe to all notifications.
/// </summary>
public interface INotificationSubscriber
{
    /// <summary>
    /// Called when any notification is published.
    /// </summary>
    /// <param name="notification">The notification that was published</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task OnNotification(INotification notification, CancellationToken cancellationToken = default);
}
