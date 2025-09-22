using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests.TestMiddleware;

/// <summary>
/// Test notification middleware with constraint for testing notification constraint analysis.
/// </summary>
/// <typeparam name="TNotification">Notification type that must be a reference type.</typeparam>
public class NotificationConstraintMiddleware<TNotification> : INotificationMiddleware
    where TNotification : class, INotification
{
    public int Order => 600;

    public async Task InvokeAsync<TNotificationInner>(TNotificationInner notification, NotificationDelegate<TNotificationInner> next, CancellationToken cancellationToken)
        where TNotificationInner : INotification
    {
        await next(notification, cancellationToken);
    }
}