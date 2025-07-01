namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler for testing cancellation token handling.
/// Used to verify that command handlers properly respond to cancellation requests.
/// </summary>
public class TestCancellableCommandHandler : IRequestHandler<TestCancellableCommand>
{
    /// <summary>
    /// Gets the last cancellation token that was passed to the handler.
    /// Used for testing purposes to verify the token was properly passed.
    /// </summary>
    public static CancellationToken LastCancellationToken { get; private set; }

    /// <summary>
    /// Handles the test cancellable command and checks for cancellation.
    /// </summary>
    /// <param name="request">The test cancellable command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task Handle(TestCancellableCommand request, CancellationToken cancellationToken = default)
    {
        LastCancellationToken = cancellationToken;
        await Task.Delay(1000, cancellationToken);
    }
}