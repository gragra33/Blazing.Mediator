namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestNamedQuery.
/// </summary>
public class TestRequestNamedQueryHandler : IRequestHandler<TestRequestNamedQuery, string>
{
    public async ValueTask<string> Handle(TestRequestNamedQuery request, CancellationToken cancellationToken)
    {
        return $"Query: {request.Value}";
    }
}