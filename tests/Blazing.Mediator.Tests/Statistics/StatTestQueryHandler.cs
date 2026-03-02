namespace Blazing.Mediator.Tests.Statistics;

public class StatTestQueryHandler : IRequestHandler<StatTestQuery, string>
{
    public async ValueTask<string> Handle(StatTestQuery request, CancellationToken cancellationToken)
    {
        return $"Handled: {request.Value}";
    }
}