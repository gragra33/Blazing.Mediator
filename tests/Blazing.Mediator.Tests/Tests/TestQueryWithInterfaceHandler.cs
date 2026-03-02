namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestQueryWithInterface.
/// </summary>
public class TestQueryWithInterfaceHandler : IRequestHandler<TestQueryWithInterface, string>
{
    public ValueTask<string> Handle(TestQueryWithInterface request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult($"Handled: {request.Value}");
    }
}