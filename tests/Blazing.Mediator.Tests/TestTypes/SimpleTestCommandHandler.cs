namespace Blazing.Mediator.Tests.TestTypes;

/// <summary>
/// Handler for SimpleTestCommand.
/// </summary>
public class SimpleTestCommandHandler : IRequestHandler<SimpleTestCommand>
{
    public ValueTask Handle(SimpleTestCommand request, CancellationToken cancellationToken)
    {
        // Simulate command execution
        return ValueTask.CompletedTask;
    }
}