using System.Diagnostics;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Detects notification patterns by analyzing registered handlers and active subscribers.
/// </summary>
public sealed class NotificationPatternDetector : INotificationPatternDetector
{
    private readonly ISubscriberTracker? _subscriberTracker;

    /// <summary>
    /// Initializes a new instance of the NotificationPatternDetector.
    /// </summary>
    /// <param name="subscriberTracker">Optional subscriber tracker for detecting active subscribers.</param>
    public NotificationPatternDetector(ISubscriberTracker? subscriberTracker = null)
    {
        _subscriberTracker = subscriberTracker;
    }

    /// <summary>
    /// Detects the notification pattern for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to analyze.</param>
    /// <param name="serviceProvider">Service provider to check for registered handlers.</param>
    /// <returns>The detected notification pattern.</returns>
    public NotificationPattern DetectPattern(Type notificationType, IServiceProvider serviceProvider)
    {
        var hasHandlers = HasRegisteredHandlers(notificationType, serviceProvider);
        var hasSubscribers = HasActiveSubscribers(notificationType);

        return (hasHandlers, hasSubscribers) switch
        {
            (true, true) => NotificationPattern.Hybrid,
            (true, false) => NotificationPattern.AutomaticHandlers,
            (false, true) => NotificationPattern.ManualSubscribers,
            (false, false) => NotificationPattern.None
        };
    }

    /// <summary>
    /// Checks if there are registered automatic handlers for a notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <param name="serviceProvider">Service provider to check for registered handlers.</param>
    /// <returns>True if handlers are registered, false otherwise.</returns>
    public bool HasRegisteredHandlers(Type notificationType, IServiceProvider serviceProvider)
    {
        try
        {
            // Look for INotificationHandler<T> implementations
            var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            
            // Try to get all registered services for this handler interface
            var handlerServices = serviceProvider.GetServices(handlerInterfaceType);
            return handlerServices.Any(h => h != null);
        }
        catch (Exception ex)
        {
            // For debugging: log the exception details
            Debug.WriteLine($"Error checking handlers for notification {notificationType.Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks if there are active manual subscribers for a notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <returns>True if active subscribers exist, false otherwise.</returns>
    public bool HasActiveSubscribers(Type notificationType)
    {
        if (_subscriberTracker == null)
        {
            return false;
        }

        try
        {
            var subscribers = _subscriberTracker.GetActiveSubscribers(notificationType);
            return subscribers.Count > 0;
        }
        catch (Exception ex)
        {
            // For debugging: log the exception details
            Debug.WriteLine($"Error checking subscribers for notification {notificationType.Name}: {ex.Message}");
            return false;
        }
    }
}