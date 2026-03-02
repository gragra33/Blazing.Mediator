namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestCommandWithInterface.
/// </summary>
public class TestCommandWithInterfaceHandler : IRequestHandler<TestCommandWithInterface, int>
{
    public async ValueTask<int> Handle(TestCommandWithInterface request, CancellationToken cancellationToken)
    {
        return request.Value?.Length ?? 0;
    }
}