namespace Blazing.Mediator.Tests;

/// <summary>
/// Conditional notification middleware for testing conditional execution.
/// </summary>
public class ConditionalNotificationMiddleware : IConditionalNotificationMiddleware
{
    public static TestNotification? LastExecuted { get; set; }

    public int Order => 5;

    public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
    {
        if (notification is TestNotification testNotification)
        {
            return testNotification.Message?.Contains("shouldexecute") == true;
        }
        return false;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        if (notification is TestNotification testNotification)
        {
            LastExecuted = testNotification;
        }
        await next(notification, cancellationToken);
    }
}