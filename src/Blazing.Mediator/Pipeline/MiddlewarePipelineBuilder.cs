namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// Enhanced with BasePipelineBuilder for optimal performance and shared functionality.
/// </summary>
public sealed class MiddlewarePipelineBuilder 
    : BasePipelineBuilder<MiddlewarePipelineBuilder>, IMiddlewarePipelineBuilder, IMiddlewarePipelineInspector
{
    /// <summary>
    /// CRTP (Curiously Recurring Template Pattern) implementation for fluent API.
    /// </summary>
    private MiddlewarePipelineBuilder Self => this;

    /// <summary>
    /// Initializes a new instance of the MiddlewarePipelineBuilder class.
    /// </summary>
    /// <param name="mediatorLogger">Optional MediatorLogger for debug-level logging of pipeline operations.</param>
    public MiddlewarePipelineBuilder(MediatorLogger? mediatorLogger = null) : base(mediatorLogger)
    {
    }

    #region AddMiddleware overloads

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class
    {
        var middlewareType = typeof(TMiddleware);
        AddMiddlewareCore(middlewareType);
        return this;
    }

    /// <inheritdoc />
    public IMiddlewarePipelineBuilder AddMiddleware(Type middlewareType)
    {
        AddMiddlewareCore(middlewareType);
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
        AddMiddlewareCore(middlewareType, configuration);
        return this;
    }

    #endregion

    #region Fallback Types Override

    /// <summary>
    /// Gets fallback types specific to request middleware for concrete type creation.
    /// </summary>
    protected override Type[] GetFallbackTypes()
    {
        return [
            typeof(InternalRequestPlaceholder), 
            typeof(InternalCommandPlaceholder), 
            typeof(object), 
            typeof(string)
        ];
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

    #region ExecutePipeline overloads - Request-Specific Implementation

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
        _mediatorLogger?.MiddlewarePipelineStarted(actualRequestType.Name, _middlewareInfos.Count);

        RequestHandlerDelegate<TResponse> pipeline = finalHandler;

        // Get middleware types that can handle this request type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            // Debug logging: Checking middleware compatibility
            _mediatorLogger?.MiddlewareCompatibilityCheck(middlewareInfo.Type.Name, actualRequestType.Name);

            Type middlewareType = middlewareInfo.Type;

            // Handle open generic types by making them closed generic types
            Type? actualMiddlewareType;

            // Use cached generic type check for better performance
            if (PipelineUtilities.IsGenericTypeCached(middlewareType) && PipelineUtilities.IsGenericTypeDefinitionCached(middlewareType))
            {
                // Use cached generic_arguments lookup
                var genericParams = PipelineUtilities.GetGenericArgumentsCached(middlewareType);
                switch (genericParams)
                {
                    case { Length: 2 }:
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!PipelineUtilities.CanSatisfyGenericConstraints(middlewareType, actualRequestType, typeof(TResponse)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this request/response pair
                        if (!PipelineUtilities.TryMakeGenericType(middlewareType, [actualRequestType, typeof(TResponse)], out actualMiddlewareType))
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

            // Use cached interfaces lookup to avoid repeated GetInterfaces() calls
            var interfaces = PipelineUtilities.GetInterfacesCached(actualMiddlewareType!);

            bool isCompatible = interfaces.Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == actualRequestType &&
                i.GetGenericArguments()[1] == typeof(TResponse));

            if (!isCompatible)
            {
                // Debug logging: Middleware not compatible
                _mediatorLogger?.MiddlewareIncompatible(actualMiddlewareType?.Name ?? "", actualRequestType.Name, middlewareInfo.Order);

                // This middleware doesn't handle this request type, skip it
                continue;
            }

            // Use the cached order from registration, but try to get actual order from instance if available
            int actualOrder = GetRuntimeOrder(middlewareInfo, serviceProvider);

            applicableMiddleware.Add((actualMiddlewareType, actualOrder)!);

            // Debug logging: Middleware successfully added
            _mediatorLogger?.MiddlewareCompatible(actualMiddlewareType?.Name ?? "", actualRequestType.Name, actualOrder);
        }

        // Debug logging: Pipeline execution info
        _mediatorLogger?.PipelineExecution(applicableMiddleware.Count);

        // Sort middleware by order (lower numbers execute first), then by registration order
        // Use pre-calculated registration indices for fast sorting operations
        var registrationIndices = CreateRegistrationIndices();

        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;

            // Use pre-calculated indices instead of expensive FindIndex calls
            int indexA = PipelineUtilities.GetRegistrationIndex(a.Type, registrationIndices);
            int indexB = PipelineUtilities.GetRegistrationIndex(b.Type, registrationIndices);
            return indexA.CompareTo(indexB);
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
                if (!expectedMiddlewareInterfaceType.IsInstanceOfType(middlewareInstance))
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

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;

            // Handle open generic types by making them closed generic types
            Type? actualMiddlewareType;
            // Use cached generic type check for better performance
            if (PipelineUtilities.IsGenericTypeCached(middlewareType) && PipelineUtilities.IsGenericTypeDefinitionCached(middlewareType))
            {
                // Use cached generic_arguments lookup
                var genericParams = PipelineUtilities.GetGenericArgumentsCached(middlewareType);
                switch (genericParams)
                {
                    case { Length: 1 }:
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!PipelineUtilities.CanSatisfyGenericConstraints(middlewareType, typeof(TRequest)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this command
                        if (!PipelineUtilities.TryMakeGenericType(middlewareType, [typeof(TRequest)], out actualMiddlewareType))
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

            if (PipelineUtilities.GetInterfacesCached(actualMiddlewareType!).All(i => i != genericMiddlewareType))
            {
                // This middleware doesn't handle this request type, skip it
                continue;
            }

            // Use the cached order from registration
            applicableMiddleware.Add((actualMiddlewareType, middlewareInfo.Order)!);
        }

        // Sort middleware by order (lower numbers execute first), then by registration order
        // Use pre-calculated registration indices for fast sorting operations
        var registrationIndices = CreateRegistrationIndices();

        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;

            // Use pre-calculated indices instead of expensive FindIndex calls
            int indexA = PipelineUtilities.GetRegistrationIndex(a.Type, registrationIndices);
            int indexB = PipelineUtilities.GetRegistrationIndex(b.Type, registrationIndices);
            return indexA.CompareTo(indexB);
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
            Type? actualMiddlewareType;
            // Use cached generic type check for better performance
            if (PipelineUtilities.IsGenericTypeCached(middlewareType) && PipelineUtilities.IsGenericTypeDefinitionCached(middlewareType))
            {
                // Use cached generic_arguments lookup
                var genericParams = PipelineUtilities.GetGenericArgumentsCached(middlewareType);
                switch (genericParams)
                {
                    case { Length: 2 }:
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        if (!PipelineUtilities.CanSatisfyGenericConstraints(middlewareType, typeof(TRequest), typeof(TResponse)))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this request/response pair
                        if (!PipelineUtilities.TryMakeGenericType(middlewareType, [typeof(TRequest), typeof(TResponse)], out actualMiddlewareType))
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
            // Use cached interfaces lookup to avoid repeated GetInterfaces() calls
            if (!PipelineUtilities.GetInterfacesCached(actualMiddlewareType!).Any(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>) &&
                i.GetGenericArguments()[0] == typeof(TRequest) &&
                i.GetGenericArguments()[1] == typeof(TResponse)))
            {
                // This middleware doesn't handle this stream request type, skip it
                continue;
            }

            // Use the cached order from registration
            applicableMiddleware.Add((actualMiddlewareType, middlewareInfo.Order)!);
        }

        // Sort middleware by order (lower numbers execute first), then by registration order
        // Use pre-calculated registration indices for fast sorting operations
        var registrationIndices = CreateRegistrationIndices();

        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;

            // Use pre-calculated indices instead of expensive FindIndex calls
            int indexA = PipelineUtilities.GetRegistrationIndex(a.Type, registrationIndices);
            int indexB = PipelineUtilities.GetRegistrationIndex(b.Type, registrationIndices);
            return indexA.CompareTo(indexB);
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

    #region Helper Methods Specific to Request Middleware

    /// <summary>
    /// Checks if middleware is conditional and whether it should execute.
    /// </summary>
    private static bool IsConditionalMiddleware(object middleware, Type requestType, Type responseType, object request)
    {
        Type conditionalInterfaceType = typeof(IConditionalMiddleware<,>).MakeGenericType(requestType, responseType);
        if (conditionalInterfaceType.IsInstanceOfType(middleware))
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
}