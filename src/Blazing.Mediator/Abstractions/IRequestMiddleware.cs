namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Generic middleware interface for processing requests in the mediator pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IRequestMiddleware<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Gets the execution order for this middleware. Lower numbers execute first.
    /// Default is 0 if not specified (neutral order).
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Handles the request processing with access to the next middleware in the pipeline.
    /// </summary>
    /// <param name="request">The request to process</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response</returns>
    Task<TResponse> HandleAsync(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken);
}

/// <summary>
/// Middleware interface for processing void commands in the mediator pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
/// <typeparam name="TRequest">The command type</typeparam>
public interface IRequestMiddleware<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Gets the execution order for this middleware. Lower numbers execute first.
    /// Default is 0 if not specified (neutral order).
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Handles the command processing with access to the next middleware in the pipeline.
    /// </summary>
    /// <param name="request">The command to process</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the completion of the command</returns>
    Task HandleAsync(
        TRequest request, 
        RequestHandlerDelegate next, 
        CancellationToken cancellationToken);
}
