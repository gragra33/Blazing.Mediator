namespace Blazing.Mediator.Benchmarks;

public class Pinged : INotification
{
}

public class PingedHandler : INotificationHandler<Pinged>
{
    public ValueTask Handle(Pinged notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

public class PingedSubscriber : INotificationSubscriber<Pinged>
{
    public Task OnNotification(Pinged notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}