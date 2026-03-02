namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test command handler for multiple handler tests.
/// Excluded from auto-discovery to prevent conflicts with TestCommandHandler.
/// </summary>
[ExcludeFromAutoDiscovery]
public class SecondTestCommandHandler : IRequestHandler<TestCommand>
{
    public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}