namespace Blazing.Mediator.Tests;

/// <summary>
/// Test command handler that returns null to test null handling behavior.
/// Used to verify mediator behavior when command handlers return null task values.
/// </summary>
public class TestNullCommandHandler : IRequestHandler<TestNullCommand>
{
    /// <summary>
    /// Handles the test null command by returning null.
    /// </summary>
    /// <param name="request">The test null command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A null task.</returns>
    public Task Handle(TestNullCommand request, CancellationToken cancellationToken = default)
    {
        return null!;
    }
}