using Blazing.Mediator.Abstractions.Middleware;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with OrderAttribute.
/// </summary>
[Order(15)]
public class NotificationMiddlewareWithOrderAttribute : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}