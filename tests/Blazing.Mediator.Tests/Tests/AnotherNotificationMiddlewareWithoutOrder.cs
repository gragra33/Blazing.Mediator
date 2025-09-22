using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Another notification middleware without Order property for testing multiple middleware.
/// </summary>
public class AnotherNotificationMiddlewareWithoutOrder : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}