namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler that always throws an exception for testing error handling.
/// Used to verify mediator behavior when command handlers fail.
/// </summary>
public class ThrowingCommandHandler : IRequestHandler<ThrowingCommand>
{
    /// <summary>
    /// Handles the throwing command by always throwing an exception.
    /// </summary>
    /// <param name="request">The command that triggers the exception.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Never returns as it always throws an exception.</returns>
    /// <exception cref="InvalidOperationException">Always thrown to test error handling.</exception>
    public Task Handle(ThrowingCommand request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Handler threw an exception");
    }
}