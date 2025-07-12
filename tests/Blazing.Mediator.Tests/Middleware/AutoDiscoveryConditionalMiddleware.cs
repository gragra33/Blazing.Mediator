using Blazing.Mediator.Abstractions;

/// <summary>
/// Auto-discovery test conditional middleware with static Order property.
/// </summary>
public class AutoDiscoveryConditionalMiddleware : IConditionalMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Gets the static order priority for this middleware.
    /// </summary>
    public static int Order => 15;

    /// <summary>
    /// Determines whether this middleware should execute for the given request.
    /// </summary>
    /// <param name="request">The request to check.</param>
    /// <returns>True if the middleware should execute.</returns>
    public bool ShouldExecute(MiddlewareTestQuery request)
    {
        // Only execute if the Value property contains "auto"
        return request.Value?.Contains("auto", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "Conditional: " prefix.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"Conditional: {result}";
    }
}
