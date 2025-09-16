using Blazing.Mediator.Abstractions;

/// <summary>
/// High order query middleware for testing execution order priority.
/// </summary>
public class HighOrderQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Gets the order priority for this middleware.
    /// </summary>
    public int Order => 100;

    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "HighOrder: " prefix.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"HighOrder: {result}";
    }
}