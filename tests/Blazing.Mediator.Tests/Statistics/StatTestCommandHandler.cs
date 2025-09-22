namespace Blazing.Mediator.Tests.Statistics;

public class StatTestCommandHandler : IRequestHandler<StatTestCommand>
{
    public Task Handle(StatTestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}