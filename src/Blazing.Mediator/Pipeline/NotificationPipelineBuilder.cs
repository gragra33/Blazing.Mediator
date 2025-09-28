namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// Builds and executes notification middleware pipelines with support for generic types and conditional execution.
/// Enhanced with BasePipelineBuilder for optimal performance and shared functionality.
/// </summary>
public sealed class NotificationPipelineBuilder
    : BasePipelineBuilder<NotificationPipelineBuilder>, INotificationPipelineBuilder, INotificationMiddlewarePipelineInspector
{
    private const string OrderPropertyName = "Order";

    /// <summary>
    /// CRTP pattern implementation for fluent API.
    /// </summary>
    protected override NotificationPipelineBuilder Self => this;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPipelineBuilder"/> class with optional logging.
    /// </summary>
    /// <param name="mediatorLogger">Optional <see cref="MediatorLogger"/> for enhanced logging.</param>
    public NotificationPipelineBuilder(MediatorLogger? mediatorLogger = null) : base(mediatorLogger)
    {
    }

    #region AddMiddleware overloads

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        AddMiddlewareCore(middlewareType);
        return this;
    }

    /// <summary>
    /// Adds notification middleware with configuration to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements <see cref="INotificationMiddleware"/>.</typeparam>
    /// <param name="configuration">Optional configuration object for the middleware.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        AddMiddlewareCore(middlewareType, configuration);
        return this;
    }

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware(Type middlewareType)
    {
        if (!typeof(INotificationMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new ArgumentException($"Type {middlewareType.Name} does not implement INotificationMiddleware", nameof(middlewareType));
        }

        AddMiddlewareCore(middlewareType);
        return this;
    }

    #endregion

    #region Fallback Types Override

    /// <summary>
    /// Gets fallback types specific to notification middleware for concrete type creation.
    /// </summary>
    /// <returns>An array of fallback types for notification middleware.</returns>
    protected override Type[] GetFallbackTypes()
    {
        return [
            typeof(MinimalNotification),
            typeof(object)
        ];
    }

    #endregion

    #region Additional Inspector Method Override

    /// <summary>
    /// Analyzes the registered middleware in the pipeline using the provided <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider for middleware resolution.</param>
    /// <returns>A read-only list of <see cref="MiddlewareAnalysis"/> results.</returns>
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider)
    {
        return AnalyzeMiddleware(serviceProvider, true); // Call the base implementation with detailed=true
    }

    #endregion

    #region Build overloads

    /// <inheritdoc />
    [Obsolete("Use ExecutePipeline method instead for better performance and consistency")]
    public NotificationDelegate<TNotification> Build<TNotification>(
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification
    {
        // Legacy build method - maintained for compatibility but ExecutePipeline is preferred
        return BuildLegacyPipeline(serviceProvider, finalHandler);
    }

    /// <summary>
    /// Legacy build method for backward compatibility.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="serviceProvider">The service provider for middleware resolution.</param>
    /// <param name="finalHandler">The final handler to execute after all middleware.</param>
    /// <returns>A delegate that executes the complete pipeline.</returns>
    private NotificationDelegate<TNotification> BuildLegacyPipeline<TNotification>(
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification
    {
        // Sort middleware by order (ascending - lower numbers execute first)
        var sortedMiddleware = _middlewareInfos
            .OrderBy(m => m.Order)
            .ToList();

        // Build pipeline from right to left (last middleware first)
        NotificationDelegate<TNotification> pipeline = finalHandler;

        for (int i = sortedMiddleware.Count - 1; i >= 0; i--)
        {
            var middlewareInfo = sortedMiddleware[i];
            var currentPipeline = pipeline;

            pipeline = async (notification, cancellationToken) =>
            {
                var middleware = (INotificationMiddleware)serviceProvider.GetRequiredService(middlewareInfo.Type);

                // Check if it's conditional middleware
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notification))
                {
                    await currentPipeline(notification, cancellationToken).ConfigureAwait(false);
                    return;
                }

                await middleware.InvokeAsync(notification, currentPipeline, cancellationToken).ConfigureAwait(false);
            };
        }

        return pipeline;
    }

    #endregion

    #region ExecutePipeline - Enhanced Implementation with Logging

    /// <summary>
    /// Executes the notification middleware pipeline with enhanced support for generic types, 
    /// conditional execution, and comprehensive logging.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being processed.</typeparam>
    /// <param name="notification">The notification to process.</param>
    /// <param name="serviceProvider">Service provider for middleware resolution.</param>
    /// <param name="finalHandler">Final handler to execute after all middleware.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        Type actualNotificationType = notification.GetType();
        var pipelineId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _mediatorLogger?.NotificationPipelineStarted(actualNotificationType.Name, _middlewareInfos.Count, pipelineId);

        NotificationDelegate<TNotification> pipeline = finalHandler;
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (MiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            Type? actualMiddlewareType;
            // Use cached generic type check for better performance
            if (PipelineUtilities.IsGenericTypeCached(middlewareType) && PipelineUtilities.IsGenericTypeDefinitionCached(middlewareType))
            {
                // Use cached generic arguments lookup
                var genericParams = PipelineUtilities.GetGenericArgumentsCached(middlewareType);
                switch (genericParams)
                {
                    case { Length: 1 }:
                        bool canSatisfyConstraints = true; // Always true now, as constraint validation is removed
                        if (!canSatisfyConstraints)
                        {
                            continue;
                        }
                        if (!PipelineUtilities.TryMakeGenericType(middlewareType, [actualNotificationType], out actualMiddlewareType))
                        {
                            continue;
                        }
                        break;
                    default:
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }

            // Use cached interfaces lookup to optimize interface checking
            var interfaces = PipelineUtilities.GetInterfacesCached(actualMiddlewareType!);
            
            bool isCompatible = typeof(INotificationMiddleware).IsAssignableFrom(actualMiddlewareType);
            if (!isCompatible)
            {
                continue;
            }

            // Cache constrained interfaces lookup
            var constrainedInterfaces = GetConstrainedNotificationInterfaces(interfaces);

            if (constrainedInterfaces.Length > 0)
            {
                bool hasCompatibleConstraint = HasCompatibleConstraint(constrainedInterfaces, actualNotificationType);
                if (!hasCompatibleConstraint)
                {
                    continue;
                }
            }

            int actualOrder = GetRuntimeOrder(middlewareInfo, serviceProvider);
            applicableMiddleware.Add((actualMiddlewareType, actualOrder)!);
        }

        // Use pre-calculated registration indices for fast sorting operations
        var registrationIndices = CreateRegistrationIndices();

        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;
            
            // Use pre-calculated indices instead of expensive operations
            int indexA = PipelineUtilities.GetRegistrationIndex(a.Type, registrationIndices);
            int indexB = PipelineUtilities.GetRegistrationIndex(b.Type, registrationIndices);
            return indexA.CompareTo(indexB);
        });

        int executedCount = 0;
        int skippedCount = _middlewareInfos.Count - applicableMiddleware.Count;

        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int order) = applicableMiddleware[i];
            NotificationDelegate<TNotification> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async (notif, ct) =>
            {
                var middlewareStopwatch = System.Diagnostics.Stopwatch.StartNew();
                object? middlewareInstance = serviceProvider.GetService(middlewareType);
                if (middlewareInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of notification middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }
                if (middlewareInstance is not INotificationMiddleware middleware)
                {
                    throw new InvalidOperationException(
                        $"Middleware {middlewareName} does not implement INotificationMiddleware.");
                }
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notif))
                {
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }
                if (IsTypeConstrainedMiddleware(middleware, actualNotificationType, notif))
                {
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }

                try
                {
                    bool invokedConstrainedMethod = await TryInvokeConstrainedMethodAsync(middleware, notif, currentPipeline, ct, actualNotificationType);
                    if (!invokedConstrainedMethod)
                    {
                        await middleware.InvokeAsync(notif, currentPipeline, ct).ConfigureAwait(false);
                    }

                    executedCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    middlewareStopwatch.Stop();
                }
            };
        }

        await pipeline(notification, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();
    }

    #endregion

    #region Helper Methods Specific to Notification Middleware

    /// <summary>
    /// Attempts to invoke a type-constrained <see cref="INotificationMiddleware{TNotification}"/> method if available.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="middleware">The middleware instance.</param>
    /// <param name="notification">The notification instance.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="actualNotificationType">The actual notification type.</param>
    /// <returns>True if a constrained method was invoked; otherwise, false.</returns>
    private async Task<bool> TryInvokeConstrainedMethodAsync<TNotification>(
        INotificationMiddleware middleware, 
        TNotification notification, 
        NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken,
        Type actualNotificationType)
        where TNotification : INotification
    {
        // Use cached interface lookup to avoid repeated reflection
        var interfaces = PipelineUtilities.GetInterfacesCached(middleware.GetType());
        var constrainedInterfaces = GetConstrainedNotificationInterfaces(interfaces);
        
        if (constrainedInterfaces.Length == 0)
        {
            return false;
        }
        
        foreach (Type constrainedInterface in constrainedInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            bool isCompatible = constraintType.IsAssignableFrom(actualNotificationType);
            
            if (isCompatible)
            {
                var constrainedMethod = constrainedInterface.GetMethod("InvokeAsync");
                if (constrainedMethod != null)
                {
                    try
                    {
                        var delegateType = typeof(NotificationDelegate<>).MakeGenericType(constraintType);
                        var constrainedNext = Delegate.CreateDelegate(delegateType, next.Target, next.Method);
                        var task = (Task?)constrainedMethod.Invoke(middleware, [notification, constrainedNext, cancellationToken]);
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            return true;
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Determines if the middleware is type-constrained and not compatible with the given notification type.
    /// </summary>
    /// <param name="middleware">The middleware instance.</param>
    /// <param name="notificationType">The notification type.</param>
    /// <param name="notification">The notification instance.</param>
    /// <returns>True if the middleware is type-constrained and not compatible; otherwise, false.</returns>
    private static bool IsTypeConstrainedMiddleware(INotificationMiddleware middleware, Type notificationType, object notification)
    {
        // Use cached interface lookup to avoid repeated reflection
        var interfaces = PipelineUtilities.GetInterfacesCached(middleware.GetType());
        var genericMiddlewareInterfaces = GetConstrainedNotificationInterfaces(interfaces);
        
        if (genericMiddlewareInterfaces.Length == 0)
        {
            return false;
        }
        
        foreach (Type t in genericMiddlewareInterfaces)
        {
            var constraintType = t.GetGenericArguments()[0];
            if (constraintType.IsAssignableFrom(notificationType))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Fast helper method to get constrained notification interfaces.
    /// </summary>
    /// <param name="interfaces">The interfaces to analyze.</param>
    /// <returns>An array of constrained notification interfaces.</returns>
    private static Type[] GetConstrainedNotificationInterfaces(Type[] interfaces)
    {
        var result = new List<Type>();
        foreach (Type iface in interfaces)
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            {
                result.Add(iface);
            }
        }
        return result.ToArray();
    }

    /// <summary>
    /// Fast helper method to check compatible constraints.
    /// </summary>
    /// <param name="constrainedInterfaces">The constrained interfaces to check.</param>
    /// <param name="actualNotificationType">The actual notification type.</param>
    /// <returns>True if a compatible constraint is found; otherwise, false.</returns>
    private static bool HasCompatibleConstraint(Type[] constrainedInterfaces, Type actualNotificationType)
    {
        foreach (Type t in constrainedInterfaces)
        {
            var constraintType = t.GetGenericArguments()[0];
            if (constraintType.IsAssignableFrom(actualNotificationType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Minimal notification for concrete type creation fallback.
    /// </summary>
    private sealed class MinimalNotification : INotification { }

    #endregion

    #region Core Middleware Addition Override

    /// <summary>
    /// Core method for adding middleware to the pipeline with base class optimizations.
    /// Uses the enhanced <see cref="BasePipelineBuilder{TBuilder}"/> order calculation with context-aware caching.
    /// </summary>
    /// <param name="middlewareType">The middleware type to add.</param>
    /// <param name="configuration">Optional configuration object.</param>
    protected new void AddMiddlewareCore(Type middlewareType, object? configuration = null)
    {
        // Use base class order calculation with context-aware caching for optimal performance
        // This ensures consistent behavior across both pipeline types while maintaining performance
        base.AddMiddlewareCore(middlewareType, configuration);
    }

    #endregion
}
