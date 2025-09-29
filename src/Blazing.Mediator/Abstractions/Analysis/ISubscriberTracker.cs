namespace Blazing.Mediator;

/// <summary>
/// Information about a notification subscriber registration.
/// </summary>
/// <param name="NotificationType">The type of notification being subscribed to.</param>
/// <param name="SubscriberType">The type of the subscriber.</param>
/// <param name="Subscriber">Weak reference to the subscriber instance.</param>
/// <param name="SubscriptionTime">When the subscription was registered.</param>
/// <param name="IsGeneric">Whether this is a generic subscriber that handles all notifications.</param>
public sealed record SubscriberInfo(
    Type NotificationType,
    Type SubscriberType,
    WeakReference Subscriber,
    DateTime SubscriptionTime,
    bool IsGeneric
);

/// <summary>
/// Provides subscriber tracking services for the manual subscriber system.
/// </summary>
public interface ISubscriberTracker
{
    /// <summary>
    /// Tracks a subscription event for analytics and statistics.
    /// </summary>
    /// <param name="notificationType">The notification type being subscribed to.</param>
    /// <param name="subscriberType">The type of the subscriber.</param>
    /// <param name="subscriber">The subscriber instance.</param>
    void TrackSubscription(Type notificationType, Type subscriberType, object subscriber);

    /// <summary>
    /// Tracks an unsubscription event for analytics and statistics.
    /// </summary>
    /// <param name="notificationType">The notification type being unsubscribed from.</param>
    /// <param name="subscriberType">The type of the subscriber.</param>
    /// <param name="subscriber">The subscriber instance.</param>
    void TrackUnsubscription(Type notificationType, Type subscriberType, object subscriber);

    /// <summary>
    /// Gets all active subscribers for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to get subscribers for.</param>
    /// <returns>Read-only list of active subscriber information.</returns>
    IReadOnlyList<SubscriberInfo> GetActiveSubscribers(Type notificationType);

    /// <summary>
    /// Gets all active subscriptions across all notification types.
    /// </summary>
    /// <returns>Dictionary mapping notification types to their active subscribers.</returns>
    IReadOnlyDictionary<Type, IReadOnlyList<SubscriberInfo>> GetAllSubscriptions();

    /// <summary>
    /// Cleans up expired weak references and performs maintenance.
    /// </summary>
    void Cleanup();
}