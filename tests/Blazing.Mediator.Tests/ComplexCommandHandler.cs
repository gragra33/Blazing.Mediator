namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler for complex commands used for testing mediator functionality with complex objects.
/// </summary>
public class ComplexCommandHandler : IRequestHandler<ComplexCommand>
{
    /// <summary>
    /// Gets the last complex command that was executed by this handler.
    /// Used for testing purposes to verify that the handler was called correctly.
    /// </summary>
    public static ComplexCommand? LastExecutedCommand { get; private set; }

    /// <summary>
    /// Handles the complex command by storing it for later verification.
    /// </summary>
    /// <param name="request">The complex command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(ComplexCommand request, CancellationToken cancellationToken = default)
    {
        LastExecutedCommand = request;
        return Task.CompletedTask;
    }
}