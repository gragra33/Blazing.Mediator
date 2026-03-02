namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test query handler.
/// </summary>
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public async ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return $"Handled: {request.Value}";
    }
}