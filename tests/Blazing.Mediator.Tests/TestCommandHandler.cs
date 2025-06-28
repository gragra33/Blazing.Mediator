namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler used for unit testing the mediator functionality.
/// Handles TestCommand instances and tracks the last executed command.
/// </summary>
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    /// <summary>
    /// Gets the last command that was executed by this handler.
    /// Used for testing purposes to verify that the handler was called correctly.
    /// </summary>
    public static TestCommand? LastExecutedCommand { get; private set; }

    /// <summary>
    /// Handles the test command by storing it for later verification.
    /// </summary>
    /// <param name="request">The test command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(TestCommand request, CancellationToken cancellationToken = default)
    {
        LastExecutedCommand = request;
        return Task.CompletedTask;
    }
}