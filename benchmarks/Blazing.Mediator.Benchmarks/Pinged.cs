namespace Blazing.Mediator.Benchmarks;

public class Pinged : INotification
{
}

public class PingedHandler : INotificationHandler<Pinged>
{
    public Task Handle(Pinged notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class PingedSubscriber : INotificationSubscriber<Pinged>
{
    public Task OnNotification(Pinged notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}