namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with no parameterless constructor.
/// Excluded from auto-discovery so the source generator does not bake it into notification wrappers
/// (its constructor requires a <c>string</c> parameter that DI cannot resolve).
/// </summary>
[ExcludeFromAutoDiscovery]
public class NotificationMiddlewareWithNoParameterlessConstructor : INotificationMiddleware
{
    public NotificationMiddlewareWithNoParameterlessConstructor(string required)
    {
        // No parameterless constructor
    }

    public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}