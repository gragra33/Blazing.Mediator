namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test command handler for multiple handler tests.
/// </summary>
public class SecondTestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}