using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with default instance Order property.
/// </summary>
public class NotificationMiddlewareWithDefaultInstanceOrder : INotificationMiddleware
{
    public int Order => 0; // Default value

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}