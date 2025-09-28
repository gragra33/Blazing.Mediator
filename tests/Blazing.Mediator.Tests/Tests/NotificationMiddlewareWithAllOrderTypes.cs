using Blazing.Mediator.Abstractions.Middleware;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with all order types to test precedence.
/// </summary>
[Order(25)]
public class NotificationMiddlewareWithAllOrderTypes : INotificationMiddleware
{
    public static int Order => 100; // Should win over field and attribute
    public static int Order2 = 50; // Field 
    public int InstanceOrder => 75; // Instance property

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}