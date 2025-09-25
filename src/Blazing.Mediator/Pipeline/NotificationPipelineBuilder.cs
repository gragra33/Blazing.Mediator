namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// Builds and executes notification middleware pipelines with support for generic types, conditional execution, and type constraints.
/// </summary>
public sealed class NotificationPipelineBuilder : INotificationPipelineBuilder, INotificationMiddlewarePipelineInspector
{
    private const string OrderPropertyName = "Order";
    private readonly List<NotificationMiddlewareInfo> _middlewareInfos = [];
    private readonly ILogger<NotificationPipelineBuilder>? _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationPipelineBuilder with optional logging.
    /// </summary>
    /// <param name="logger">Optional logger for debug-level logging of pipeline operations.</param>
    public NotificationPipelineBuilder(ILogger<NotificationPipelineBuilder>? logger = null)
    {
        _logger = logger;
    }

    private sealed record NotificationMiddlewareInfo(Type Type, int Order, object? Configuration = null);

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

    #region ExecutePipeline - Enhanced Implementation

    /// <summary>
    /// Executes the notification middleware pipeline with enhanced support for generic types, 
    /// conditional execution, and type constraints.
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

        NotificationDelegate<TNotification> pipeline = finalHandler;

        // Get middleware types that can handle this notification type, sorted by order
        List<(Type Type, int Order)> applicableMiddleware = [];

        foreach (NotificationMiddlewareInfo middlewareInfo in _middlewareInfos)
        {
            Type middlewareType = middlewareInfo.Type;

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
                        if (!CanSatisfyGenericConstraints(middlewareType, actualNotificationType))
                        {
                            // Type constraints cannot be satisfied, skip this middleware
                            continue;
                        }

                        // Create the specific generic type for this notification
                        try
                        {
                            actualMiddlewareType = middlewareType.MakeGenericType(actualNotificationType);
                        }
                        catch (ArgumentException)
                        {
                            // Generic constraints were not satisfied, skip this middleware
                            continue;
                        }
                        break;
                    default:
                        // Unsupported number of generic parameters for notification middleware
                        continue;
                }
            }
            else
            {
                actualMiddlewareType = middlewareType;
            }

            // Check if this middleware type implements INotificationMiddleware
            bool isCompatible = typeof(INotificationMiddleware).IsAssignableFrom(actualMiddlewareType);

            if (!isCompatible)
            {
                // This middleware doesn't handle this notification type, skip it
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
            NotificationDelegate<TNotification> currentPipeline = pipeline;
            string middlewareName = middlewareType.Name;

            pipeline = async (notif, ct) =>
            {
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
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }

                // Check if this is type-constrained middleware
                if (IsTypeConstrainedMiddleware(middleware, actualNotificationType, notif))
                {
                    await currentPipeline(notif, ct).ConfigureAwait(false);
                    return;
                }
                
                try
                {
                    await middleware.InvokeAsync(notif, currentPipeline, ct).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw;
                }
            };
        }

        // Execute the pipeline
        await pipeline(notification, cancellationToken).ConfigureAwait(false);
        
        stopwatch.Stop();
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
                        // Try to find types that can satisfy the middleware constraints
                        actualMiddlewareType = TryCreateConcreteNotificationMiddlewareType(middlewareInfo.Type, serviceProvider);
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

    #region Helper Methods

    /// <summary>
    /// Determines the order for a notification middleware type. Middleware with explicit Order property use that value.
    /// Middleware without explicit order are assigned incrementally starting from 1, 2, 3, etc.
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

        // Fallback: middleware has no explicit order, assign it simple incremental values starting from 1
        // Count how many unordered middleware we already have to maintain discovery order
        int unorderedCount = _middlewareInfos.Count(m => m.Order >= 1 && m.Order < 100); // Count middleware in the 1-99 range (unordered)

        // Assign simple incremental order: 1, 2, 3, etc.
        return unorderedCount + 1;
    }

    /// <summary>
    /// Attempts to create a concrete notification middleware type from a generic type definition by finding suitable type arguments.
    /// Uses assembly scanning to find types that naturally satisfy the middleware's type constraints.
    /// </summary>
    private static Type? TryCreateConcreteNotificationMiddlewareType(Type middlewareTypeDefinition, IServiceProvider serviceProvider)
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
    /// Checks if middleware is type-constrained and whether it should execute for the given notification type.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="notificationType">The notification type being processed</param>
    /// <param name="notification">The notification instance</param>
    /// <returns>True if the middleware should be skipped due to type constraints, false otherwise</returns>
    private static bool IsTypeConstrainedMiddleware(INotificationMiddleware middleware, Type notificationType, object notification)
    {
        // Check if middleware implements any type-specific interfaces that would constrain it
        var middlewareType = middleware.GetType();
        
        // Look for generic interfaces that might indicate type constraints
        var interfaces = middlewareType.GetInterfaces();
        
        foreach (var @interface in interfaces)
        {
            if (@interface.IsGenericType)
            {
                var genericTypeDefinition = @interface.GetGenericTypeDefinition();
                var typeArguments = @interface.GetGenericArguments();
                
                // Check for notification-specific constraints
                if (typeArguments.Length == 1 && typeof(INotification).IsAssignableFrom(typeArguments[0]))
                {
                    var constrainedNotificationType = typeArguments[0];
                    
                    // If the middleware is constrained to a specific notification type,
                    // and the current notification doesn't match, skip it
                    if (!constrainedNotificationType.IsAssignableFrom(notificationType))
                    {
                        return true; // Should skip this middleware
                    }
                }
            }
        }
        
        return false; // Should not skip this middleware
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
