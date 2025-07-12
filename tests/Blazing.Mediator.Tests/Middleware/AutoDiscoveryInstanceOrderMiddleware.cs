using Blazing.Mediator.Abstractions;

/// <summary>
/// Auto-discovery test middleware with instance Order property.
/// </summary>
public class AutoDiscoveryInstanceOrderMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Gets the instance order priority for this middleware.
    /// </summary>
    public int Order => 10;

    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "InstanceOrder: " prefix.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"InstanceOrder: {result}";
    }
}
