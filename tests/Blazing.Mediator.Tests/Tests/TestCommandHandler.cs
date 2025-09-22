namespace Blazing.Mediator.Tests.Statistics;

/// <summary>
/// Test command handler.
/// </summary>
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}