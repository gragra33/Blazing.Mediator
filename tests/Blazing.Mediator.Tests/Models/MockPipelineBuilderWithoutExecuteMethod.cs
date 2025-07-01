using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator.Tests;

/// <summary>
/// Mock classes for testing edge cases
/// </summary>

/// <summary>
/// Mock pipeline builder that simulates not having the ExecutePipeline method for reflection testing.
/// </summary>
public class MockPipelineBuilderWithoutExecuteMethod : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class => this;
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType) => this;
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse> => finalHandler;
    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest => finalHandler;
    public IReadOnlyList<Type> GetRegisteredMiddleware() => new List<Type>();
    public IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration() => new Dictionary<Type, object?>();

    // Required interface methods - but these will be found by reflection, so the test needs different expectations
    public Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        return finalHandler();
    }

    public Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        return finalHandler();
    }
}