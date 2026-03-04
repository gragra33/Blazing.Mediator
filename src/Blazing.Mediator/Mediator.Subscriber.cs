namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Subscribe to notifications of a specific type.
    /// Subscribers actively choose to listen to notifications they're interested in.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to subscribe to</typeparam>
    /// <param name="subscriber">The subscriber that will receive notifications</param>
    public void Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        _specificSubscribers.AddOrUpdate(
            typeof(TNotification),
            [subscriber],
            (_, existing) =>
            {
                existing.Add(subscriber);
                return existing;
            });

        // Track subscription for enhanced statistics — no reflection; TNotification from compile-time type param
        _subscriberTracker?.TrackSubscription(subscriber);
    }

    /// <summary>
    /// Subscribe to all notifications (generic/broadcast).
    /// Subscribers actively choose to listen to all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber that will receive all notifications</param>
    public void Subscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);
        _genericSubscribers.Add(subscriber);

        // Track generic subscription for enhanced statistics
        // Use a marker type for generic subscribers
        _subscriberTracker?.TrackSubscription(typeof(INotification), subscriber.GetType(), subscriber);
    }

    /// <summary>
    /// Unsubscribe from notifications of a specific type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to unsubscribe from</typeparam>
    /// <param name="subscriber">The subscriber to remove</param>
    public void Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber) where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        if (!_specificSubscribers.TryGetValue(typeof(TNotification), out var subscribers))
        {
            return;
        }

        // Create new bag without the subscriber
        var newSubscribers = new ConcurrentBag<object>();
        foreach (var existing in from existing in subscribers
                                 where !ReferenceEquals(existing, subscriber)
                                 select existing)
        {
            newSubscribers.Add(existing);
        }

        if (newSubscribers.IsEmpty)
        {
            _specificSubscribers.TryRemove(typeof(TNotification), out _);
        }
        else
        {
            _specificSubscribers.TryUpdate(typeof(TNotification), newSubscribers, subscribers);
        }

        // Track unsubscription for enhanced statistics — no reflection; TNotification from compile-time type param
        _subscriberTracker?.TrackUnsubscription(subscriber);
    }

    /// <summary>
    /// Unsubscribe from all notifications.
    /// </summary>
    /// <param name="subscriber">The subscriber to remove from all notifications</param>
    public void Unsubscribe(INotificationSubscriber subscriber)
    {
        ArgumentNullException.ThrowIfNull(subscriber);

        // Remove from generic subscribers
        var newGenericSubscribers = new ConcurrentBag<INotificationSubscriber>();
        foreach (var existing in _genericSubscribers
                     .Where(existing => !ReferenceEquals(existing, subscriber)))
        {
            newGenericSubscribers.Add(existing);
        }

        // Replace the entire bag
        _genericSubscribers.Clear();
        foreach (var sub in newGenericSubscribers)
        {
            _genericSubscribers.Add(sub);
        }

        // Track generic unsubscription for enhanced statistics
        // Use a marker type for generic subscribers
        _subscriberTracker?.TrackUnsubscription(typeof(INotification), subscriber.GetType(), subscriber);
    }

    #region Internal Methods for Generated Code

    /// <summary>
    /// Internal method for generated notification dispatcher.
    /// Returns task invocations for all manually subscribed handlers.
    /// This method is called by generated code and should not be used directly.
    /// Use <see cref="Subscribe{TNotification}"/> and <see cref="Unsubscribe{TNotification}"/> instead.
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <param name="notification">The notification instance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerable of subscriber task invocations</returns>
    /// <remarks>
    /// This method is marked internal and called by the generated NotificationDispatcher
    /// to integrate manual subscribers with generated handler dispatch.
    /// Thread-safe enumeration is achieved using ToArray() snapshot.
    /// </remarks>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal IEnumerable<Task> GetSubscriberInvocations<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationType = typeof(TNotification);

        // Process specific subscribers
        if (_specificSubscribers.TryGetValue(notificationType, out var specificBag))
        {
            // ToArray() creates snapshot for thread-safe enumeration
            foreach (var subscriber in specificBag.ToArray())
            {
                if (subscriber is INotificationSubscriber<TNotification> typedSubscriber)
                {
                    yield return typedSubscriber.OnNotification(notification, cancellationToken);
                }
            }
        }

        // Process generic subscribers (can handle any notification)
        // ToArray() creates snapshot for thread-safe enumeration
        foreach (var genericSubscriber in _genericSubscribers.ToArray())
        {
            yield return genericSubscriber.OnNotification(notification, cancellationToken);
        }
    }

    #endregion
}