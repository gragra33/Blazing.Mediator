using Blazing.Mediator.Abstractions;

/// <summary>
/// Auto-discovery test middleware with static Order property.
/// </summary>
public class AutoDiscoveryStaticOrderMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Gets the static order priority for this middleware.
    /// </summary>
    public static int Order => 5;

    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "StaticOrder: " prefix.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"StaticOrder: {result}";
    }
}
