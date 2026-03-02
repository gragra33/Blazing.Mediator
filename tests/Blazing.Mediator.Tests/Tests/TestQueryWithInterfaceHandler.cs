namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestQueryWithInterface.
/// </summary>
public class TestQueryWithInterfaceHandler : IRequestHandler<TestQueryWithInterface, string>
{
    public async ValueTask<string> Handle(TestQueryWithInterface request, CancellationToken cancellationToken)
    {
        return $"Handled: {request.Value}";
    }
}