using Blazing.Mediator.Abstractions;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Test notification middleware for pipeline execution tests.
/// </summary>
public class TestNotificationMiddleware1 : INotificationMiddleware
{
    public int Order => 1;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Add to execution order tracking if it exists
        if (notification is TestNotification testNotification)
        {
            TestNotificationExecutionTracker.ExecutionOrder.Add("Middleware1");
        }
        await next(notification, cancellationToken);
    }
}