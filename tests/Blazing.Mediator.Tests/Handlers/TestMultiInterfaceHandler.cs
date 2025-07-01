namespace Blazing.Mediator.Tests;

/// <summary>
/// Test handler that implements multiple request handler interfaces.
/// Used to verify that handlers can implement both command and query interfaces correctly.
/// </summary>
public class TestMultiInterfaceHandler :
    IRequestHandler<TestMultiCommand>,
    IRequestHandler<TestMultiQuery, string>
{
    /// <summary>
    /// Handles the test multi command.
    /// </summary>
    /// <param name="request">The test multi command to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A completed task.</returns>
    public Task Handle(TestMultiCommand request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Handles the test multi query and returns a fixed response.
    /// </summary>
    /// <param name="request">The test multi query to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the string "multi".</returns>
    public Task<string> Handle(TestMultiQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult("multi");
}