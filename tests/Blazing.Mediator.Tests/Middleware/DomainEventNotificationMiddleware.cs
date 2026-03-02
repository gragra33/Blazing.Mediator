namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test notification middleware with interface constraint.
/// </summary>
public class DomainEventNotificationMiddleware : INotificationMiddleware
{
    public int Order => 700;

    public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}