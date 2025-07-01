namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler for registration testing in the dependency injection container.
/// Used to verify that command handlers are properly registered and resolved.
/// </summary>
public class TestRegistrationCommandHandler : IRequestHandler<TestRegistrationCommand>
{
    /// <summary>
    /// Handles the test registration command.
    /// </summary>
    /// <param name="request">The test registration command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(TestRegistrationCommand request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}