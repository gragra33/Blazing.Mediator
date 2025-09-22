namespace Blazing.Mediator.Tests.Statistics;

public class StatTestQueryHandler : IRequestHandler<StatTestQuery, string>
{
    public Task<string> Handle(StatTestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}