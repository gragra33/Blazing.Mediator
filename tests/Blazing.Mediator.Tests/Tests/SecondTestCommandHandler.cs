namespace Blazing.Mediator.Tests;

/// <summary>
/// Second test command handler for multiple handler tests.
/// </summary>
public class SecondTestCommandHandler : IRequestHandler<TestCommand>
{
    public ValueTask Handle(TestCommand request, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}