namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasequery.
/// </summary>
public class TestRequestLowercasequeryHandler : IRequestHandler<TestRequestLowercasequery, string>
{
    public ValueTask<string> Handle(TestRequestLowercasequery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"lowercase: {request.Value}");
    }
}