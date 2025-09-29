namespace Blazing.Mediator;

/// <summary>
/// Central mediator for handling requests and implementing CQRS pattern
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a command that doesn't return a value
    /// </summary>
    /// <param name="request">The command to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task Send(IRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a query that returns a value
    /// </summary>
    /// <typeparam name="TResponse">The type of response expected</typeparam>
    /// <param name="request">The query to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task containing the response</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a stream request that returns an async enumerable
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable of response items</returns>
    IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a notification to all subscribers following the observer pattern.
    /// Publishers blindly send notifications without caring about recipients.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to publish</typeparam>
    /// <param name="notification">The notification to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;

    /// <summary>
    /// Subscribe to notifications of a specific type.
    /// Subscribers actively choose to listen to notifications they're interested in.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to subscribe to</typeparam>
    /// <param name="subscriber">The subscriber that will receive notifications</param>
    void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification;

    /// <summary>
    /// Subscribe to all notifications (generic/broadcast).
    /// Subscribers actively choose to listen to all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber that will receive all notifications</param>
    void Subscribe(INotificationSubscriber subscriber);

    /// <summary>
    /// Unsubscribe from notifications of a specific type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to unsubscribe from</typeparam>
    /// <param name="subscriber">The subscriber to remove</param>
    void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification;

    /// <summary>
    /// Unsubscribe from all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove from all notifications</param>
    void Unsubscribe(INotificationSubscriber subscriber);
}
