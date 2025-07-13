using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Mock pipeline builder that returns null from reflection calls for testing fallback behavior.
/// </summary>
public class MockPipelineBuilderReturningNull : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class => this;
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType) => this;
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse> => finalHandler;
    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest => finalHandler;
    public IReadOnlyList<Type> GetRegisteredMiddleware() => new List<Type>();
    public IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration() => new Dictionary<Type, object?>();

    // ExecutePipeline methods that will be called via reflection and can return appropriate results
    public Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return finalHandler();
    }

    public Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        return finalHandler();
    }

    // Stream pipeline methods for IStreamRequest support
    public IAsyncEnumerable<TResponse> ExecuteStreamPipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IStreamRequest<TResponse>
    {
        return finalHandler();
    }

    public StreamRequestHandlerDelegate<TResponse> BuildStreamPipeline<TRequest, TResponse>(IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler) where TRequest : IStreamRequest<TResponse>
    {
        return finalHandler;
    }
}