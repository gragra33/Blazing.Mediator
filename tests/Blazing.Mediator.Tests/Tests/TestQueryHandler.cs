namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test query handler.
/// </summary>
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}