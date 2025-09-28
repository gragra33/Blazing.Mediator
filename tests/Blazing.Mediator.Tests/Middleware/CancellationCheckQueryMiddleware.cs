using Blazing.Mediator;

/// <summary>
/// Cancellation check query middleware for testing cancellation token handling.
/// </summary>
public class CancellationCheckQueryMiddleware : IRequestMiddleware<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Handles the query request asynchronously.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested.</exception>
    /// <returns>The response from the next handler.</returns>
    public async Task<string> HandleAsync(MiddlewareTestQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await next();
    }
}