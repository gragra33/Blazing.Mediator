namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for SimpleTestCommand.
/// </summary>
public class SimpleTestCommandHandler : IRequestHandler<SimpleTestCommand>
{
    public Task Handle(SimpleTestCommand request, CancellationToken cancellationToken)
    {
        // Simulate command execution
        return Task.CompletedTask;
    }
}