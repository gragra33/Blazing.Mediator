namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query handler for registration testing in the dependency injection container.
/// Used to verify that query handlers are properly registered and resolved.
/// </summary>
public class TestRegistrationQueryHandler : IRequestHandler<TestRegistrationQuery, string>
{
    /// <summary>
    /// Handles the test registration query by returning a fixed response.
    /// </summary>
    /// <param name="request">The test registration query to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the string "TestRegistrationQueryHandler".</returns>
    public Task<string> Handle(TestRegistrationQuery request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("TestRegistrationQueryHandler");
    }
}