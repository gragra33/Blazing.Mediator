using Blazing.Mediator;

/// <summary>
/// Conditional query middleware for testing conditional execution.
/// </summary>
public class ConditionalQueryMiddleware : IConditionalMiddleware<ConditionalQuery, string>
{
    /// <summary>
    /// Determines whether the middleware should execute.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <returns>True if the middleware should execute; otherwise, false.</returns>
    public bool ShouldExecute(ConditionalQuery request)
    {
        return request.ShouldExecuteMiddleware;
    }

    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response with "Conditional: " prefix.</returns>
    public async Task<string> HandleAsync(ConditionalQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        string result = await next();
        return $"Conditional: {result}";
    }
}