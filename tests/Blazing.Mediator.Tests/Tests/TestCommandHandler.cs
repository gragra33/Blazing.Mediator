namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test command handler.
/// </summary>
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}