namespace Blazing.Mediator;

/// <summary>
/// Handler interface for processing notifications. 
/// This is an internal interface used by the subscription system.
/// Subscribers should use INotificationSubscriber directly.
/// </summary>
/// <typeparam name="TNotification">The type of notification to handle</typeparam>
internal interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}
