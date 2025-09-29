namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestQueryWithInterface.
/// </summary>
public class TestQueryWithInterfaceHandler : IRequestHandler<TestQueryWithInterface, string>
{
    public Task<string> Handle(TestQueryWithInterface request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Handled: {request.Value}");
    }
}