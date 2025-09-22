using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with static Order field.
/// </summary>
public class NotificationMiddlewareWithStaticOrderField : INotificationMiddleware
{
    public static int Order = 5;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}