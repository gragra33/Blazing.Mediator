namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test query handler for multiple handler tests.
/// Excluded from auto-discovery to prevent conflicts with TestQueryHandler.
/// </summary>
[ExcludeFromAutoDiscovery]
public class SecondTestQueryHandler : IRequestHandler<TestQuery, string>
{
    public ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Second: {request.Value}");
    }
}