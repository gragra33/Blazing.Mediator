namespace Blazing.Mediator;

/// <summary>
/// Pre-resolved handler array for a specific notification type.
/// Passed to <see cref="INotificationPublisher"/> so the publisher can iterate over
/// a cached, strongly-typed array without additional DI resolution.
/// </summary>
/// <typeparam name="TNotification">The notification type.</typeparam>
public readonly struct NotificationHandlers<TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// The resolved handlers (never <see langword="null"/>; may be empty).
    /// </summary>
    public readonly INotificationHandler<TNotification>[] Handlers;

    /// <summary>
    /// Initialises a new <see cref="NotificationHandlers{TNotification}"/> with the supplied array.
    /// </summary>
    /// <param name="handlers">The resolved handler instances. Must not be <see langword="null"/>.</param>
    public NotificationHandlers(INotificationHandler<TNotification>[] handlers)
        => Handlers = handlers ?? [];
}

/// <summary>
/// Pluggable strategy that controls how notification handlers are invoked.
/// Two built-in implementations are provided:
/// <list type="bullet">
///   <item><see cref="Blazing.Mediator.Notifications.SequentialNotificationPublisher"/> — default, foreach await with unrolled fast paths</item>
///   <item><see cref="Blazing.Mediator.Notifications.ConcurrentNotificationPublisher"/> — parallel dispatch, all handlers start before first await</item>
/// </list>
/// Configure via <see cref="Configuration.MediatorOptions.NotificationPublisher"/>.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Invokes all handlers in <paramref name="handlers"/> for the given <paramref name="notification"/>.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="handlers">Pre-resolved handler array (cached at startup for Singleton lifetime).</param>
    /// <param name="notification">The notification instance to deliver.</param>
    /// <param name="cancellationToken">Token that can cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have been invoked.</returns>
    ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification;
}
