namespace Blazing.Mediator;

/// <summary>
/// Provides notification pattern detection services to identify which notification system is in use.
/// </summary>
public interface INotificationPatternDetector
{
    /// <summary>
    /// Detects the notification pattern for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to analyze.</param>
    /// <param name="serviceProvider">Service provider to check for registered handlers.</param>
    /// <returns>The detected notification pattern.</returns>
    NotificationPattern DetectPattern(Type notificationType, IServiceProvider serviceProvider);

    /// <summary>
    /// Checks if there are registered automatic handlers for a notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <param name="serviceProvider">Service provider to check for registered handlers.</param>
    /// <returns>True if handlers are registered, false otherwise.</returns>
    bool HasRegisteredHandlers(Type notificationType, IServiceProvider serviceProvider);

    /// <summary>
    /// Checks if there are active manual subscribers for a notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <returns>True if active subscribers exist, false otherwise.</returns>
    bool HasActiveSubscribers(Type notificationType);
}