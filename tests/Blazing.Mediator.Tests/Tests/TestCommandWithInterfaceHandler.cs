namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestCommandWithInterface.
/// </summary>
public class TestCommandWithInterfaceHandler : IRequestHandler<TestCommandWithInterface, int>
{
    public ValueTask<int> Handle(TestCommandWithInterface request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(request.Value?.Length ?? 0);
    }
}