namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with static Order property.
/// </summary>
public class NotificationMiddlewareWithStaticOrder : INotificationMiddleware
{
    public static int Order => 10;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}