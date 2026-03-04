namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestNamedQuery.
/// </summary>
public class TestRequestNamedQueryHandler : IRequestHandler<TestRequestNamedQuery, string>
{
    public ValueTask<string> Handle(TestRequestNamedQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Query: {request.Value}");
    }
}