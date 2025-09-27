namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public sealed class MiddlewarePipelineBuilder : IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    private readonly List<MiddlewareInfo> _middlewareInfos = [];
    private readonly ILogger<MiddlewarePipelineBuilder>? _logger;

    private sealed record MiddlewareInfo(Type Type, int Order, object? Configuration = null);

    /// <summary>
    /// Initializes a new instance of the MiddlewarePipelineBuilder class.
    /// </summary>
    /// <param name="logger">Optional logger for debug-level logging of pipeline operations.</param>
    public MiddlewarePipelineBuilder(ILogger<MiddlewarePipelineBuilder>? logger = null)
    {
        _logger = logger;
    }

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
    [Obsolete("Use ExecutePipeline method instead for queries")]
    public RequestHandlerDelegate<TResponse> Build<TRequest, TResponse>(
        IServiceProvider serviceProvider,
        RequestHandlerDelegate<TResponse> finalHandler)
        where TRequest : IRequest<TResponse>
    {
        // This method is not used - ExecutePipeline is used instead
        // Return a delegate that throws when called
        return () => throw new InvalidOperationException("Use ExecutePipeline method instead for queries");
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
        // Use the actual runtime type of the request instead of the generic parameter type
        Type actualRequestType = request.GetType();

        // Debug logging: Pipeline started
        _logger?.LogDebug("Request middleware pipeline started for {RequestType} with {MiddlewareCount} middleware components", actualRequestType.Name, _middlewareInfos.Count);

        _ = Guid.NewGuid().ToString("N")[..8];

        RequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            // Debug logging: Checking middleware compatibility
            _logger?.LogDebug("Checking middleware compatibility: {MiddlewareName} with request {RequestType}", middlewareInfo.Type.Name, actualRequestType.Name);

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
                        if (!CanSatisfyGenericConstraints(middlewareType, actualRequestType, typeof(TResponse)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this request/response pair
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(actualRequestType, typeof(TResponse));
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
            var interfaces = actualMiddlewareType.GetInterfaces();

            bool isCompatible = interfaces.Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == actualRequestType &&
                i.GetGenericArguments()[1] == typeof(TResponse));

            if (!isCompatible)
            {
                // Debug logging: Middleware not compatible
                _logger?.LogDebug("Middleware {MiddlewareName} is not compatible with request {RequestType}, order: {Order}", actualMiddlewareType.Name, actualRequestType.Name, middlewareInfo.Order);

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

            // Debug logging: Middleware successfully added
            _logger?.LogDebug("Middleware {MiddlewareName} is compatible with request {RequestType}, order: {Order}", actualMiddlewareType.Name, actualRequestType.Name, actualOrder);
        }

        // Debug logging: Pipeline execution info
        _logger?.LogDebug("Executing request middleware pipeline with {ApplicableMiddlewareCount} applicable middleware components", applicableMiddleware.Count);

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
                object? middlewareInstance = serviceProvider.GetService(middlewareType);

                if (middlewareInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }

                // Cast to the specific interface that this middleware implements using the actual request type
                Type expectedMiddlewareInterfaceType = typeof(IRequestMiddleware<,>).MakeGenericType(actualRequestType, typeof(TResponse));
                if (!expectedMiddlewareInterfaceType.IsAssignableFrom(middlewareInstance.GetType()))
                {
                    throw new InvalidOperationException(
                        $"Middleware {middlewareName} does not implement IRequestMiddleware<{actualRequestType.Name}, {typeof(TResponse).Name}>.");
                }

                // Use dynamic invocation to call HandleAsync with the correct types
                object middleware = middlewareInstance;

                return middleware switch
                {
                    // Check if this is conditional middleware and should execute
                    _ when IsConditionalMiddleware(middleware, actualRequestType, typeof(TResponse), request) => await currentPipeline().ConfigureAwait(false),
                    _ => await InvokeMiddlewareHandleAsync(middleware, request, currentPipeline, cancellationToken).ConfigureAwait(false)
                };
            };
        }
        TResponse result = await pipeline().ConfigureAwait(false);
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
                object? middlewareInstance = serviceProvider.GetService(middlewareType);

                if (middlewareInstance == null)
                {
                    throw new InvalidOperationException($"Could not create instance of middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }

                // Cast to the specific interface that this middleware implements
                if (middlewareInstance is not IRequestMiddleware<TRequest> middleware)
                {
                    throw new InvalidOperationException(
                        $"Middleware {middlewareName} does not implement IRequestMiddleware<{typeof(TRequest).Name}>.");
                }

                switch (middleware)
                {
                    // Check if this is conditional middleware and should execute
                    case IConditionalMiddleware<TRequest> conditionalMiddleware when !conditionalMiddleware.ShouldExecute(request):
                        // Skip this middleware
                        await currentPipeline().ConfigureAwait(false);
                        return;
                    default:
                        await middleware.HandleAsync(request, currentPipeline, cancellationToken).ConfigureAwait(false);
                        break;
                }
            };
        }

        await pipeline().ConfigureAwait(false);
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
                var middleware = (IStreamRequestMiddleware<TRequest, TResponse>)serviceProvider.GetRequiredService(middlewareType);

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

        // Use service provider to get actual runtime order values using improved logic
        var result = new List<(Type Type, int Order, object? Configuration)>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            int actualOrder = middlewareInfo.Order; // Start with cached order

            try
            {
                if (middlewareInfo.Type.IsGenericTypeDefinition)
                {
                    // For generic type definitions, try to find a suitable concrete type to instantiate
                    var genericParams = middlewareInfo.Type.GetGenericArguments();
                    Type? actualMiddlewareType = null;

                    switch (genericParams)
                    {
                        case { Length: 2 }:
                            // Try to find types that can satisfy the middleware constraints
                            actualMiddlewareType = TryCreateConcreteMiddlewareType(middlewareInfo.Type, serviceProvider, 2);
                            break;
                        case { Length: 1 }:
                            // Try to find types that can satisfy the middleware constraints
                            actualMiddlewareType = TryCreateConcreteMiddlewareType(middlewareInfo.Type, serviceProvider, 1);
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

    /// <summary>
    /// Attempts to create a concrete middleware type from a generic type definition by finding suitable type arguments.
    /// Uses fast fallback types to avoid expensive assembly scanning.
    /// </summary>
    private static Type? TryCreateConcreteMiddlewareType(Type middlewareTypeDefinition, IServiceProvider serviceProvider, int parameterCount)
    {
        if (!middlewareTypeDefinition.IsGenericTypeDefinition)
            return middlewareTypeDefinition;

        var genericParams = middlewareTypeDefinition.GetGenericArguments();
        if (genericParams.Length != parameterCount)
            return null;

        // Use fast fallback types instead of expensive assembly scanning
        // This significantly improves performance by avoiding AppDomain.CurrentDomain.GetAssemblies() 
        // and assembly.GetTypes() calls which can take 30+ seconds
        var fastFallbackTypes = new[] 
        { 
            typeof(InternalRequestPlaceholder), 
            typeof(InternalCommandPlaceholder), 
            typeof(object), 
            typeof(string)
        };

        // Try different combinations of fast fallback types as type arguments
        if (parameterCount == 2)
        {
            // For 2-parameter middleware (TRequest, TResponse)
            foreach (var requestType in fastFallbackTypes)
            {
                foreach (var responseType in new[] { typeof(object), typeof(string), typeof(int) })
                {
                    if (TryMakeGenericType(middlewareTypeDefinition, [requestType, responseType], out var concreteType))
                    {
                        return concreteType;
                    }
                }
            }
        }
        else if (parameterCount == 1)
        {
            // For 1-parameter middleware (TRequest)
            foreach (var requestType in fastFallbackTypes)
            {
                if (TryMakeGenericType(middlewareTypeDefinition, [requestType], out var concreteType))
                {
                    return concreteType;
                }
            }
        }

        // If fast fallback fails, return null instead of expensive assembly scanning
        // The middleware order will use the cached registration-time value
        return null;
    }

    /// <summary>
    /// Safely attempts to create a generic type with the given type arguments.
    /// </summary>
    private static bool TryMakeGenericType(Type genericTypeDefinition, Type[] typeArguments, out Type? concreteType)
    {
        concreteType = null;
        
        try
        {
            // First check if the constraints can be satisfied
            if (!CanSatisfyGenericConstraints(genericTypeDefinition, typeArguments))
            {
                return false;
            }

            // Try to create the concrete type
            concreteType = genericTypeDefinition.MakeGenericType(typeArguments);
            return true;
        }
        catch (ArgumentException ex)
        {
            // Constraints were not satisfied
            return false;
        }
        catch
        {
            // Other error
            return false;
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
    /// Gets the type name preserving generic arity notation for display purposes.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>The clean type name without backtick notation (e.g., "ErrorHandlingMiddleware" instead of "ErrorHandlingMiddleware`1").</returns>
    private static string GetCleanTypeName(Type type)
    {
        var typeName = type.Name;
        var backtickIndex = typeName.IndexOf('`');
        return backtickIndex > 0 ? typeName[..backtickIndex] : typeName;
    }

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true)
    {
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);

        var analysisResults = new List<MiddlewareAnalysis>();

        foreach (var (type, order, configuration) in middlewareInfos.OrderBy(m => m.Order))
        {
            var orderDisplay = order == int.MaxValue ? "Default" : order.ToString();
            var className = GetCleanTypeName(type);
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
            if (middlewareType.IsGenericTypeDefinition)
            {
                // For generic type definitions, we can't determine the actual order at registration time
                // The ExecutePipeline method will get the correct order from the concrete instance
                // Use a placeholder order that indicates this middleware has an explicit order
                // but needs to be resolved at execution time
                return int.MaxValue - 1000000; // Use fallback order for now
            }
            else
            {
                // For non-generic types, we can create an instance to get the order
                try
                {
                    object? instance = Activator.CreateInstance(middlewareType);
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
        }

        // Fallback: middleware has no explicit order, assign it after all explicitly ordered middleware
        // Use a high base value (int.MaxValue - 1000000) and increment from there to maintain discovery order
        const int unorderedMiddlewareBaseOrder = int.MaxValue - 1000000;

        // Count how many unordered middleware we already have to maintain discovery order
        int unorderedCount = _middlewareInfos.Count(m => m.Order >= unorderedMiddlewareBaseOrder);

        return unorderedMiddlewareBaseOrder + unorderedCount;
    }

    #endregion

    #region Placeholder Types for Constraint Satisfaction

    /// <summary>
    /// Internal command placeholder that satisfies IRequest constraints.
    /// This type is used internally for type constraint validation and should never be exposed to users.
    /// </summary>
    internal sealed class InternalCommandPlaceholder : IRequest { }

    /// <summary>
    /// Internal request placeholder that satisfies IRequest{T} constraints.
    /// This type is used internally for type constraint validation and should never be exposed to users.
    /// </summary>
    internal sealed class InternalRequestPlaceholder : IRequest<object> { }

    #endregion

    #region Helper methods for dynamic middleware invocation

    /// <summary>
    /// Checks if middleware is conditional and whether it should execute.
    /// </summary>
    private static bool IsConditionalMiddleware(object middleware, Type requestType, Type responseType, object request)
    {
        Type conditionalInterfaceType = typeof(IConditionalMiddleware<,>).MakeGenericType(requestType, responseType);
        if (conditionalInterfaceType.IsAssignableFrom(middleware.GetType()))
        {
            var shouldExecuteMethod = conditionalInterfaceType.GetMethod("ShouldExecute");
            if (shouldExecuteMethod != null)
            {
                var result = shouldExecuteMethod.Invoke(middleware, [request]);
                return result is bool shouldExecute && !shouldExecute;
            }
        }
        return false;
    }

    /// <summary>
    /// Invokes HandleAsync method on middleware using reflection.
    /// </summary>
    private static async Task<TResponse> InvokeMiddlewareHandleAsync<TResponse>(
        object middleware,
        object request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get the specific HandleAsync method with the correct parameter types
        var requestType = request.GetType();
        var nextType = typeof(RequestHandlerDelegate<TResponse>);
        var cancellationTokenType = typeof(CancellationToken);

        var handleAsyncMethod = middleware.GetType().GetMethod("HandleAsync",
            [requestType, nextType, cancellationTokenType]);

        if (handleAsyncMethod == null)
        {
            throw new InvalidOperationException($"Middleware {middleware.GetType().Name} does not have HandleAsync method with signature ({requestType.Name}, {nextType.Name}, {cancellationTokenType.Name}).");
        }

        try
        {
            var result = handleAsyncMethod.Invoke(middleware, [request, next, cancellationToken]);
            if (result is Task<TResponse> task)
            {
                return await task.ConfigureAwait(false);
            }

            throw new InvalidOperationException($"HandleAsync method on {middleware.GetType().Name} did not return Task<{typeof(TResponse).Name}>.");
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            // Unwrap reflection exceptions to preserve original exception type
            throw ex.InnerException;
        }
    }

    #endregion
}