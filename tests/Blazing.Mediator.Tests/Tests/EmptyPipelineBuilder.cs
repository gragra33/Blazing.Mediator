using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Empty pipeline builder that doesn't have ExecutePipeline methods for testing fallback behavior.
/// </summary>
public class EmptyPipelineBuilder : IMiddlewarePipelineBuilder
{
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class
    {
        return this;
    }

    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType)
    {
        return this;
    }

    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse>
    {
        return finalHandler;
    }

    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest
    {
        return finalHandler;
    }

    public StreamRequestHandlerDelegate<TResponse> BuildStreamPipeline<TRequest, TResponse>(IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler) where TRequest : IStreamRequest<TResponse>
    {
        return finalHandler;
    }

    public Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return finalHandler();
    }

    public Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        return finalHandler();
    }

    public IAsyncEnumerable<TResponse> ExecuteStreamPipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IStreamRequest<TResponse>
    {
        return finalHandler();
    }

    // Note: This class intentionally doesn't have ExecutePipeline methods to test fallback behavior
}