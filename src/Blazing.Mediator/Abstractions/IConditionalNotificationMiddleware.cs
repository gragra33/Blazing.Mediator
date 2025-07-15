namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Conditional notification middleware that can determine whether it should execute
/// based on the notification type or content.
/// </summary>
public interface IConditionalNotificationMiddleware : INotificationMiddleware
{
    /// <summary>
    /// Determines whether this middleware should execute for the specified notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification</typeparam>
    /// <param name="notification">The notification being processed</param>
    /// <returns>True if the middleware should execute; otherwise, false</returns>
    bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification;
}
