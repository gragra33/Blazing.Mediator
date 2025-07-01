namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query handler used for unit testing the mediator functionality.
/// Handles TestQuery instances and returns a formatted string result.
/// </summary>
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    /// <summary>
    /// Handles the test query and returns a formatted result.
    /// </summary>
    /// <param name="request">The test query to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing a formatted string result.</returns>
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Result: {request.Value}");
    }
}