namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestRequestLowercasequery.
/// </summary>
public class TestRequestLowercasequeryHandler : IRequestHandler<TestRequestLowercasequery, string>
{
    public async ValueTask<string> Handle(TestRequestLowercasequery request, CancellationToken cancellationToken)
    {
        return $"lowercase: {request.Value}";
    }
}