using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Blazing.Mediator.Exceptions;
using System.Reflection;

namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// Builds and executes notification middleware pipelines with support for generic types, conditional execution, and type constraints.
/// </summary>
public sealed class NotificationPipelineBuilder : INotificationPipelineBuilder, INotificationMiddlewarePipelineInspector
{
    private const string OrderPropertyName = "Order";
    private readonly List<NotificationMiddlewareInfo> _middlewareInfos = [];
    private readonly MediatorLogger? _mediatorLogger;
    private ConstraintCompatibilityChecker? _constraintChecker;

    /// <summary>
    /// Information about registered notification middleware.
    /// </summary>
    /// <param name="Type">The middleware type</param>
    /// <param name="Order">The execution order</param>
    /// <param name="Configuration">Optional configuration object</param>
    private sealed record NotificationMiddlewareInfo(Type Type, int Order, object? Configuration = null);

    /// <summary>
    /// Initializes a new instance of the NotificationPipelineBuilder with optional logging.
    /// </summary>
    /// <param name="mediatorLogger">Optional MediatorLogger for enhanced constraint-based logging.</param>
    public NotificationPipelineBuilder(MediatorLogger? mediatorLogger = null)
    {
        _mediatorLogger = mediatorLogger;
    }

    /// <summary>
    /// Sets the constraint validation options for this pipeline builder.
    /// </summary>
    /// <param name="options">The constraint validation options to use.</param>
    public void SetConstraintValidationOptions(ConstraintValidationOptions? options)
    {
        _constraintChecker = options != null ? new ConstraintCompatibilityChecker(options) : null;
        _mediatorLogger?.MiddlewareConstraintValidation("NotificationPipelineBuilder", "N/A", true, 
            $"Constraint validation options updated: {options?.Strictness ?? ConstraintValidationOptions.ValidationStrictness.Disabled}", 0);
    }

    #region AddMiddleware overloads

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));

        return this;
    }

    /// <summary>
    /// Adds notification middleware with configuration to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements INotificationMiddleware.</typeparam>
    /// <param name="configuration">Optional configuration object for the middleware.</param>
    /// <returns>The pipeline builder for chaining.</returns>
    public INotificationPipelineBuilder AddMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class, INotificationMiddleware
    {
        var middlewareType = typeof(TMiddleware);
        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order, configuration));

        return this;
    }

    /// <inheritdoc />
    public INotificationPipelineBuilder AddMiddleware(Type middlewareType)
    {
        if (!typeof(INotificationMiddleware).IsAssignableFrom(middlewareType))
        {
            throw new ArgumentException($"Type {middlewareType.Name} does not implement INotificationMiddleware", nameof(middlewareType));
        }

        var order = GetMiddlewareOrder(middlewareType);
        _middlewareInfos.Add(new NotificationMiddlewareInfo(middlewareType, order));

        return this;
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

    #region ExecutePipeline - Enhanced Implementation with Comprehensive Logging

    /// <summary>
    /// Executes the notification middleware pipeline with enhanced support for generic types, 
    /// conditional execution, type constraints, and comprehensive logging.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification being processed</typeparam>
    /// <param name="notification">The notification to process</param>
    /// <param name="serviceProvider">Service provider for middleware resolution</param>
    /// <param name="finalHandler">Final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Use the actual runtime type of the notification instead of the generic parameter type
        Type actualNotificationType = notification.GetType();

        var pipelineId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Enhanced logging: Pipeline started
        _mediatorLogger?.NotificationPipelineStarted(actualNotificationType.Name, _middlewareInfos.Count, pipelineId);

        NotificationDelegate<TNotification> pipeline = finalHandler;

        // Get middleware types that can handle this notification type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        // Performance tracking for constraint checking
        var constraintCheckStopwatch = System.Diagnostics.Stopwatch.StartNew();
        int constraintChecksPerformed = 0;
        int cacheHits = 0;

        foreach (NotificationMiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;
            constraintChecksPerformed++;

            // Enhanced logging: Checking notification middleware compatibility
            _mediatorLogger?.MiddlewareConstraintCheck(middlewareType.Name, actualNotificationType.Name, false);

            // Log constraint information
            var constraintTypes = GetMiddlewareConstraintTypes(middlewareType);
            bool hasConstraints = constraintTypes.Any();
            string constraintTypeNames = hasConstraints ? string.Join(", ", constraintTypes.Select(t => t.Name)) : "";
            
            _mediatorLogger?.MiddlewareConstraintCheck(middlewareType.Name, actualNotificationType.Name, hasConstraints, constraintTypeNames);

            // Handle open generic types by making them closed generic types
            Type actualMiddlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Check if this middleware supports notification middleware patterns
                var genericParams = middlewareType.GetGenericArguments();
                switch (genericParams)
                {
                    case { Length: 1 }:
                        // Check if the generic constraints can be satisfied before attempting to create the type
                        var constraintValidationStopwatch = System.Diagnostics.Stopwatch.StartNew();
                        bool canSatisfyConstraints = CanSatisfyGenericConstraints(middlewareType, actualNotificationType);
                        constraintValidationStopwatch.Stop();

                        if (!canSatisfyConstraints)
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            string skipReason = "Generic constraints not satisfied";
                            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, actualNotificationType.Name, 
                                false, skipReason, constraintValidationStopwatch.Elapsed.TotalMilliseconds);
                            continue;
                        }

                        // Create the specific generic type for this notification
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(actualNotificationType);
                            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, actualNotificationType.Name, 
                                true, "Generic constraints satisfied", constraintValidationStopwatch.Elapsed.TotalMilliseconds);
                        }
                        catch (ArgumentException ex)
                        {
                            // Generic constraints were not satisfied, skip this middleware
                            string createReason = $"Failed to create concrete generic type: {ex.Message}";
                            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, actualNotificationType.Name, 
                                false, createReason, constraintValidationStopwatch.Elapsed.TotalMilliseconds);
                            continue;
                        }
                        break;
                    default:
                        // Unsupported number of generic parameters for notification middleware
                        string paramReason = $"Unsupported generic parameter count {genericParams.Length}";
                        _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, actualNotificationType.Name, 
                            false, paramReason, 0);
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }

            // Check if this middleware type implements INotificationMiddleware or INotificationMiddleware<T>
            bool isCompatible = typeof(INotificationMiddleware).IsAssignableFrom(actualMiddlewareType);

            if (!isCompatible)
            {
                // This middleware doesn't implement INotificationMiddleware, skip it
                string compatReason = "Does not implement INotificationMiddleware";
                _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                    false, compatReason, 0);
                continue;
            }

            // Enhanced check for type-constrained middleware - verify constraint compatibility
            if (actualMiddlewareType != middlewareType) // This is a concrete generic type
            {
                // Check if the middleware implements INotificationMiddleware<TNotification>
                var constrainedInterfaces = actualMiddlewareType.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                               i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                    .ToArray();

                if (constrainedInterfaces.Length > 0)
                {
                    // Verify that at least one constraint type is compatible with the notification type
                    bool hasCompatibleConstraint = constrainedInterfaces.Any(constrainedInterface =>
                    {
                        var constraintType = constrainedInterface.GetGenericArguments()[0];
                        bool isAssignable = constraintType.IsAssignableFrom(actualNotificationType);
                        
                        if (isAssignable)
                        {
                            _mediatorLogger?.ConstrainedMethodInvocation(constraintType.Name, actualNotificationType.Name, 
                                constraintType.Name, "ConstraintCompatibilityCheck");
                        }
                        
                        return isAssignable;
                    });

                    if (!hasCompatibleConstraint)
                    {
                        // No compatible constraint found, skip this middleware
                        var constraintNames = constrainedInterfaces.Select(i => i.GetGenericArguments()[0].Name);
                        string reason = $"No compatible constraint found. Available: [{string.Join(", ", constraintNames)}]";
                        _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                            false, reason, 0);
                        continue;
                    }
                    else
                    {
                        _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                            true, "Compatible constraint found", 0);
                    }
                }
            }
            else
            {
                // For non-generic middleware, check if it implements INotificationMiddleware<T> with constraints
                var constrainedInterfaces = actualMiddlewareType.GetInterfaces()
                    .Where(i => i.IsGenericType && 
                               i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                    .ToArray();

                if (constrainedInterfaces.Length > 0)
                {
                    // This is a concrete type that implements INotificationMiddleware<TSpecificType>
                    // Check if the constraint matches the current notification
                    bool hasCompatibleConstraint = constrainedInterfaces.Any(constrainedInterface =>
                    {
                        var constraintType = constrainedInterface.GetGenericArguments()[0];
                        return constraintType.IsAssignableFrom(actualNotificationType);
                    });

                    if (!hasCompatibleConstraint)
                    {
                        // No compatible constraint found, skip this middleware
                        var constraintNames = constrainedInterfaces.Select(i => i.GetGenericArguments()[0].Name);
                        string reason = $"Type constraints not compatible. Available: [{string.Join(", ", constraintNames)}]";
                        _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                            false, reason, 0);
                        continue;
                    }
                    else
                    {
                        _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                            true, "Type constraints compatible", 0);
                    }
                }
                else
                {
                    // No constraints - compatible with all notifications
                    _mediatorLogger?.MiddlewareConstraintValidation(actualMiddlewareType.Name, actualNotificationType.Name, 
                        true, "No constraints - universal compatibility", 0);
                }
            }

            // Get actual order - prioritize runtime resolution over cached registration order
            int actualOrder = GetActualMiddlewareOrder(middlewareInfo, actualMiddlewareType, serviceProvider);

            applicableMiddleware.Add((actualMiddlewareType, actualOrder));

            // Enhanced logging: Middleware successfully added
            _mediatorLogger?.MiddlewareExecutionDecision(actualMiddlewareType.Name, actualNotificationType.Name, true, 
                $"Compatible with notification, order: {actualOrder}", actualOrder);
        }

        constraintCheckStopwatch.Stop();

        // Log constraint checking performance
        _mediatorLogger?.ConstraintCheckingPerformance(actualNotificationType.Name, 
            constraintCheckStopwatch.Elapsed.TotalMilliseconds, constraintChecksPerformed, cacheHits);

        // Enhanced logging: Pipeline execution info
        _mediatorLogger?.NotificationPipelineStarted(actualNotificationType.Name, applicableMiddleware.Count, pipelineId);

        // Enhanced sorting: Sort middleware by order (lower numbers execute first), then by registration order
        applicableMiddleware.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;

            // If orders are equal, maintain registration order by finding original registration index
            int indexA = GetOriginalRegistrationIndex(a.Type);
            int indexB = GetOriginalRegistrationIndex(b.Type);
            return indexA.CompareTo(indexB);
        });

        // Track execution metrics
        int executedCount = 0;
        int skippedCount = _middlewareInfos.Count - applicableMiddleware.Count;

        // Build pipeline in reverse order so the first middleware in the sorted list runs first
        for (int i = applicableMiddleware.Count - 1; i >= 0; i--)
        {
            (Type middlewareType, int order) = applicableMiddleware[i];
            NotificationDelegate<TNotification> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async (notif, ct) =>
            {
                var middlewareStopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Create middleware instance using DI container
                object? middlewareInstance = serviceProvider.GetService(middlewareType);

                if (middlewareInstance == null)
                {
                    throw new InvalidOperationException(
                        $"Could not create instance of notification middleware {middlewareName}. Make sure the middleware is registered in the DI container.");
                }

                // Cast to the notification middleware interface
                if (middlewareInstance is not INotificationMiddleware middleware)
                {
                    throw new InvalidOperationException(
                        $"Middleware {middlewareName} does not implement INotificationMiddleware.");
                }

                // Check if this is conditional middleware and should execute
                if (middleware is IConditionalNotificationMiddleware conditionalMiddleware &&
                    !conditionalMiddleware.ShouldExecute(notif))
                {
                    // Skip this middleware
                    string skipReason = "Conditional middleware chose not to execute";
                    _mediatorLogger?.MiddlewareExecutionDecision(middlewareName, actualNotificationType.Name, false, skipReason, order);
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }

                // Check if this is type-constrained middleware and handle it appropriately
                if (IsTypeConstrainedMiddleware(middleware, actualNotificationType, notif))
                {
                    // Middleware doesn't match the notification type constraint, skip it
                    string skipReason = "Type constraints not compatible";
                    _mediatorLogger?.MiddlewareExecutionDecision(middlewareName, actualNotificationType.Name, false, skipReason, order);
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }

                // Determine if this middleware is constrained and get constraint info
                var constrainedInterfaces = middleware.GetType().GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                    .ToArray();
                
                bool isConstrained = constrainedInterfaces.Length > 0;
                string? constraintType = isConstrained ? constrainedInterfaces[0].GetGenericArguments()[0].Name : null;

                // Log execution start
                _mediatorLogger?.MiddlewareExecutionStarted(middlewareName, actualNotificationType.Name, order, isConstrained, constraintType);

                bool executionSuccessful = false;

                try
                {
                    // Enhanced constrained method invocation with better error handling
                    bool invokedConstrainedMethod = await TryInvokeConstrainedMethodAsync(middleware, notif, currentPipeline, ct, actualNotificationType);

                    // If we couldn't invoke the constrained method, use the general method
                    if (!invokedConstrainedMethod)
                    {
                        try
                        {
                            _mediatorLogger?.MiddlewareExecutionStarted(middlewareName, actualNotificationType.Name, order, false, null);
                            await middleware.InvokeAsync(notif, currentPipeline, ct).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // Don't wrap cancellation exceptions
                            throw;
                        }
                        catch (Exception ex)
                        {
                            // Enhance exception with middleware context
                            throw new NotificationMiddlewareException(
                                $"Exception occurred in notification middleware '{middlewareName}' while processing notification of type '{actualNotificationType.Name}'.",
                                ex,
                                middlewareName,
                                actualNotificationType,
                                notif?.GetType());
                        }
                    }

                    executionSuccessful = true;
                    executedCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    executionSuccessful = false;
                    throw;
                }
                finally
                {
                    middlewareStopwatch.Stop();
                    
                    // Log execution completion
                    _mediatorLogger?.MiddlewareExecutionCompleted(middlewareName, actualNotificationType.Name,
                        middlewareStopwatch.Elapsed.TotalMilliseconds, executionSuccessful, isConstrained);
                }
            };
        }

        // Execute the pipeline
        await pipeline(notification, cancellationToken).ConfigureAwait(false);
        
        stopwatch.Stop();
        
        // Calculate efficiency metrics
        double efficiencyPercentage = _middlewareInfos.Count > 0 
            ? (double)executedCount / _middlewareInfos.Count * 100.0 
            : 100.0;

        // Final logging: Pipeline completed
        _mediatorLogger?.NotificationPipelineCompleted(actualNotificationType.Name, pipelineId, 
            stopwatch.Elapsed.TotalMilliseconds, executedCount, skippedCount);
        _mediatorLogger?.PipelineEfficiencyMetrics(actualNotificationType.Name, _middlewareInfos.Count, 
            executedCount, skippedCount, efficiencyPercentage);
    }

    #endregion

    #region Inspector Methods

    /// <inheritdoc />
    public IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        return _middlewareInfos.Select(info => info.Type).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration()
    {
        return _middlewareInfos.Select(info => (info.Type, info.Configuration)).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null)
    {
        if (serviceProvider == null)
        {
            // Return cached registration-time order values
            return _middlewareInfos.Select(info => (info.Type, info.Order, info.Configuration)).ToList();
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

                    if (genericParams.Length == 1)
                    {
                        // Try to find types that can satisfy the middleware's type constraints
                        actualMiddlewareType = TryCreateConcreteNotificationMiddlewareType(middlewareInfo.Type);
                    }

                    if (actualMiddlewareType != null)
                    {
                        // Try to get the actual Order from instance - same as ExecutePipeline
                        actualOrder = GetActualMiddlewareOrder(middlewareInfo, actualMiddlewareType, serviceProvider);
                    }
                }
                else
                {
                    // For non-generic types, resolve directly from DI - same as ExecutePipeline
                    actualOrder = GetActualMiddlewareOrder(middlewareInfo, middlewareInfo.Type, serviceProvider);
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
            var className = GetCleanTypeName(type);
            var typeParameters = type.IsGenericType ?
                $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.Name))}>" :
                string.Empty;

            var detailed = isDetailed ?? true;

            // Extract generic constraints for notification middleware
            var genericConstraints = detailed ? GetGenericConstraints(type) : string.Empty;

            // Always discover configuration, but adjust output detail based on mode
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

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider)
    {
        return AnalyzeMiddleware(serviceProvider, true);
    }

    #endregion

    #region Enhanced Inspector Methods - Implementation of missing interface methods

    /// <inheritdoc />
    public NotificationConstraintAnalysis AnalyzeConstraints(Type notificationType, IServiceProvider serviceProvider)
    {
        var analysis = new NotificationConstraintAnalysis
        {
            NotificationType = notificationType,
            ApplicableMiddleware = new List<Type>(),
            SkippedMiddleware = new Dictionary<Type, string>(),
            TotalMiddlewareCount = _middlewareInfos.Count
        };

        foreach (var middlewareInfo in _middlewareInfos)
        {
            var middlewareType = middlewareInfo.Type;
            bool isApplicable = false;
            string skipReason = "";

            try
            {
                // Handle generic type definitions
                Type actualMiddlewareType;
                if (middlewareType.IsGenericTypeDefinition)
                {
                    if (!CanSatisfyGenericConstraints(middlewareType, notificationType))
                    {
                        skipReason = "Generic constraints not satisfied";
                    }
                    else
                    {
                        actualMiddlewareType = middlewareType.MakeGenericType(notificationType);
                        isApplicable = IsMiddlewareApplicable(actualMiddlewareType, notificationType);
                        if (!isApplicable)
                        {
                            skipReason = "Type constraints not compatible";
                        }
                    }
                }
                else
                {
                    isApplicable = IsMiddlewareApplicable(middlewareType, notificationType);
                    if (!isApplicable)
                    {
                        skipReason = "Type constraints not compatible";
                    }
                }

                if (isApplicable)
                {
                    analysis.ApplicableMiddleware.Add(middlewareType);
                }
                else
                {
                    analysis.SkippedMiddleware[middlewareType] = skipReason;
                }
            }
            catch (Exception ex)
            {
                analysis.SkippedMiddleware[middlewareType] = $"Analysis error: {ex.Message}";
            }
        }

        analysis.ExecutionEfficiency = analysis.TotalMiddlewareCount > 0 
            ? (double)analysis.ApplicableMiddleware.Count / analysis.TotalMiddlewareCount 
            : 1.0;

        return analysis;
    }

    /// <inheritdoc />
    public NotificationConstraintAnalysis AnalyzeConstraints<TNotification>(IServiceProvider serviceProvider)
        where TNotification : INotification
    {
        return AnalyzeConstraints(typeof(TNotification), serviceProvider);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Type, IReadOnlyList<Type>> GetConstraintUsageMap(IServiceProvider serviceProvider)
    {
        var constraintMap = new Dictionary<Type, List<Type>>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            var middlewareType = middlewareInfo.Type;
            var constraintTypes = GetMiddlewareConstraintTypes(middlewareType);

            foreach (var constraintType in constraintTypes)
            {
                if (!constraintMap.ContainsKey(constraintType))
                {
                    constraintMap[constraintType] = new List<Type>();
                }
                constraintMap[constraintType].Add(middlewareType);
            }
        }

        return constraintMap.ToDictionary(
            kvp => kvp.Key, 
            kvp => (IReadOnlyList<Type>)kvp.Value.AsReadOnly()
        );
    }

    /// <inheritdoc />
    public PipelineConstraintAnalysis AnalyzePipelineConstraints(IServiceProvider serviceProvider)
    {
        var constraintUsageMap = GetConstraintUsageMap(serviceProvider);
        var generalMiddleware = new List<Type>();
        var constrainedMiddleware = new List<Type>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            var middlewareType = middlewareInfo.Type;
            var constraintTypes = GetMiddlewareConstraintTypes(middlewareType);

            if (constraintTypes.Any())
            {
                constrainedMiddleware.Add(middlewareType);
            }
            else
            {
                generalMiddleware.Add(middlewareType);
            }
        }

        var recommendations = new List<string>();
        
        // Generate optimization recommendations
        if (generalMiddleware.Count > constrainedMiddleware.Count)
        {
            recommendations.Add($"Consider adding type constraints to some of the {generalMiddleware.Count} general middleware to improve performance");
        }

        if (constraintUsageMap.Count == 0)
        {
            recommendations.Add("No type constraints found. Consider using INotificationMiddleware<T> for better performance");
        }

        // Check for over-constrained scenarios
        var heavilyConstrainedTypes = constraintUsageMap.Where(kvp => kvp.Value.Count > 5).ToList();
        if (heavilyConstrainedTypes.Any())
        {
            recommendations.Add($"Consider consolidating middleware for heavily constrained types: {string.Join(", ", heavilyConstrainedTypes.Select(t => t.Key.Name))}");
        }

        return new PipelineConstraintAnalysis
        {
            TotalMiddlewareCount = _middlewareInfos.Count,
            GeneralMiddlewareCount = generalMiddleware.Count,
            ConstrainedMiddlewareCount = constrainedMiddleware.Count,
            UniqueConstraintTypes = constraintUsageMap.Count,
            ConstraintUsageMap = constraintUsageMap,
            OptimizationRecommendations = recommendations
        };
    }

    /// <inheritdoc />
    public MiddlewareExecutionPath AnalyzeExecutionPath(INotification notification, IServiceProvider serviceProvider)
    {
        var notificationType = notification.GetType();
        var executionPath = new MiddlewareExecutionPath
        {
            NotificationType = notificationType,
            TotalMiddlewareCount = _middlewareInfos.Count,
            ExecutionSteps = new List<MiddlewareExecutionStep>()
        };

        var orderedMiddleware = _middlewareInfos
            .OrderBy(m => m.Order)
            .ToList();

        int executingCount = 0;
        int skippingCount = 0;
        double totalEstimatedDuration = 0;

        foreach (var middlewareInfo in orderedMiddleware)
        {
            var middlewareType = middlewareInfo.Type;
            bool willExecute = false;
            string reason = "";
            double estimatedDuration = 0.5; // Default estimate in ms

            try
            {
                // Check if middleware will execute for this notification
                if (middlewareType.IsGenericTypeDefinition)
                {
                    if (CanSatisfyGenericConstraints(middlewareType, notificationType))
                    {
                        var concreteType = middlewareType.MakeGenericType(notificationType);
                        willExecute = IsMiddlewareApplicable(concreteType, notificationType);
                        reason = willExecute ? "Generic constraints satisfied" : "Type constraints not compatible";
                    }
                    else
                    {
                        willExecute = false;
                        reason = "Generic constraints not satisfied";
                    }
                }
                else
                {
                    willExecute = IsMiddlewareApplicable(middlewareType, notificationType);
                    reason = willExecute ? "Type constraints compatible" : "Type constraints not compatible";
                }

                // Check if it's conditional middleware
                if (willExecute)
                {
                    try
                    {
                        var instance = serviceProvider.GetService(middlewareType);
                        if (instance is IConditionalNotificationMiddleware conditionalMiddleware &&
                            !conditionalMiddleware.ShouldExecute(notification))
                        {
                            willExecute = false;
                            reason = "Conditional middleware chose not to execute";
                        }
                        
                        // Estimate duration based on middleware complexity
                        if (instance != null)
                        {
                            var methods = instance.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                            estimatedDuration = Math.Max(0.1, methods.Length * 0.1); // Rough complexity estimate
                        }
                    }
                    catch
                    {
                        // If we can't resolve, assume it will execute
                        reason = willExecute ? "Assumed to execute (cannot resolve instance)" : reason;
                    }
                }

                if (willExecute)
                {
                    executingCount++;
                    totalEstimatedDuration += estimatedDuration;
                }
                else
                {
                    skippingCount++;
                    estimatedDuration = 0; // No time for skipped middleware
                }
            }
            catch (Exception ex)
            {
                willExecute = false;
                reason = $"Analysis error: {ex.Message}";
                skippingCount++;
            }

            executionPath.ExecutionSteps.Add(new MiddlewareExecutionStep
            {
                MiddlewareType = middlewareType,
                Order = middlewareInfo.Order,
                WillExecute = willExecute,
                Reason = reason,
                EstimatedDurationMs = estimatedDuration
            });
        }

        executionPath.ExecutingMiddlewareCount = executingCount;
        executionPath.SkippingMiddlewareCount = skippingCount;
        executionPath.EstimatedTotalDurationMs = totalEstimatedDuration;

        return executionPath;
    }

    #endregion

    #region Enhanced Helper Methods
    
    /// <summary>
    /// Enhanced method to try invoking type-constrained InvokeAsync method with better error handling and logging.
    /// Now uses the advanced constraint compatibility checker for better validation.
    /// </summary>
    private async Task<bool> TryInvokeConstrainedMethodAsync<TNotification>(
        INotificationMiddleware middleware, 
        TNotification notification, 
        NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken,
        Type actualNotificationType)
        where TNotification : INotification
    {
        string middlewareName = middleware.GetType().Name;

        // Use enhanced constraint checker if available
        if (_constraintChecker != null)
        {
            var middlewareType = middleware.GetType();
            if (!_constraintChecker.IsCompatible(middlewareType, actualNotificationType))
            {
                _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, actualNotificationType.Name,
                    false, "Constraint checker determined incompatibility", 0);
                return false; // Not compatible, skip this middleware
            }
        }

        // Check if middleware implements INotificationMiddleware<T> for direct invocation
        var constrainedInterfaces = middleware.GetType().GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();

        if (constrainedInterfaces.Length == 0)
        {
            return false; // No constrained interfaces found
        }

        // Try to invoke the type-constrained InvokeAsync method
        foreach (var constrainedInterface in constrainedInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            
            // Verify the notification type matches the constraint using enhanced checking
            bool isCompatible = _constraintChecker?.IsCompatible(middleware.GetType(), actualNotificationType) 
                ?? constraintType.IsAssignableFrom(actualNotificationType);

            if (isCompatible)
            {
                // Get the constrained InvokeAsync method
                var constrainedMethod = constrainedInterface.GetMethod("InvokeAsync");
                if (constrainedMethod != null)
                {
                    try
                    {
                        _mediatorLogger?.ConstrainedMethodInvocation(middlewareName, actualNotificationType.Name, 
                            constraintType.Name, "InvokeAsync");

                        // Create the constrained delegate
                        var delegateType = typeof(NotificationDelegate<>).MakeGenericType(constraintType);
                        var constrainedNext = Delegate.CreateDelegate(delegateType, next.Target, next.Method);
                        
                        // Invoke the constrained method
                        var task = (Task?)constrainedMethod.Invoke(middleware, [notification, constrainedNext, cancellationToken]);
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            return true; // Successfully invoked constrained method
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but fall back to the general method
                        string reason = $"Exception during constrained method invocation: {ex.Message}";
                        _mediatorLogger?.ConstrainedMethodFallback(middlewareName, actualNotificationType.Name, reason);
                        break;
                    }
                }
                else
                {
                    string reason = "Constrained InvokeAsync method not found";
                    _mediatorLogger?.ConstrainedMethodFallback(middlewareName, actualNotificationType.Name, reason);
                }
            }
        }

        return false; // Could not invoke constrained method
    }

    private bool IsMiddlewareApplicable(Type middlewareType, Type notificationType)
    {
        // Use enhanced constraint checker if available
        if (_constraintChecker != null)
        {
            var constraintCheckStopwatch = System.Diagnostics.Stopwatch.StartNew();
            bool isCompatible = _constraintChecker.IsCompatible(middlewareType, notificationType);
            constraintCheckStopwatch.Stop();

            // Log cache usage if constraint checker supports it
            string cacheKey = $"{middlewareType.Name}+{notificationType.Name}";
            
            // Enhanced logging for constraint checker results
            _mediatorLogger?.ConstraintValidationCache(middlewareType.Name, notificationType.Name, false, cacheKey);
            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, notificationType.Name, 
                isCompatible, isCompatible ? "Constraint checker validated compatibility" : "Constraint checker found incompatibility", 
                constraintCheckStopwatch.Elapsed.TotalMilliseconds);

            return isCompatible;
        }

        // Fallback to basic checking
        return IsMiddlewareApplicableBasic(middlewareType, notificationType);
    }

    private bool IsMiddlewareApplicableBasic(Type middlewareType, Type notificationType)
    {
        // Check if middleware implements INotificationMiddleware
        if (!typeof(INotificationMiddleware).IsAssignableFrom(middlewareType))
        {
            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, notificationType.Name, 
                false, "Does not implement INotificationMiddleware", 0);
            return false;
        }

        // Check for type constraints
        var constrainedInterfaces = middlewareType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();

        if (!constrainedInterfaces.Any())
        {
            _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, notificationType.Name, 
                true, "No constraints - universal compatibility", 0);
            return true; // No constraints - applicable to all notifications
        }

        // Check if any constraint is compatible
        bool hasCompatibleConstraint = constrainedInterfaces.Any(constrainedInterface =>
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            bool isAssignable = constraintType.IsAssignableFrom(notificationType);
            
            if (isAssignable)
            {
                _mediatorLogger?.ConstrainedMethodInvocation(middlewareType.Name, notificationType.Name, 
                    constraintType.Name, "ConstraintCheck");
            }
            
            return isAssignable;
        });

        string reason = hasCompatibleConstraint 
            ? "Compatible constraint found" 
            : $"No compatible constraints. Available: [{string.Join(", ", constrainedInterfaces.Select(i => i.GetGenericArguments()[0].Name))}]";

        _mediatorLogger?.MiddlewareConstraintValidation(middlewareType.Name, notificationType.Name, 
            hasCompatibleConstraint, reason, 0);

        return hasCompatibleConstraint;
    }

    private List<Type> GetMiddlewareConstraintTypes(Type middlewareType)
    {
        var constraintTypes = new List<Type>();

        // Handle generic type definitions
        if (middlewareType.IsGenericTypeDefinition)
        {
            var genericParams = middlewareType.GetGenericArguments();
            foreach (var param in genericParams)
            {
                var constraints = param.GetGenericParameterConstraints();
                constraintTypes.AddRange(constraints.Where(c => typeof(INotification).IsAssignableFrom(c)));
            }
        }
        else
        {
            // Handle concrete types
            var constrainedInterfaces = middlewareType.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                .ToArray();

            constraintTypes.AddRange(constrainedInterfaces.Select(i => i.GetGenericArguments()[0]));
        }

        return constraintTypes.Distinct().ToList();
    }

    // Enhanced CanSatisfyGenericConstraints method with better constraint checking
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

            // Enhanced constraint validation with better error handling
            if (!ValidateGenericParameterConstraints(parameter, argument))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateGenericParameterConstraints(Type parameter, Type argument)
    {
        try
        {
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

            // Enhanced type constraints checking with better compatibility validation
            var constraints = parameter.GetGenericParameterConstraints();
            foreach (var constraint in constraints)
            {
                if (!IsConstraintSatisfiedByArgument(constraint, argument))
                {
                    return false;
                }
            }
        }
        catch (Exception)
        {
            // For complex constraint scenarios, let runtime handle validation
            // to avoid breaking existing functionality
            return true;
        }

        return true;
    }

    private static bool IsConstraintSatisfiedByArgument(Type constraint, Type argument)
    {
        try
        {
            if (constraint.IsInterface)
            {
                // Interface constraint - check if argument implements the interface
                return constraint.IsAssignableFrom(argument);
            }
            else if (constraint.IsClass)
            {
                // Class constraint - check if argument derives from or is the class
                return constraint.IsAssignableFrom(argument);
            }
            else if (constraint.IsGenericType)
            {
                // Generic constraint - handle generic type constraints more carefully
                return constraint.IsAssignableFrom(argument);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Original Helper Methods - These were accidentally removed

    /// <summary>
    /// Determines the order for a notification middleware type with backward compatibility.
    /// Maintains the original NotificationPipelineBuilder behavior for unordered middleware (1, 2, 3...).
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

        // Try to get order from instance Order property (for INotificationMiddleware implementations)
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

        // Fallback: middleware has no explicit order, assign simple incremental values starting from 1
        // This maintains backward compatibility with existing tests and expected behavior
        // Count how many unordered middleware we already have to maintain discovery order
        int unorderedCount = _middlewareInfos.Count(m => m.Order >= 1 && m.Order < 100); // Count middleware in the 1-99 range (unordered)

        // Assign simple incremental order: 1, 2, 3, etc. (backward compatible behavior)
        return unorderedCount + 1;
    }

    /// <summary>
    /// Gets the actual middleware order at runtime, prioritizing instance resolution over cached values.
    /// Supports both generic and non-generic middleware consistently.
    /// </summary>
    private int GetActualMiddlewareOrder(NotificationMiddlewareInfo middlewareInfo, Type actualMiddlewareType, IServiceProvider serviceProvider)
    {
        int actualOrder = middlewareInfo.Order; // Start with cached order

        try
        {
            var instance = serviceProvider.GetService(actualMiddlewareType);
            if (instance != null)
            {
                var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                {
                    actualOrder = (int)orderProperty.GetValue(instance)!;
                    // Use _mediatorLogger for enhanced logging instead of basic _logger
                    _mediatorLogger?.MiddlewareExecutionDecision(actualMiddlewareType.Name, "N/A", true, 
                        $"Resolved runtime order {actualOrder}", actualOrder);
                }
            }
        }
        catch (Exception ex)
        {
            // If we can't get instance, use cached order and log the issue with enhanced logging
            _mediatorLogger?.ConstrainedMethodFallback(actualMiddlewareType.Name, "N/A", 
                $"Failed to resolve runtime order: {ex.Message}. Using cached order {middlewareInfo.Order}");
        }

        return actualOrder;
    }

    /// <summary>
    /// Gets the original registration index for a middleware type, supporting both generic definitions and concrete types.
    /// </summary>
    private int GetOriginalRegistrationIndex(Type middlewareType)
    {
        return _middlewareInfos.FindIndex(info =>
            info.Type == middlewareType ||
            (info.Type.IsGenericTypeDefinition && middlewareType.IsGenericType && 
             info.Type == middlewareType.GetGenericTypeDefinition()));
    }

    /// <summary>
    /// Enhanced method to try invoking type-constrained InvokeAsync method with better error handling and logging.
    /// </summary>
    private async Task<bool> TryInvokeConstrainedMethod<TNotification>(
        INotificationMiddleware middleware, 
        TNotification notification, 
        NotificationDelegate<TNotification> next, 
        CancellationToken cancellationToken,
        Type actualNotificationType)
        where TNotification : INotification
    {
        // Check if middleware implements INotificationMiddleware<T> for direct invocation
        var constrainedInterfaces = middleware.GetType().GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();

        if (constrainedInterfaces.Length == 0)
        {
            return false; // No constrained interfaces found
        }

        // Try to invoke the type-constrained InvokeAsync method
        foreach (var constrainedInterface in constrainedInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            
            // Verify the notification type matches the constraint
            if (constraintType.IsAssignableFrom(actualNotificationType))
            {
                // Get the constrained InvokeAsync method
                var constrainedMethod = constrainedInterface.GetMethod("InvokeAsync");
                if (constrainedMethod != null)
                {
                    try
                    {
                        // Create the constrained delegate
                        var delegateType = typeof(NotificationDelegate<>).MakeGenericType(constraintType);
                        var constrainedNext = Delegate.CreateDelegate(delegateType, next.Target, next.Method);
                        
                        // Invoke the constrained method
                        var task = (Task?)constrainedMethod.Invoke(middleware, [notification, constrainedNext, cancellationToken]);
                        if (task != null)
                        {
                            await task.ConfigureAwait(false);
                            return true; // Successfully invoked constrained method
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but fall back to the general method
                        string reason = $"Exception during constrained method invocation: {ex.Message}";
                        _mediatorLogger?.ConstrainedMethodFallback(middleware.GetType().Name, actualNotificationType.Name, reason);
                        break;
                    }
                }
            }
        }

        return false; // Could not invoke constrained method
    }

    /// <summary>
    /// Attempts to create a concrete notification middleware type from a generic type definition by finding suitable type arguments.
    /// Uses assembly scanning to find types that naturally satisfy the middleware's type constraints.
    /// </summary>
    private static Type? TryCreateConcreteNotificationMiddlewareType(Type middlewareTypeDefinition)
    {
        if (!middlewareTypeDefinition.IsGenericTypeDefinition)
            return middlewareTypeDefinition;

        var genericParams = middlewareTypeDefinition.GetGenericArguments();
        if (genericParams.Length != 1)
            return null;

        // Get all notification types from loaded assemblies, prioritizing test assemblies
        var candidateTypes = new List<Type>();
        
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .OrderBy(a =>
                {
                    var name = a.FullName ?? "";
                    // Prioritize test assemblies and user assemblies over system assemblies
                    if (name.Contains("Test")) return 0;
                    if (!name.StartsWith("System") && !name.StartsWith("Microsoft")) return 1;
                    return 2;
                })
                .ToArray();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition && t.IsPublic)
                        .Where(t => typeof(INotification).IsAssignableFrom(t))
                        .Where(t => t.GetConstructor(Type.EmptyTypes) != null); // Must have parameterless constructor

                    candidateTypes.AddRange(types);
                }
                catch
                {
                    // Skip assemblies that can't be inspected
                    continue;
                }
            }
        }
        catch
        {
            // If assembly scanning fails, fall back to simple types
            candidateTypes.AddRange([typeof(MinimalNotification)]);
        }

        // Try different notification types as type arguments
        foreach (var notificationType in candidateTypes)
        {
            if (TryMakeGenericType(middlewareTypeDefinition, [notificationType], out var concreteType))
            {
                return concreteType;
            }
        }

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
        catch (ArgumentException)
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
    /// Checks if middleware is type-constrained and whether it should execute for the given notification type.
    /// Enhanced version with better logging and constraint checking.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="notificationType">The notification type being processed</param>
    /// <param name="notification">The notification instance</param>
    /// <returns>True if the middleware should be skipped due to type constraints, false otherwise</returns>
    private static bool IsTypeConstrainedMiddleware(INotificationMiddleware middleware, Type notificationType, object notification)
    {
        var middlewareType = middleware.GetType();
        
        // Check if middleware implements INotificationMiddleware<TNotification>
        var genericMiddlewareInterfaces = middlewareType.GetInterfaces()
            .Where(i => i.IsGenericType && 
                       i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
            .ToArray();

        if (genericMiddlewareInterfaces.Length == 0)
        {
            // Not type-constrained - should execute for all notifications
            return false;
        }

        // Check if any of the type constraints match the current notification type
        foreach (var constrainedInterface in genericMiddlewareInterfaces)
        {
            var constraintType = constrainedInterface.GetGenericArguments()[0];
            
            // If the notification type matches or implements the constraint type, middleware should execute
            if (constraintType.IsAssignableFrom(notificationType))
            {
                return false; // Should NOT skip this middleware
            }
        }
        
        // None of the constraints match - skip this middleware
        return true;
    }

    /// <summary>
    /// Extracts generic constraints from a notification middleware type for display purposes.
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
    /// Gets the clean type name without generic backtick notation.
    /// </summary>
    /// <param name="type">The type to get the clean name for.</param>
    /// <returns>The clean type name without backtick notation (e.g., "ErrorHandlingMiddleware" instead of "ErrorHandlingMiddleware`1").</returns>
    private static string GetCleanTypeName(Type type)
    {
        var typeName = type.Name;
        var backtickIndex = typeName.IndexOf('`');
        return backtickIndex > 0 ? typeName[..backtickIndex] : typeName;
    }

    #endregion

    #region Placeholder Types for Constraint Satisfaction

    /// <summary>
    /// Minimal notification implementation that satisfies basic INotification constraints for middleware inspection.
    /// </summary>
    private sealed class MinimalNotification : INotification { }

    #endregion
}
