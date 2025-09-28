using Blazing.Mediator;

/// <summary>
/// Throwing query middleware for testing exception handling.
/// </summary>
public class ThrowingQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">Always thrown to test exception handling.</exception>
    /// <returns>This method never completes successfully.</returns>
    public Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Query middleware exception");
    }
}