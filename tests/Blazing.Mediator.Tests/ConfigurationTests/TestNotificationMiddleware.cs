using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.ConfigurationTests;

public class TestNotificationMiddleware : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        await next(notification, cancellationToken);
    }
}