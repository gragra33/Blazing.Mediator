namespace Blazing.Mediator.Tests;

/// <summary>
/// Test abstract handler for testing abstract class registration scenarios.
/// Used to verify that abstract handlers are not registered in the dependency injection container.
/// </summary>
public abstract class TestAbstractHandler : IRequestHandler<TestAbstractCommand>
{
    /// <summary>
    /// Abstract method to handle the test abstract command.
    /// </summary>
    /// <param name="request">The test abstract command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public abstract Task Handle(TestAbstractCommand request, CancellationToken cancellationToken = default);
}