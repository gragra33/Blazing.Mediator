namespace Blazing.Mediator.Tests.Statistics;

public class StatTestCommandHandler : IRequestHandler<StatTestCommand>
{
    public ValueTask Handle(StatTestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}