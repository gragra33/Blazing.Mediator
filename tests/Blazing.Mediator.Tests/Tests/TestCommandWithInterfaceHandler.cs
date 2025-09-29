namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Handler for TestCommandWithInterface.
/// </summary>
public class TestCommandWithInterfaceHandler : IRequestHandler<TestCommandWithInterface, int>
{
    public Task<int> Handle(TestCommandWithInterface request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request.Value?.Length ?? 0);
    }
}