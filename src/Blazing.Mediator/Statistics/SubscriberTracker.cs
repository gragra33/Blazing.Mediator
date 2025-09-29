using System.Diagnostics;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Thread-safe tracker for notification subscriber registrations and analytics.
/// Uses weak references to prevent memory leaks from tracking subscriber instances.
/// </summary>
public sealed class SubscriberTracker : ISubscriberTracker, IDisposable
{
    private readonly ConcurrentDictionary<Type, ConcurrentBag<SubscriberInfo>> _subscriptions = new();
    private readonly Timer? _cleanupTimer;
    private readonly Lock _cleanupLock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SubscriberTracker with automatic cleanup.
    /// </summary>
    /// <param name="cleanupIntervalMinutes">Interval in minutes for cleaning up expired weak references. Default is 5 minutes.</param>
    public SubscriberTracker(int cleanupIntervalMinutes = 5)
    {
        // Set up periodic cleanup of expired weak references
        var cleanupInterval = TimeSpan.FromMinutes(cleanupIntervalMinutes);
        _cleanupTimer = new Timer(PerformCleanup, null, cleanupInterval, cleanupInterval);
    }

    /// <summary>
    /// Tracks a subscription event for analytics and statistics.
    /// </summary>
    /// <param name="notificationType">The notification type being subscribed to.</param>
    /// <param name="subscriberType">The type of the subscriber.</param>
    /// <param name="subscriber">The subscriber instance.</param>
    public void TrackSubscription(Type notificationType, Type subscriberType, object subscriber)
    {
        if (_disposed)
            return;

        try
        {
            var subscriberInfo = new SubscriberInfo(
                notificationType,
                subscriberType,
                new WeakReference(subscriber),
                DateTime.UtcNow,
                IsGenericSubscriber(subscriberType)
            );

            _subscriptions.AddOrUpdate(
                notificationType,
                [subscriberInfo],
                (_, existing) =>
                {
                    existing.Add(subscriberInfo);
                    return existing;
                }
            );
        }
        catch (Exception ex)
        {
            // Log the exception but don't throw to avoid breaking subscription
            Debug.WriteLine($"Error tracking subscription for {notificationType.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Tracks an unsubscription event for analytics and statistics.
    /// </summary>
    /// <param name="notificationType">The notification type being unsubscribed from.</param>
    /// <param name="subscriberType">The type of the subscriber.</param>
    /// <param name="subscriber">The subscriber instance.</param>
    public void TrackUnsubscription(Type notificationType, Type subscriberType, object subscriber)
    {
        if (_disposed)
            return;

        try
        {
            if (!_subscriptions.TryGetValue(notificationType, out var subscribers))
                return;

            // Create a new bag without the unsubscribed subscriber
            var newSubscribers = new ConcurrentBag<SubscriberInfo>();
            
            foreach (var info in subscribers)
            {
                // Keep if it's not the subscriber being removed and the weak reference is still alive
                var target = info.Subscriber.Target;
                if (target != null && !ReferenceEquals(target, subscriber))
                {
                    newSubscribers.Add(info);
                }
            }

            // Update or remove the entry
            if (newSubscribers.IsEmpty)
            {
                _subscriptions.TryRemove(notificationType, out _);
            }
            else
            {
                _subscriptions.TryUpdate(notificationType, newSubscribers, subscribers);
            }
        }
        catch (Exception ex)
        {
            // Log the exception but don't throw to avoid breaking unsubscription
            //Debug.WriteLine($"Error tracking unsubscription for {notificationType.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all active subscribers for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to get subscribers for.</param>
    /// <returns>Read-only list of active subscriber information.</returns>
    public IReadOnlyList<SubscriberInfo> GetActiveSubscribers(Type notificationType)
    {
        if (_disposed || !_subscriptions.TryGetValue(notificationType, out var subscribers))
            return [];

        try
        {
            // Filter out expired weak references
            return subscribers
                .Where(info => info.Subscriber.Target != null)
                .ToArray();
        }
        catch (Exception ex)
        {
            //Debug.WriteLine($"Error getting active subscribers for {notificationType.Name}: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Gets all active subscriptions across all notification types.
    /// </summary>
    /// <returns>Dictionary mapping notification types to their active subscribers.</returns>
    public IReadOnlyDictionary<Type, IReadOnlyList<SubscriberInfo>> GetAllSubscriptions()
    {
        if (_disposed)
            return new Dictionary<Type, IReadOnlyList<SubscriberInfo>>();

        try
        {
            var result = new Dictionary<Type, IReadOnlyList<SubscriberInfo>>();

            foreach (var kvp in _subscriptions)
            {
                var activeSubscribers = kvp.Value
                    .Where(info => info.Subscriber.Target != null)
                    .ToArray();

                if (activeSubscribers.Length > 0)
                {
                    result[kvp.Key] = activeSubscribers;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            //Debug.WriteLine($"Error getting all subscriptions: {ex.Message}");
            return new Dictionary<Type, IReadOnlyList<SubscriberInfo>>();
        }
    }

    /// <summary>
    /// Cleans up expired weak references and performs maintenance.
    /// </summary>
    public void Cleanup()
    {
        if (_disposed)
            return;

        lock (_cleanupLock)
        {
            if (_disposed)
                return;

            try
            {
                var typesToRemove = new List<Type>();

                foreach (var kvp in _subscriptions)
                {
                    // Create new bag with only live subscribers
                    var liveSubscribers = new ConcurrentBag<SubscriberInfo>();
                    
                    foreach (var info in kvp.Value)
                    {
                        if (info.Subscriber.Target != null)
                        {
                            liveSubscribers.Add(info);
                        }
                    }

                    if (liveSubscribers.IsEmpty)
                    {
                        typesToRemove.Add(kvp.Key);
                    }
                    else if (liveSubscribers.Count != kvp.Value.Count)
                    {
                        _subscriptions.TryUpdate(kvp.Key, liveSubscribers, kvp.Value);
                    }
                }

                // Remove empty entries
                foreach (var type in typesToRemove)
                {
                    _subscriptions.TryRemove(type, out _);
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"Error during subscriber cleanup: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Determines if a subscriber type is a generic subscriber (handles all notifications).
    /// </summary>
    /// <param name="subscriberType">The subscriber type to check.</param>
    /// <returns>True if the subscriber is generic, false if specific to a notification type.</returns>
    private static bool IsGenericSubscriber(Type subscriberType)
    {
        // Check if the type implements INotificationSubscriber (generic) vs INotificationSubscriber<T> (specific)
        var interfaces = subscriberType.GetInterfaces();
        
        // Generic subscriber implements INotificationSubscriber directly
        if (interfaces.Contains(typeof(INotificationSubscriber)))
        {
            return true;
        }

        // Specific subscriber implements INotificationSubscriber<T>
        return interfaces.Any(i => 
            i.IsGenericType && 
            i.GetGenericTypeDefinition() == typeof(INotificationSubscriber<>));
    }

    /// <summary>
    /// Performs cleanup of expired weak references.
    /// </summary>
    /// <param name="state">Timer callback state (unused).</param>
    private void PerformCleanup(object? state)
    {
        Cleanup();
    }

    /// <summary>
    /// Disposes resources used by the SubscriberTracker.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        lock (_cleanupLock)
        {
            if (_disposed)
                return;

            _disposed = true;
            _cleanupTimer?.Dispose();
            _subscriptions.Clear();
        }
    }
}