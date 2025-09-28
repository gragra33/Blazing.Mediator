namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test notification middleware for pipeline execution tests.
/// </summary>
public class TestNotificationMiddleware2 : INotificationMiddleware
{
    public int Order => 2;

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        // Add to execution order tracking if it exists
        if (notification is TestNotification testNotification)
        {
            TestNotificationExecutionTracker.ExecutionOrder.Add("Middleware2");
        }
        await next(notification, cancellationToken);
    }
}