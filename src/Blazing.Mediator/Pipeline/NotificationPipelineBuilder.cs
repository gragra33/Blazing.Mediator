namespace Blazing.Mediator.Pipeline;

/// <summary>
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
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

    /// <summary>
    /// Determines the order for a notification middleware type, using next available order after highest explicit order as fallback.
    /// Orders range from int.MinValue to int.MaxValue. Middleware without explicit order are assigned incrementally after the highest explicit order.
    /// </summary>
    private int GetMiddlewareOrder(Type middlewareType)
    {
        // Try to get order from a static Order property or field first
        var staticOrder = GetStaticOrder(middlewareType);
        if (staticOrder.HasValue)
        {
            return staticOrder.Value;
        }

        // Check for OrderAttribute if it exists (common pattern)
        var attributeOrder = GetOrderFromAttribute(middlewareType);
        if (attributeOrder.HasValue)
        {
            return attributeOrder.Value;
        }

        // Try to get order from instance Order property
        var instanceOrder = GetInstanceOrder(middlewareType);
        if (instanceOrder.HasValue)
        {
            return instanceOrder.Value;
        }

        // Fallback: assign order after the highest explicitly set order
        return GetNextAvailableOrder();
    }

    private static int? GetStaticOrder(Type middlewareType)
    {
        var orderProperty = middlewareType.GetProperty(OrderPropertyName, BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            return (int)orderProperty.GetValue(null)!;
        }

        var orderField = middlewareType.GetField(OrderPropertyName, BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            return (int)orderField.GetValue(null)!;
        }

        return null;
    }

    private static int? GetOrderFromAttribute(Type middlewareType)
    {
        var orderAttribute = middlewareType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute == null)
        {
            return null;
        }
        var orderProp = orderAttribute.GetType().GetProperty(OrderPropertyName);
        if (orderProp != null && orderProp.PropertyType == typeof(int))
        {
            return (int)orderProp.GetValue(orderAttribute)!;
        }

        return null;
    }

    private static int? GetInstanceOrder(Type middlewareType)
    {
        var instanceOrderProperty = middlewareType.GetProperty(OrderPropertyName, BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty == null || instanceOrderProperty.PropertyType != typeof(int) ||
            (instanceOrderProperty.GetGetMethod()!.IsVirtual &&
             instanceOrderProperty.DeclaringType == typeof(INotificationMiddleware)))
        {
            return null;
        }

        try
        {
            // Handle generic type definitions by checking constraints first
            Type typeToInstantiate = middlewareType;
            if (middlewareType.IsGenericTypeDefinition)
            {
                // Check if we can satisfy generic constraints for notification middleware
                // Use a minimal notification type for constraint checking
                if (!CanSatisfyGenericConstraints(middlewareType, typeof(MinimalNotification)))
                {
                    return null; // Cannot satisfy constraints
                }

                try
                {
                    typeToInstantiate = middlewareType.MakeGenericType(typeof(MinimalNotification));
                }
                catch (ArgumentException)
                {
                    return null; // Constraints not satisfied
                }
            }

            // Create a temporary instance to get the Order value
            object? instance = Activator.CreateInstance(typeToInstantiate);
            if (instance != null)
            {
                int orderValue = (int)instanceOrderProperty.GetValue(instance)!;
                // Only use non-default values as explicit orders
                if (orderValue != 0)
                {
                    return orderValue;
                }
            }
        }
        catch
        {
            // If we can't create an instance, fall through to fallback logic
        }

        return null;
    }

    private int GetNextAvailableOrder()
    {
        // If no middleware registered yet, start from 1
        if (_middlewareInfos.Count == 0)
        {
            return 1;
        }

        // Find the highest explicit order and add 1
        var highestExplicitOrder = _middlewareInfos.Max(m => m.Order);

        // Ensure we don't exceed the maximum allowed order
        var nextOrder = highestExplicitOrder + 1;

        return Math.Min(nextOrder, int.MaxValue);
    }

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

        // Use service provider to get actual runtime order values using the same logic as ExecutePipeline
        var result = new List<(Type Type, int Order, object? Configuration)>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            int actualOrder = middlewareInfo.Order; // Start with cached order

            try
            {
                // For notification middleware, directly resolve from DI to get actual runtime order
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
            catch
            {
                // If we can't get instance, use cached order - same as ExecutePipeline
            }

            result.Add((middlewareInfo.Type, actualOrder, middlewareInfo.Configuration));
        }

        return result;
    }

    /// <inheritdoc />
    public NotificationDelegate<TNotification> Build<TNotification>(
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

    /// <inheritdoc />
    public async Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Debug logging: Notification pipeline execution started
        _logger?.NotificationMiddlewarePipelineStarted(typeof(TNotification).Name, _middlewareInfos.Count);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var pipeline = Build(serviceProvider, finalHandler);
        await pipeline(notification, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        // Debug logging: Notification pipeline execution completed
        _logger?.NotificationMiddlewarePipelineCompleted(typeof(TNotification).Name, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <inheritdoc />
    public IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider)
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

            // Extract generic constraints for notification middleware
            var genericConstraints = GetGenericConstraints(type);

            analysisResults.Add(new MiddlewareAnalysis(
                Type: type,
                Order: order,
                OrderDisplay: orderDisplay,
                ClassName: className,
                TypeParameters: typeParameters,
                GenericConstraints: genericConstraints,
                Configuration: configuration
            ));
        }

        return analysisResults;
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
            if (parameterConstraints.Count <= 0)
            {
                continue;
            }

            var constraintText = $"where {parameter.Name} : {string.Join(", ", parameterConstraints)}";
            constraintParts.Add(constraintText);
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
        {
            return false;
        }

        var genericParameters = genericTypeDefinition.GetGenericArguments();

        if (genericParameters.Length != typeArguments.Length)
        {
            return false;
        }

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
                        return false;
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
    /// Minimal notification implementation that satisfies basic INotification constraints for middleware inspection.
    /// </summary>
    private sealed class MinimalNotification : INotification { }
}
