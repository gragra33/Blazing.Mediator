namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with all order mechanisms to test precedence.
/// [Order(n)] attribute wins at both runtime and source-generator compile time.
/// </summary>
[Order(25)]
public class NotificationMiddlewareWithAllOrderTypes : INotificationMiddleware
{
    public static int Order => 100; // Superseded by [Order(25)] attribute above
    public static int Order2 = 50; // Field — lowest static priority
    public int InstanceOrder => 75; // Instance property — not named 'Order', not checked

    public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}