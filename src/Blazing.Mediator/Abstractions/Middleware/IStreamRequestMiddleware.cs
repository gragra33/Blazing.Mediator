namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Middleware interface for processing stream requests in the mediator pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The stream request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IStreamRequestMiddleware<in TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Gets the execution order for this middleware. Lower numbers execute first.
    /// Default is 0 if not specified (neutral order).
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Handles the stream request processing with access to the next middleware in the pipeline.
    /// </summary>
    /// <param name="request">The stream request to process</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response items</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
