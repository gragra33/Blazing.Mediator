using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with instance Order property.
/// </summary>
public class NotificationMiddlewareWithInstanceOrder : INotificationMiddleware
{
    public int Order => 20;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}