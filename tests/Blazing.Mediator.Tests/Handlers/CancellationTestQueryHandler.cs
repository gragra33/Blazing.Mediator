namespace Blazing.Mediator.Tests;

/// <summary>
/// Test query handler used for testing cancellation token handling.
/// </summary>
public class CancellationTestQueryHandler : IRequestHandler<CancellationTestQuery, string>
{
    /// <summary>
    /// Handles the cancellation test query.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation and contains the response.</returns>
    public Task<string> Handle(CancellationTestQuery request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult("result");
    }
}