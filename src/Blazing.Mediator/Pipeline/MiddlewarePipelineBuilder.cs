namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    private readonly List<MiddlewareInfo> _middlewareInfos = [];

    private record MiddlewareInfo(Type Type, int Order, object? Configuration = null);

    /// <summary>
    /// Determines the order for a middleware type, using next available order after highest explicit order as fallback.
    /// Orders range from -999 to 999. Middleware without explicit order are assigned incrementally after the highest explicit order.
    /// </summary>
    private int GetMiddlewareOrder(Type middlewareType)
    {
        // Try to get order from a static Order property or field first
        var orderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            int staticOrder = (int)orderProperty.GetValue(null)!;
            return Math.Clamp(staticOrder, -999, 999);
        }

        var orderField = middlewareType.GetField("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            int staticOrder = (int)orderField.GetValue(null)!;
            return Math.Clamp(staticOrder, -999, 999);
        }

        // Check for OrderAttribute if it exists (common pattern)
        var orderAttribute = middlewareType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute != null)
        {
            var orderProp = orderAttribute.GetType().GetProperty("Order");
            if (orderProp != null && orderProp.PropertyType == typeof(int))
            {
                int attrOrder = (int)orderProp.GetValue(orderAttribute)!;
                return Math.Clamp(attrOrder, -999, 999);
            }
        }

        // Try to get order from instance Order property (for IRequestMiddleware implementations)
        var instanceOrderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty != null && instanceOrderProperty.PropertyType == typeof(int))
        {
            // Check if the property has a custom implementation (not the default interface implementation)
            // We can do this by checking if it's not virtual or if it's overridden
            if (!instanceOrderProperty.GetGetMethod()!.IsVirtual || 
                instanceOrderProperty.DeclaringType != typeof(IRequestMiddleware<,>) && 
                instanceOrderProperty.DeclaringType != typeof(IRequestMiddleware<>))
            {
                try
                {
                    // Create a temporary instance to get the Order value
                    object? instance = Activator.CreateInstance(middlewareType);
                    if (instance != null)
                    {
                        int orderValue = (int)instanceOrderProperty.GetValue(instance)!;
                        // Only use non-default values as explicit orders
                        if (orderValue != 0) 
                        {
                            return Math.Clamp(orderValue, -999, 999);
                        }
                    }
                }
                catch
                {
                    // If we can't create an instance, fall through to fallback logic
                }
            }
        }

        // Fallback: assign order after the highest explicitly set order
        // If no middleware registered yet, start from 1
        if (_middlewareInfos.Count == 0)
        {
            return 1;
        }

        // Find the highest explicit order and add 1
        var highestExplicitOrder = _middlewareInfos.Max(m => m.Order);
        
        // Ensure we don't exceed the maximum allowed order
        var nextOrder = highestExplicitOrder + 1;
        return Math.Min(nextOrder, 999);
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new MiddlewareInfo(middlewareType, order));
        return this;
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType)
    {
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new MiddlewareInfo(middlewareType, order));
        return this;
    }

    /// <summary>
    /// Adds middleware with configuration to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements IRequestMiddleware.</typeparam>
    /// <param name="configuration">Optional configuration object for the middleware.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new MiddlewareInfo(middlewareType, order, configuration));
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
        _ = Guid.NewGuid().ToString("N")[..8];

        RequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Check if this middleware supports the 2-parameter IRequestMiddleware<TRequest, TResponse>
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 2 }:
                        // Create the specific generic type for this request/response pair
                        actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest), typeof(TResponse));
                        break;
                    case { Length: 1 }:
                        // This is a 1-parameter middleware, skip it in this pipeline (it's for commands without response)
                        continue;
                    default:
                        // Unsupported number of generic parameters
                        continue;
                }
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

            // Use the cached order from registration
            applicableMiddleware.Add((actualMiddlewareType, middlewareInfo.Order));
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
            (Type middlewareType, int _) = applicableMiddleware[i];
            RequestHandlerDelegate<TResponse> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async () =>
            {
                // Create middleware instance using DI container
                IRequestMiddleware<TRequest, TResponse>? middleware = serviceProvider.GetService(middlewareType) as IRequestMiddleware<TRequest, TResponse>;

                return middleware switch
                {
                    null => throw new InvalidOperationException(
                        $"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container."),
                    // Check if this is conditional middleware and should execute
                    IConditionalMiddleware<TRequest, TResponse> conditionalMiddleware when !conditionalMiddleware
                        .ShouldExecute(request) => await currentPipeline(),
                    _ => await middleware.HandleAsync(request, currentPipeline, cancellationToken)
                };
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
        RequestHandlerDelegate pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach ((Type? middlewareType, int order, var _) in _middlewareInfos)
        {
            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Check if this middleware supports the 1-parameter IRequestMiddleware<TRequest>
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 1 }:
                        // Create the specific generic type for this command
                        actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest));
                        break;
                    case { Length: 2 }:
                        // This is a 2-parameter middleware, skip it in this pipeline (it's for queries/commands with response)
                        continue;
                    default:
                        // Unsupported number of generic parameters
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }
            
            // Check if this middleware type implements IRequestMiddleware<TRequest>
            Type genericMiddlewareType = typeof(IRequestMiddleware<>).MakeGenericType(typeof(TRequest));
            
            if (actualMiddlewareType.GetInterfaces().All(i => i != genericMiddlewareType))
            {
                // This middleware doesn't handle this request type, skip it
                continue;
            }

            // Use the cached order from registration
            applicableMiddleware.Add((actualMiddlewareType, Order: order));
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
            (Type middlewareType, int _) = applicableMiddleware[i];
            RequestHandlerDelegate currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;


            pipeline = async () =>
            {
                // Create middleware instance using DI container
                IRequestMiddleware<TRequest>? middleware = serviceProvider.GetService(middlewareType) as IRequestMiddleware<TRequest>;
                
                switch (middleware)
                {
                    case null:
                        throw new InvalidOperationException($"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                    // Check if this is conditional middleware and should execute
                    case IConditionalMiddleware<TRequest> conditionalMiddleware when !conditionalMiddleware.ShouldExecute(request):
                        // Skip this middleware
                        await currentPipeline();
                        return;
                    default:
                        await middleware.HandleAsync(request, currentPipeline, cancellationToken);
                        break;
                }
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
