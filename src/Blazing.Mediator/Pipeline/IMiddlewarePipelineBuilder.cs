namespace Blazing.Mediator.Pipeline;

/// <summary>
/// Configuration interface for building middleware pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface IMiddlewarePipelineBuilder
{
    /// <summary>
    /// Adds a middleware type to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements IRequestMiddleware</typeparam>
    /// <returns>The pipeline builder for chaining</returns>
    IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class;

    /// <summary>
    /// Adds a middleware type to the pipeline using a Type parameter.
    /// </summary>
    /// <param name="middlewareType">The middleware type (can be open generic)</param>
    /// <returns>The pipeline builder for chaining</returns>
    IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType);

    /// <summary>
    /// Executes the middleware pipeline for a specific request.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The request to process</param>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the pipeline</returns>
    Task<TResponse> ExecutePipeline<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate<TResponse> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Builds the middleware pipeline for the specified request and response types.
    /// </summary>
    /// <typeparam name="TRequest">The request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="serviceProvider">Service provider for resolving middleware instances</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <returns>A delegate that executes the complete pipeline</returns>
    RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        RequestHandlerDelegate<TResponse> finalHandler)
        where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Executes the middleware pipeline for a void command.
    /// </summary>
    /// <typeparam name="TRequest">The command type</typeparam>
    /// <param name="request">The command to process</param>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the completion of the command</returns>
    Task ExecutePipeline<TRequest>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest;

    /// <summary>
    /// Builds the middleware pipeline for the specified command type.
    /// </summary>
    /// <typeparam name="TRequest">The command type</typeparam>
    /// <param name="serviceProvider">Service provider for resolving middleware instances</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <returns>A delegate that executes the complete pipeline</returns>
    RequestHandlerDelegate Build<TRequest>(
        IServiceProvider serviceProvider,
        RequestHandlerDelegate finalHandler)
        where TRequest : IRequest;

    /// <summary>
    /// Executes the middleware pipeline for a stream request.
    /// </summary>
    /// <typeparam name="TRequest">The stream request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The stream request to process</param>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An async enumerable of response items from the pipeline</returns>
    IAsyncEnumerable<TResponse> ExecuteStreamPipeline<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        StreamRequestHandlerDelegate<TResponse> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IStreamRequest<TResponse>;

    /// <summary>
    /// Builds the middleware pipeline for the specified stream request type.
    /// </summary>
    /// <typeparam name="TRequest">The stream request type</typeparam>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="serviceProvider">Service provider for resolving middleware instances</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <returns>A delegate that executes the complete stream pipeline</returns>
    StreamRequestHandlerDelegate<TResponse> BuildStreamPipeline<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        StreamRequestHandlerDelegate<TResponse> finalHandler)
        where TRequest : IStreamRequest<TResponse>;
}
