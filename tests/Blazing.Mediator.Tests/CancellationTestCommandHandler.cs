namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler used for testing cancellation token handling.
/// </summary>
public class CancellationTestCommandHandler : IRequestHandler<CancellationTestCommand>
{
    /// <summary>
    /// Handles the cancellation test command.
    /// </summary>
    /// <param name="request">The command to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Handle(CancellationTestCommand request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}