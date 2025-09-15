namespace Blazing.Mediator.Tests;

/// <summary>
/// Abstract handler implementation used for testing that abstract types are skipped during registration.
/// </summary>
public abstract class AbstractHandler : IRequestHandler<TestCommand>
{
    /// <summary>
    /// Handles the test command. Must be implemented by derived classes.
    /// </summary>
    /// <param name="request">The test command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Handle(TestCommand request, CancellationToken cancellationToken = default);
}