using Blazing.Mediator;

/// <summary>
/// Middleware test query handler for testing middleware integration.
/// </summary>
public class MiddlewareTestQueryHandler : IRequestHandler<MiddlewareTestQuery, string>
{
    /// <summary>
    /// Handles the middleware test query.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response including the query value.</returns>
    public async ValueTask<string> Handle(MiddlewareTestQuery request, CancellationToken cancellationToken = default)
    {
        return $"Handler: {request.Value}";
    }
}