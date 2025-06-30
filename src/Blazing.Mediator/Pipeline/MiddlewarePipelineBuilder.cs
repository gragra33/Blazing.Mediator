namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    private readonly List<MiddlewareInfo> _middlewareInfos = new();

    private record MiddlewareInfo(Type Type, object? Configuration = null);

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class
    {
        _middlewareInfos.Add(new MiddlewareInfo(typeof(TMiddleware)));
        return this;
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType)
    {
        _middlewareInfos.Add(new MiddlewareInfo(middlewareType));
        return this;
    }

    /// <summary>
    /// Adds middleware with configuration.
    /// </summary>
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class
    {
        _middlewareInfos.Add(new MiddlewareInfo(typeof(TMiddleware), configuration));
        return this;
    }

    /// <inheritdoc />
    public IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        return _middlewareInfos.Select(info => info.Type).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Type, object?> GetMiddlewareConfiguration()
    {
        return _middlewareInfos.ToDictionary(info => info.Type, info => info.Configuration);
    }

    /// <inheritdoc />
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(
        IServiceProvider serviceProvider, 
        RequestHandlerDelegate<TResponse> finalHandler)
        where TRequest : IRequest<TResponse>
    {
        // Return a factory that will build the actual pipeline when executed
        return () => throw new InvalidOperationException("Use ExecutePipeline method instead");
    }

    /// <summary>
    /// Executes the middleware pipeline for a specific request with support for ordering and conditional execution.
    /// </summary>
    public async Task<TResponse> ExecutePipeline<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate<TResponse> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        string requestId = Guid.NewGuid().ToString("N")[..8];
        string requestType = request.GetType().Name;
        
        RequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = new List<(Type Type, int Order)>();

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Create the specific generic type for this request/response pair
                actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest), typeof(TResponse));
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }
            
            // Check if this middleware type implements IRequestMiddleware<TRequest, TResponse>
            if (!actualMiddlewareType.GetInterfaces().Any(i => i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == typeof(TRequest) &&
                i.GetGenericArguments()[1] == typeof(TResponse)))
            {
                // This middleware doesn't handle this request type, skip it
                continue;
            }

            // Get the order from the middleware if it has one
            int order = 0;
            try
            {
                // Create instance to get the order (middleware should have parameterless constructor)
                if (Activator.CreateInstance(actualMiddlewareType) is IRequestMiddleware<TRequest, TResponse> tempMiddleware)
                {
                    order = tempMiddleware.Order;
                }
            }
            catch
            {
                // If we can't create the middleware to get the order, use default order 0
            }

            applicableMiddleware.Add((actualMiddlewareType, order));
        }

        // Sort middleware by order (lower numbers execute first), then by registration order
        applicableMiddleware.Sort((a, b) => 
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;
            
            // If orders are equal, maintain registration order
            return _middlewareInfos.FindIndex(info => 
                info.Type == a.Type || 
                (info.Type.IsGenericTypeDefinition && a.Type.IsGenericType && info.Type == a.Type.GetGenericTypeDefinition())
            ).CompareTo(_middlewareInfos.FindIndex(info => 
                info.Type == b.Type || 
                (info.Type.IsGenericTypeDefinition && b.Type.IsGenericType && info.Type == b.Type.GetGenericTypeDefinition())
            ));
        });

        // Build pipeline in reverse order so the first middleware in the sorted list runs first
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int middlewareOrder) = applicableMiddleware[i];
            RequestHandlerDelegate<TResponse> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async () =>
            {
                // Create middleware instance directly (middleware should have parameterless constructor)
                IRequestMiddleware<TRequest, TResponse>? middleware = Activator.CreateInstance(middlewareType) as IRequestMiddleware<TRequest, TResponse>;
                
                if (middleware == null)
                {
                    throw new InvalidOperationException($"Could not create instance of middleware {middlewareName}");
                }

                // Check if this is conditional middleware and should execute
                if (middleware is IConditionalMiddleware<TRequest, TResponse> conditionalMiddleware && !conditionalMiddleware.ShouldExecute(request))
                {
                    // Skip this middleware
                    return await currentPipeline();
                }

                return await middleware.HandleAsync(request, currentPipeline, cancellationToken);
            };
        }
        TResponse result = await pipeline();
        return result;
    }

    /// <summary>
    /// Executes the middleware pipeline for a void command with support for ordering and conditional execution.
    /// </summary>
    public async Task ExecutePipeline<TRequest>(
        TRequest request,
        IServiceProvider serviceProvider,
        RequestHandlerDelegate finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IRequest
    {
        string requestId = Guid.NewGuid().ToString("N")[..8];
        string requestType = request.GetType().Name;
        
        RequestHandlerDelegate pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = new List<(Type Type, int Order)>();

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Create the specific generic type for this command
                actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest));
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }
            
            // Check if this middleware type implements IRequestMiddleware<TRequest>
            Type genericMiddlewareType = typeof(IRequestMiddleware<>).MakeGenericType(typeof(TRequest));
            
            if (!actualMiddlewareType.GetInterfaces().Any(i => i == genericMiddlewareType))
            {
                // This middleware doesn't handle this request type, skip it
                continue;
            }

            // Get the order from the middleware if it has one
            int order = 0;
            try
            {
                // Create instance to get the order (middleware should have parameterless constructor)
                if (Activator.CreateInstance(actualMiddlewareType) is IRequestMiddleware<TRequest> tempMiddleware)
                {
                    order = tempMiddleware.Order;
                }
            }
            catch
            {
                // If we can't create the middleware to get the order, use default order 0
            }

            applicableMiddleware.Add((actualMiddlewareType, order));
        }


        // Sort middleware by order (lower numbers execute first), then by registration order
        applicableMiddleware.Sort((a, b) => 
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;
            
            // If orders are equal, maintain registration order
            return _middlewareInfos.FindIndex(info => 
                info.Type == a.Type || 
                (info.Type.IsGenericTypeDefinition && a.Type.IsGenericType && info.Type == a.Type.GetGenericTypeDefinition())
            ).CompareTo(_middlewareInfos.FindIndex(info => 
                info.Type == b.Type || 
                (info.Type.IsGenericTypeDefinition && b.Type.IsGenericType && info.Type == b.Type.GetGenericTypeDefinition())
            ));
        });

        // Build pipeline in reverse order so the first middleware in the sorted list runs first
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int middlewareOrder) = applicableMiddleware[i];
            RequestHandlerDelegate currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;


            pipeline = async () =>
            {
                // Create middleware instance directly (middleware should have parameterless constructor)
                IRequestMiddleware<TRequest>? middleware = Activator.CreateInstance(middlewareType) as IRequestMiddleware<TRequest>;
                
                if (middleware == null)
                {
                    throw new InvalidOperationException($"Could not create instance of middleware {middlewareName}");
                }

                // Check if this is conditional middleware and should execute
                if (middleware is IConditionalMiddleware<TRequest> conditionalMiddleware && !conditionalMiddleware.ShouldExecute(request))
                {
                    // Skip this middleware
                    await currentPipeline();
                    return;
                }

                await middleware.HandleAsync(request, currentPipeline, cancellationToken);
            };
        }

        await pipeline();
    }

    /// <summary>
    /// Builds the middleware pipeline for the specified command type.
    /// </summary>
    public RequestHandlerDelegate Build<TRequest>(
        IServiceProvider serviceProvider, 
        RequestHandlerDelegate finalHandler)
        where TRequest : IRequest
    {
        return () => throw new InvalidOperationException("Use ExecutePipeline method instead for commands");
    }
}
