namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasequery.
/// </summary>
public class TestRequestLowercasequeryHandler : IRequestHandler<TestRequestLowercasequery, string>
{
    public Task<string> Handle(TestRequestLowercasequery request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"lowercase: {request.Value}");
    }
}