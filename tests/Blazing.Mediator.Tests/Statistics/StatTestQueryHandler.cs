namespace Blazing.Mediator.Tests.Statistics;

public class StatTestQueryHandler : IRequestHandler<StatTestQuery, string>
{
    public ValueTask<string> Handle(StatTestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Handled: {request.Value}");
    }
}