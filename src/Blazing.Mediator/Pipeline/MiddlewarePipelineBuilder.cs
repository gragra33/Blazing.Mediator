namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public sealed class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    private readonly List<MiddlewareInfo> _middlewareInfos = [];

    private sealed record MiddlewareInfo(Type Type, int Order, object? Configuration = null);

    #region AddMiddleware overloads

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

    #endregion

    #region Build overloads

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
    /// Builds the middleware pipeline for the specified command type.
    /// </summary>
    public RequestHandlerDelegate Build<TRequest>(
        IServiceProvider serviceProvider,
        RequestHandlerDelegate finalHandler)
        where TRequest : IRequest
    {
        return () => throw new InvalidOperationException("Use ExecutePipeline method instead for commands");
    }

    /// <summary>
    /// Builds the middleware pipeline for the specified stream request type.
    /// </summary>
    public StreamRequestHandlerDelegate<TResponse> BuildStreamPipeline<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        StreamRequestHandlerDelegate<TResponse> finalHandler)
        where TRequest : IStreamRequest<TResponse>
    {
        return () => throw new InvalidOperationException("Use ExecuteStreamPipeline method instead for stream requests");
    }

    #endregion

    #region ExecutePipeline overloads

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
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!CanSatisfyGenericConstraints(middlewareType, typeof(TRequest), typeof(TResponse)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this request/response pair
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest), typeof(TResponse));
                        }
                        catch (ArgumentException)
                        {
                            // Generic constraints were not satisfied, skip this middleware
                            continue;
                        }
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

            // Use the cached order from registration, but try to get actual order from instance if available
            int actualOrder = middlewareInfo.Order;

            // Try to get the actual Order from instance if we can create one from DI
            try
            {
                var instance = serviceProvider.GetService(actualMiddlewareType);
                if (instance != null)
                {
                    var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                    if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                    {
                        actualOrder = (int)orderProperty.GetValue(instance)!;
                    }
                }
            }
            catch
            {
                // If we can't get instance, use cached order
            }

            applicableMiddleware.Add((actualMiddlewareType, actualOrder));
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

        foreach ((Type middlewareType, int order, _) in _middlewareInfos)
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
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!CanSatisfyGenericConstraints(middlewareType, typeof(TRequest)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this command
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest));
                        }
                        catch (ArgumentException)
                        {
                            // Generic constraints were not satisfied, skip this middleware
                            continue;
                        }
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
    /// Executes the middleware pipeline for a stream request with support for ordering and conditional execution.
    /// </summary>
    public IAsyncEnumerable<TResponse> ExecuteStreamPipeline<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        StreamRequestHandlerDelegate<TResponse> finalHandler,
        CancellationToken cancellationToken)
        where TRequest : IStreamRequest<TResponse>
    {
        return ExecuteStreamPipelineInternal(request, serviceProvider, finalHandler, cancellationToken);
    }

    #endregion

    #region Inspector Methods

    /// <inheritdoc />
    public IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        List<Type> list = [];
        list.AddRange(_middlewareInfos.Select(info => info.Type));

        return list;
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration()
    {
        List<(Type Type, object? Configuration)> list = [];
        list.AddRange(_middlewareInfos.Select(info => (info.Type, info.Configuration)));

        return list;
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null)
    {
        if (serviceProvider == null)
        {
            // Return cached registration-time order values
            List<(Type Type, int Order, object? Configuration)> list = [];
            list.AddRange(_middlewareInfos.Select(info => (info.Type, info.Order, info.Configuration)));

            return list;
        }

        // Use service provider to get actual runtime order values using the same logic as ExecutePipeline
        var result = new List<(Type Type, int Order, object? Configuration)>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            int actualOrder = middlewareInfo.Order; // Start with cached order

            try
            {
                if (middlewareInfo.Type.IsGenericTypeDefinition)
                {
                    // Use the same logic as ExecutePipeline to make concrete types
                    var genericParams = middlewareInfo.Type.GetGenericArguments();
                    Type? actualMiddlewareType = null;

                    switch (genericParams)
                    {
                        case { Length: 2 }:
                            // Check if this is meant for IRequestMiddleware<TRequest, TResponse>
                            // Create concrete type with minimal types that satisfy IRequest constraints
                            if (CanSatisfyGenericConstraints(middlewareInfo.Type, typeof(MinimalRequest), typeof(object)))
                            {
                                try
                                {
                                    actualMiddlewareType = middlewareInfo.Type.MakeGenericType(typeof(MinimalRequest), typeof(object));
                                }
                                catch (ArgumentException)
                                {
                                    // Constraints not satisfied, skip
                                }
                            }
                            break;
                        case { Length: 1 }:
                            // Check if this is meant for IRequestMiddleware<TRequest> 
                            // Create concrete type for single parameter middleware
                            if (CanSatisfyGenericConstraints(middlewareInfo.Type, typeof(MinimalRequest)))
                            {
                                try
                                {
                                    actualMiddlewareType = middlewareInfo.Type.MakeGenericType(typeof(MinimalRequest));
                                }
                                catch (ArgumentException)
                                {
                                    // Constraints not satisfied, skip
                                }
                            }
                            break;
                    }

                    if (actualMiddlewareType != null)
                    {
                        // Try to get the actual Order from instance - same as ExecutePipeline
                        var instance = serviceProvider.GetService(actualMiddlewareType);
                        if (instance != null)
                        {
                            var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                            if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                            {
                                actualOrder = (int)orderProperty.GetValue(instance)!;
                            }
                        }
                    }
                }
                else
                {
                    // For non-generic types, resolve directly from DI - same as ExecutePipeline
                    var instance = serviceProvider.GetService(middlewareInfo.Type);
                    if (instance != null)
                    {
                        var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                        {
                            actualOrder = (int)orderProperty.GetValue(instance)!;
                        }
                    }
                }
            }
            catch
            {
                // If we can't get instance, use cached order - same as ExecutePipeline
            }

            result.Add((middlewareInfo.Type, actualOrder, middlewareInfo.Configuration));
        }

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true)
    {
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);

        var analysisResults = new List<MiddlewareAnalysis>();

        foreach (var (type, order, configuration) in middlewareInfos.OrderBy(m => m.Order))
        {
            var orderDisplay = order == int.MaxValue ? "Default" : order.ToString();
            var className = type.Name;
            var typeParameters = type.IsGenericType ?
                $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.Name))}>" :
                string.Empty;

            var detailed = isDetailed ?? true;

            // Extract generic constraints
            var genericConstraints = detailed ? GetGenericConstraints(type) : string.Empty;

            // Always discover handlers, but adjust output detail based on mode
            var handlerInfo = detailed ? configuration : null; // Include configuration only in detailed mode

            analysisResults.Add(new MiddlewareAnalysis(
                Type: type,
                Order: order,
                OrderDisplay: orderDisplay,
                ClassName: className,
                TypeParameters: detailed ? typeParameters : string.Empty, // Skip type parameters in compact mode
                GenericConstraints: genericConstraints,
                Configuration: handlerInfo
            ));
        }

        return analysisResults;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines the order for a middleware type. Middleware with explicit Order property use that value.
    /// Middleware without explicit order are assigned incrementally starting from int.MaxValue - 1000000 
    /// to ensure they execute after all explicitly ordered middleware.
    /// </summary>
    private int GetMiddlewareOrder(Type middlewareType)
    {
        // Try to get order from a static Order property or field first
        var orderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            int staticOrder = (int)orderProperty.GetValue(null)!;
            return staticOrder; // No clamping - use full int range
        }

        var orderField = middlewareType.GetField("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            int staticOrder = (int)orderField.GetValue(null)!;
            return staticOrder; // No clamping - use full int range
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
                return attrOrder; // No clamping - use full int range
            }
        }

        // Try to get order from instance Order property (for IRequestMiddleware implementations)
        var instanceOrderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty != null && instanceOrderProperty.PropertyType == typeof(int))
        {
            try
            {
                // Handle generic type definitions by making them concrete first
                Type typeToInstantiate = middlewareType;
                if (middlewareType.IsGenericTypeDefinition)
                {
                    // For generic types, try to make a concrete type to get the Order value
                    var genericArgs = middlewareType.GetGenericArguments();
                    Type[] concreteArgs = new Type[genericArgs.Length];
                    for (int i = 0; i < genericArgs.Length; i++)
                    {
                        concreteArgs[i] = typeof(object); // Use object as placeholder
                    }
                    typeToInstantiate = middlewareType.MakeGenericType(concreteArgs);
                }

                // Create a temporary instance to get the Order value
                object? instance = Activator.CreateInstance(typeToInstantiate);
                if (instance != null)
                {
                    int orderValue = (int)instanceOrderProperty.GetValue(instance)!;
                    return orderValue; // No clamping - use full int range
                }
            }
            catch
            {
                // If we can't create an instance, fall through to fallback logic
            }
        }

        // Fallback: middleware has no explicit order, assign it after all explicitly ordered middleware
        // Use a high base value (int.MaxValue - 1000000) and increment from there to maintain discovery order
        const int unorderedMiddlewareBaseOrder = int.MaxValue - 1000000;

        // Count how many unordered middleware we already have to maintain discovery order
        int unorderedCount = _middlewareInfos.Count(m => m.Order >= unorderedMiddlewareBaseOrder);

        return unorderedMiddlewareBaseOrder + unorderedCount;
    }

    /// <summary>
    /// Internal implementation of stream pipeline execution
    /// </summary>
    private async IAsyncEnumerable<TResponse> ExecuteStreamPipelineInternal<TRequest, TResponse>(
        TRequest request,
        IServiceProvider serviceProvider,
        StreamRequestHandlerDelegate<TResponse> finalHandler,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        where TRequest : IStreamRequest<TResponse>
    {
        StreamRequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this stream request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;

            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Check if this middleware supports the IStreamRequestMiddleware<TRequest, TResponse>
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 2 }:
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!CanSatisfyGenericConstraints(middlewareType, typeof(TRequest), typeof(TResponse)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this request/response pair
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest), typeof(TResponse));
                        }
                        catch (ArgumentException)
                        {
                            // Generic constraints were not satisfied, skip this middleware
                            continue;
                        }
                        break;
                    default:
                        // Unsupported number of generic parameters for stream middleware
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }

            // Check if this middleware type implements IStreamRequestMiddleware<TRequest, TResponse>
            if (!actualMiddlewareType.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == typeof(TRequest) &&
                i.GetGenericArguments()[1] == typeof(TResponse)))
            {
                // This middleware doesn't handle this stream request type, skip it
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
            StreamRequestHandlerDelegate<TResponse> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = () =>
            {
                // Create middleware instance using DI container
                IStreamRequestMiddleware<TRequest, TResponse>? middleware = serviceProvider.GetService(middlewareType) as IStreamRequestMiddleware<TRequest, TResponse>;

                return middleware switch
                {
                    null => throw new InvalidOperationException(
                        $"Could not create instance of stream middleware {middlewareName}. Make sure the middleware is registered in the DI container."),
                    // Check if this is conditional middleware and should execute
                    IConditionalStreamRequestMiddleware<TRequest, TResponse> conditionalMiddleware when !conditionalMiddleware
                        .ShouldExecute(request) => currentPipeline(),
                    _ => middleware.HandleAsync(request, currentPipeline, cancellationToken)
                };
            };
        }

        // Execute the pipeline and yield results
        await foreach (TResponse item in pipeline().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Checks if a generic type definition can be instantiated with the given type arguments
    /// by validating all generic constraints.
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition to check.</param>
    /// <param name="typeArguments">The type arguments to validate against the constraints.</param>
    /// <returns>True if the type can be instantiated with the given arguments, false otherwise.</returns>
    private static bool CanSatisfyGenericConstraints(Type genericTypeDefinition, params Type[] typeArguments)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            return false;

        var genericParameters = genericTypeDefinition.GetGenericArguments();

        if (genericParameters.Length != typeArguments.Length)
            return false;

        for (int i = 0; i < genericParameters.Length; i++)
        {
            var parameter = genericParameters[i];
            var argument = typeArguments[i];

            // Check class constraint
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && argument.IsValueType)
            {
                return false;
            }

            // Check struct constraint
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
                (!argument.IsValueType || (argument.IsGenericType && argument.GetGenericTypeDefinition() == typeof(Nullable<>))))
            {
                return false;
            }

            // Check new() constraint
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
                !argument.IsValueType && argument.GetConstructor(Type.EmptyTypes) == null)
            {
                return false;
            }

            // Check type constraints (where T : SomeType)
            var constraints = parameter.GetGenericParameterConstraints();
            foreach (var constraint in constraints)
            {
                // For now, only enforce constraints that we can confidently validate
                // to avoid false negatives that break existing functionality
                if (constraint is { IsInterface: true, IsGenericType: false })
                {
                    // Simple interface constraint (like IDisposable)
                    if (!constraint.IsAssignableFrom(argument))
                    {
                        return false;
                    }
                }
                else if (constraint is { IsClass: true, IsGenericType: false } && !constraint.IsAssignableFrom(argument)) // Simple class constraint (like Exception)
                {
                    return false;
                }
                // For complex constraints involving generic types or generic parameters,
                // let runtime handle the validation to avoid breaking existing scenarios
            }
        }

        return true;
    }

    /// <summary>
    /// Extracts generic constraints from a type for display purposes.
    /// </summary>
    /// <param name="type">The type to analyze for generic constraints.</param>
    /// <returns>A formatted string describing the generic constraints.</returns>
    private static string GetGenericConstraints(Type type)
    {
        if (!type.IsGenericTypeDefinition)
            return string.Empty;

        var genericParameters = type.GetGenericArguments();
        if (genericParameters.Length == 0)
            return string.Empty;

        var constraintParts = new List<string>();

        foreach (var parameter in genericParameters)
        {
            var parameterConstraints = new List<string>();

            // Check for reference type constraint (class)
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                parameterConstraints.Add("class");
            }

            // Check for value type constraint (struct)
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                parameterConstraints.Add("struct");
            }

            // Add type constraints (interfaces and base classes)
            var typeConstraints = parameter.GetGenericParameterConstraints();
            parameterConstraints.AddRange(typeConstraints
                .Where(constraint => constraint.IsInterface || constraint.IsClass)
                .Select(FormatTypeName));

            // Check for new() constraint
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                parameterConstraints.Add("new()");
            }

            // If this parameter has constraints, add them
            if (parameterConstraints.Count > 0)
            {
                var constraintText = $"where {parameter.Name} : {string.Join(", ", parameterConstraints)}";
                constraintParts.Add(constraintText);
            }
        }

        return constraintParts.Count > 0 ? string.Join(" ", constraintParts) : string.Empty;
    }

    /// <summary>
    /// Formats a type name for display, handling generic types nicely.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A formatted type name string.</returns>
    private static string FormatTypeName(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var genericTypeName = type.Name;
        var backtickIndex = genericTypeName.IndexOf('`');
        if (backtickIndex > 0)
        {
            genericTypeName = genericTypeName[..backtickIndex];
        }

        var genericArgs = type.GetGenericArguments();
        var genericArgNames = genericArgs.Select(arg => arg.IsGenericParameter ? arg.Name : FormatTypeName(arg));

        return $"{genericTypeName}<{string.Join(", ", genericArgNames)}>";
    }

    /// <summary>
    /// Minimal request implementation that satisfies both IRequest and IRequest{T} constraints for middleware inspection.
    /// </summary>
    private sealed class MinimalRequest : IRequest, IRequest<object> { /* skipped */ }

    #endregion

    /// <summary>
    /// Returns the list of middleware types that were actually executed for a given request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>List of executed middleware types in order of execution.</returns>
    public async Task<IReadOnlyList<Type>> GetExecutedMiddlewareAsync<TRequest, TResponse>(TRequest request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var executed = new List<Type>();
        RequestHandlerDelegate<TResponse> pipeline = () => Task.FromResult(default(TResponse)!);
        List<(Type Type, int Order)> applicableMiddleware = [];
        foreach (var middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 2 }:
                        if (!CanSatisfyGenericConstraints(middlewareType, typeof(TRequest), typeof(TResponse)))
                            continue;
                        try { actualMiddlewareType = middlewareType.MakeGenericType(typeof(TRequest), typeof(TResponse)); }
                        catch { continue; }
                        break;
                    default:
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }
            if (!actualMiddlewareType.GetInterfaces().Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == typeof(TRequest) &&
                i.GetGenericArguments()[1] == typeof(TResponse)))
            {
                continue;
            }
            applicableMiddleware.Add((actualMiddlewareType, middlewareInfo.Order));
        }
        applicableMiddleware.Sort((a, b) => a.Order.CompareTo(b.Order));
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int _) = applicableMiddleware[i];
            RequestHandlerDelegate<TResponse> currentPipeline = pipeline;
            pipeline = async () =>
            {
                var middleware = serviceProvider.GetService(middlewareType) as IRequestMiddleware<TRequest, TResponse>;
                if (middleware == null) throw new InvalidOperationException();
                executed.Add(middlewareType);
                return await middleware.HandleAsync(request, currentPipeline, cancellationToken);
            };
        }
        try { await pipeline(); } catch { }
        return executed;
    }
}