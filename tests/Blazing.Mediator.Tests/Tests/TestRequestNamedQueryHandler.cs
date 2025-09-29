namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestNamedQuery.
/// </summary>
public class TestRequestNamedQueryHandler : IRequestHandler<TestRequestNamedQuery, string>
{
    public Task<string> Handle(TestRequestNamedQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Query: {request.Value}");
    }
}