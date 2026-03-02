namespace Blazing.Mediator.Tests;

/// <summary>
/// Notification middleware with no parameterless constructor.
/// Excluded from source-generator auto-discovery so it does not break global ContainerMetadata init.
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