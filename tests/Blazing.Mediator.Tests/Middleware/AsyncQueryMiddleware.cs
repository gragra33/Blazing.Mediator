using Blazing.Mediator;

/// <summary>
/// Async query middleware for testing asynchronous operations.
/// </summary>
public class AsyncQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "Async: " prefix after a delay.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        string result = await next();
        return $"Async: {result}";
    }
}