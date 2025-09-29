using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// A wrapper around the MiddlewarePipelineBuilder that tracks middleware execution for testing.
/// This extends the standard pipeline builder to provide execution tracking capabilities needed for telemetry tests.
/// </summary>
public class TrackingMiddlewarePipelineBuilder(IMiddlewareExecutionTracker executionTracker)
    : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    private readonly MiddlewarePipelineBuilder _innerBuilder = new();
    private readonly IMiddlewareExecutionTracker _executionTracker = executionTracker ?? throw new ArgumentNullException(nameof(executionTracker));

    #region IMiddlewarePipelineBuilder implementation

    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>() where TMiddleware : class
    {
        _innerBuilder.AddMiddleware<TMiddleware>();
        return this;
    }

    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType)
    {
        _innerBuilder.AddMiddleware(middlewareType);
        return this;
    }

    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler) where TRequest : IRequest<TResponse>
    {
        return _innerBuilder.Build<TRequest, TResponse>(serviceProvider, finalHandler);
    }

    public RequestHandlerDelegate Build<TRequest>(IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler) where TRequest : IRequest
    {
        return _innerBuilder.Build<TRequest>(serviceProvider, finalHandler);
    }

    public async Task<TResponse> ExecutePipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>
    {
        _executionTracker.Clear();
        return await ExecutePipelineWithTracking<TRequest, TResponse>(request, serviceProvider, finalHandler, cancellationToken);
    }

    public async Task ExecutePipeline<TRequest>(TRequest request, IServiceProvider serviceProvider, RequestHandlerDelegate finalHandler, CancellationToken cancellationToken) where TRequest : IRequest
    {
        _executionTracker.Clear();
        await ExecutePipelineWithTracking<TRequest>(request, serviceProvider, finalHandler, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> ExecuteStreamPipeline<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler, CancellationToken cancellationToken) where TRequest : IStreamRequest<TResponse>
    {
        _executionTracker.Clear();
        return _innerBuilder.ExecuteStreamPipeline(request, serviceProvider, finalHandler, cancellationToken);
    }

    public StreamRequestHandlerDelegate<TResponse> BuildStreamPipeline<TRequest, TResponse>(IServiceProvider serviceProvider, StreamRequestHandlerDelegate<TResponse> finalHandler) where TRequest : IStreamRequest<TResponse>
    {
        return _innerBuilder.BuildStreamPipeline<TRequest, TResponse>(serviceProvider, finalHandler);
    }

    #endregion

    #region IMiddlewarePipelineInspector implementation

    public IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        return _innerBuilder.GetRegisteredMiddleware();
    }

    public IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration()
    {
        return _innerBuilder.GetMiddlewareConfiguration();
    }

    public IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null)
    {
        return _innerBuilder.GetDetailedMiddlewareInfo(serviceProvider);
    }

    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true)
    {
        return _innerBuilder.AnalyzeMiddleware(serviceProvider, isDetailed);
    }

    #endregion

    #region Execution tracking implementation

    /// <summary>
    /// Gets the execution tracker for accessing executed middleware information.
    /// </summary>
    public IMiddlewareExecutionTracker ExecutionTracker => _executionTracker;

    /// <summary>
    /// Custom implementation of ExecutePipeline that tracks middleware execution.
    /// This mirrors the logic in MiddlewarePipelineBuilder but adds execution tracking.
    /// </summary>
    private async Task<TResponse> ExecutePipelineWithTracking<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate<TResponse> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        RequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        var applicableMiddleware = new List<(Type Type, int Order)>();
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);

        foreach (var (type, order, _) in middlewareInfos)
        {
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (type.IsGenericTypeDefinition)
            {
                var genericParams = type.GetGenericArguments();
                if (genericParams.Length == 2)
                {
                    try
                    {
                        actualMiddlewareType = type.MakeGenericType(typeof(TRequest), typeof(TResponse));
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            else
            {
                actualMiddlewareType = type;
            }

            // Check if this middleware type implements IRequestMiddleware<TRequest, TResponse>
            if (!actualMiddlewareType.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == typeof(TRequest) &&
                i.GetGenericArguments()[1] == typeof(TResponse)))
            {
                continue;
            }

            applicableMiddleware.Add((actualMiddlewareType, order));
        }

        // Sort middleware by order (lower numbers execute first)
        applicableMiddleware.Sort((a, b) => a.Order.CompareTo(b.Order));

        // Build pipeline in reverse order so the first middleware in the sorted list runs first
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            var (middlewareType, _) = applicableMiddleware[i];
            var currentPipeline = pipeline;
            var middlewareName = middlewareType.Name;

            pipeline = async () =>
            {
                // Create middleware instance using DI container
                var middleware = serviceProvider.GetService(middlewareType) as IRequestMiddleware<TRequest, TResponse>;

                if (middleware == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }

                // Check if this is conditional middleware and should execute
                if (middleware is IConditionalMiddleware<TRequest, TResponse> conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(request))
                {
                    return await currentPipeline();
                }

                // Record that this middleware is executing
                _executionTracker.RecordExecution(middlewareType);

                // Execute the middleware
                return await middleware.HandleAsync(request, currentPipeline, cancellationToken);
            };
        }

        return await pipeline();
    }

    /// <summary>
    /// Custom implementation of ExecutePipeline for void commands that tracks middleware execution.
    /// </summary>
    private async Task ExecutePipelineWithTracking<TRequest>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        RequestHandlerDelegate pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        var applicableMiddleware = new List<(Type Type, int Order)>();
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);

        foreach (var (type, order, _) in middlewareInfos)
        {
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (type.IsGenericTypeDefinition)
            {
                var genericParams = type.GetGenericArguments();
                if (genericParams.Length == 1)
                {
                    try
                    {
                        actualMiddlewareType = type.MakeGenericType(typeof(TRequest));
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            else
            {
                actualMiddlewareType = type;
            }

            // Check if this middleware type implements IRequestMiddleware<TRequest>
            var genericMiddlewareType = typeof(IRequestMiddleware<>).MakeGenericType(typeof(TRequest));
            if (actualMiddlewareType.GetInterfaces().All(i => i != genericMiddlewareType))
            {
                continue;
            }

            applicableMiddleware.Add((actualMiddlewareType, order));
        }

        // Sort middleware by order (lower numbers execute first)
        applicableMiddleware.Sort((a, b) => a.Order.CompareTo(b.Order));

        // Build pipeline in reverse order so the first middleware in the sorted list runs first
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            var (middlewareType, _) = applicableMiddleware[i];
            var currentPipeline = pipeline;
            var middlewareName = middlewareType.Name;

            pipeline = async () =>
            {
                // Create middleware instance using DI container
                var middleware = serviceProvider.GetService(middlewareType) as IRequestMiddleware<TRequest>;

                if (middleware == null)
                {
                    throw new InvalidOperationException($"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }

                // Check if this is conditional middleware and should execute
                if (middleware is IConditionalMiddleware<TRequest> conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(request))
                {
                    await currentPipeline();
                    return;
                }

                // Record that this middleware is executing
                _executionTracker.RecordExecution(middlewareType);

                // Execute the middleware
                await middleware.HandleAsync(request, currentPipeline, cancellationToken);
            };
        }

        await pipeline();
    }

    #endregion
}