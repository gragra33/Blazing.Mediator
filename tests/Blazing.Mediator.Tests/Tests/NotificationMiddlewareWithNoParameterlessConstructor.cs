namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with no parameterless constructor.
/// </summary>
public class NotificationMiddlewareWithNoParameterlessConstructor : INotificationMiddleware
{
    public NotificationMiddlewareWithNoParameterlessConstructor(string required)
    {
        // No parameterless constructor
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}