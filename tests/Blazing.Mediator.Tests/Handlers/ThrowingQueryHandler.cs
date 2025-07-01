namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query handler that always throws an exception for testing error handling.
/// Used to verify mediator behavior when query handlers fail.
/// </summary>
public class ThrowingQueryHandler : IRequestHandler<ThrowingQuery, string>
{
    /// <summary>
    /// Handles the throwing query by always throwing an exception.
    /// </summary>
    /// <param name="request">The query that triggers the exception.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>Never returns as it always throws an exception.</returns>
    /// <exception cref="InvalidOperationException">Always thrown to test error handling.</exception>
    public Task<string> Handle(ThrowingQuery request, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Query handler threw an exception");
    }
}