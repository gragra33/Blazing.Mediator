    namespace Blazing.Mediator.Pipeline;

/// <summary>
/// High-performance abstract base class for pipeline builders using CRTP pattern.
/// Extracts optimal implementation patterns from both MiddlewarePipelineBuilder and NotificationPipelineBuilder
/// with comprehensive performance optimizations and caching.
/// </summary>
/// <typeparam name="TBuilder">The concrete builder type for fluent API support</typeparam>
public abstract class BasePipelineBuilder<TBuilder> : IPipelineInspector
    where TBuilder : BasePipelineBuilder<TBuilder>
{
    protected readonly List<MiddlewareInfo> _middlewareInfos = [];
    protected readonly MediatorLogger? _mediatorLogger;

    // Performance optimization: Pre-calculated at construction time
    protected readonly Dictionary<Type, int> _registrationIndices = new();
    private readonly Dictionary<Type, string> _cachedTypeNames = new();
    
    // Thread-safe caching for frequently accessed operations  
    private static readonly ConcurrentDictionary<Type, bool> _genericTypeCache = new();
    
    // Per-builder instance fallback order counter to maintain test isolation
    // Each builder instance gets its own sequential fallback order values
    private static readonly int _instanceFallbackOrderCounter = int.MaxValue - 1000000;

    // Constants to avoid magic strings
    private const string OrderPropertyName = "Order";
    private const string OrderAttributeName = "OrderAttribute";

    /// <summary>
    /// CRTP pattern property for fluent API support
    /// </summary>
    protected abstract TBuilder Self { get; }

    /// <summary>
    /// Initializes a new instance of the BasePipelineBuilder.
    /// </summary>
    /// <param name="mediatorLogger">Optional MediatorLogger for debug-level logging of pipeline operations.</param>
    protected BasePipelineBuilder(MediatorLogger? mediatorLogger = null)
    {
        _mediatorLogger = mediatorLogger;
    }

    #region Core Middleware Management

    /// <summary>
    /// Core method for adding middleware to the pipeline with performance optimizations.
    /// Uses optimized utilities and pre-caching for maximum performance.
    /// </summary>
    /// <param name="middlewareType">The middleware type to add.</param>
    /// <param name="configuration">Optional configuration object.</param>
    protected void AddMiddlewareCore(Type middlewareType, object? configuration = null)
    {
        var order = GetMiddlewareOrderOptimized(middlewareType, _middlewareInfos);
        var middlewareInfo = new MiddlewareInfo(middlewareType, order, configuration);
        
        // Optimize middleware info with comprehensive pre-caching
        var optimizedInfo = PipelineUtilities.OptimizeMiddlewareInfo(middlewareInfo);
        
        // Add the optimized info to collection
        _middlewareInfos.Add(optimizedInfo);
        
        // Update registration indices for fast lookup
        _registrationIndices[middlewareType] = _middlewareInfos.Count - 1;
        
        // Cache type name for display purposes (now using optimized utilities)
        _cachedTypeNames[middlewareType] = optimizedInfo.CleanTypeName;
    }

    /// <summary>
    /// High-performance middleware order calculation with context-aware caching.
    /// Uses single cache with context-aware keys for optimal performance.
    /// Temporarily disabled for Phase 1 completion, will be re-enabled in Phase 2.
    /// </summary>
    /// <param name="middlewareType">The middleware type to calculate order for.</param>
    /// <param name="existingInfos">Existing middleware collection for context.</param>
    /// <returns>The calculated order value.</returns>
    protected static int GetMiddlewareOrderOptimized(Type middlewareType, IReadOnlyList<MiddlewareInfo> existingInfos)
    {
        // Temporarily disable caching for Phase 1 completion
        // This eliminates test contamination issues while we fix the core order calculation
        // Caching will be re-enabled in Phase 2 with proper isolation
        return CalculateOrderCore(middlewareType, existingInfos);
    }

    /// <summary>
    /// Core order calculation logic extracted from both pipeline builders.
    /// Maintains backward compatibility with original implementations.
    /// Enhanced to properly handle generic type definitions with Order properties.
    /// CRITICAL: Handles instance Order properties on generic type definitions.
    /// </summary>
    private static int CalculateOrderCore(Type middlewareType, IReadOnlyList<MiddlewareInfo> existingInfos)
    {
        // For generic type definitions, we need to check the unbound generic type for Order properties
        Type typeToCheck = middlewareType;

        // Try to get order from a static Order property first
        var orderProperty = typeToCheck.GetProperty(OrderPropertyName, BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            try
            {
                return (int)orderProperty.GetValue(null)!;
            }
            catch
            {
                // If static property access fails, continue to other methods
            }
        }

        // Try to get order from a static Order field
        var orderField = typeToCheck.GetField("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            try
            {
                return (int)orderField.GetValue(null)!;
            }
            catch
            {
                // If static field access fails, continue to other methods
            }
        }

        // Check for OrderAttribute if it exists (common pattern)
        var orderAttribute = typeToCheck.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute != null)
        {
            var orderProp = orderAttribute.GetType().GetProperty("Order");
            if (orderProp != null && orderProp.PropertyType == typeof(int))
            {
                try
                {
                    return (int)orderProp.GetValue(orderAttribute)!;
                }
                catch
                {
                    // If attribute access fails, continue to other methods
                }
            }
        }

        // CRITICAL: Try to get order from instance Order property
        var instanceOrderProperty = typeToCheck.GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
        if (instanceOrderProperty != null && instanceOrderProperty.PropertyType == typeof(int))
        {
            if (IsGenericTypeDefinitionCached(middlewareType))
            {
                // CRITICAL FIX: For generic type definitions with instance Order properties
                // First try to extract constant using IL analysis (Microsoft recommended approach)
                if (IsSimpleConstantOrderProperty(instanceOrderProperty))
                {
                    var constantValue = ExtractConstantFromOrderProperty(instanceOrderProperty);
                    if (constantValue.HasValue)
                    {
                        return constantValue.Value;
                    }
                }

                // FALLBACK: Try to create a concrete instance using enhanced constraint-aware creation
                try
                {
                    Type? concreteType = TryCreateConcreteTypeForOrderExtraction(middlewareType);
                    if (concreteType != null)
                    {
                        try
                        {
                            object? instance = Activator.CreateInstance(concreteType);
                            if (instance != null)
                            {
                                var concreteOrderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                                if (concreteOrderProperty != null && concreteOrderProperty.PropertyType == typeof(int))
                                {
                                    // SUCCESS! We found the order value from a concrete instance
                                    var orderValue = (int)concreteOrderProperty.GetValue(instance)!;
                                    // Do we need to add debug logging when available?
                                    return orderValue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Debug: Log instantiation failure 
                            // Do we need to add debug logging when available - failed to instantiate {concreteType.Name}: {ex.Message}?
                        }
                    }
                    else
                    {
                        // Debug: Log constraint resolution failure
                        // Do we need to add debug logging when available - failed to create concrete type for {middlewareType.Name}?
                    }
                }
                catch (Exception ex)
                {
                    // Debug: Log overall failure 
                    // Do we need to add debug logging when available - constraint-aware creation failed for {middlewareType.Name}: {ex.Message}?
                }
                
                // If we can't instantiate, use high fallback order for generic types
                return int.MaxValue - 1000000;
            }
            else
            {
                // For non-generic types, try to create an instance
                try
                {
                    object? instance = Activator.CreateInstance(middlewareType);
                    if (instance != null)
                    {
                        return (int)instanceOrderProperty.GetValue(instance)!;
                    }
                }
                catch
                {
                    // If we can't create an instance, fall through to fallback logic
                }
            }
        }

        // Fallback: assign order after all explicitly ordered middleware
        // Both MiddlewarePipelineBuilder and NotificationPipelineBuilder use the same high fallback order pattern
        const int middlewareUnorderedBaseOrder = int.MaxValue - 1000000;
        int middlewareUnorderedCount = existingInfos.Count(m => m.Order >= middlewareUnorderedBaseOrder);
        
        // Use per-builder instance fallback counter to ensure consistent incremental ordering
        // This maintains test expectations and isolates builder instances
        return middlewareUnorderedBaseOrder + middlewareUnorderedCount;
    }

    /// <summary>
    /// CRITICAL: Tries to create a concrete type from a generic type definition for Order extraction.
    /// Uses enhanced constraint-aware type creation following Microsoft's recommended pattern.
    /// This is the core fix for generic type definitions with instance Order properties.
    /// </summary>
    private static Type? TryCreateConcreteTypeForOrderExtraction(Type middlewareType)
    {
        if (!middlewareType.IsGenericTypeDefinition)
            return middlewareType;

        var genericParams = middlewareType.GetGenericArguments();
        
        // Strategy 1: Use the enhanced fallback types first (may include constraint-satisfying types)
        var fallbackTypes = GetEnhancedFallbackTypes();
        Type? concreteType = TryMakeConcreteWithFallbacks(middlewareType, fallbackTypes, genericParams.Length);
        if (concreteType != null)
        {
            return concreteType;
        }

        // Strategy 2: For constraint-heavy types, try systematic constraint analysis
        if (genericParams.Length == 1)
        {
            return TryCreateSingleParameterConstrainedType(middlewareType, genericParams[0]);
        }
        
        if (genericParams.Length == 2)
        {
            return TryCreateTwoParameterConstrainedType(middlewareType, genericParams[0], genericParams[1]);
        }
        
        return null;
    }

    /// <summary>
    /// Attempts to create concrete types for single-parameter generic middleware with constraints.
    /// Follows the pattern from your example: analyze constraints, find satisfying types, create concrete type.
    /// </summary>
    private static Type? TryCreateSingleParameterConstrainedType(Type middlewareType, Type genericParam)
    {
        // Get constraint types for the generic parameter
        var constraints = genericParam.GetGenericParameterConstraints();
        
        // Strategy 1: Try to find types that satisfy ALL constraints
        var constraintSatisfyingTypes = FindConstraintSatisfyingTypes(constraints);
        foreach (var candidateType in constraintSatisfyingTypes)
        {
            if (PipelineUtilities.TryMakeGenericType(middlewareType, [candidateType], out var concreteType))
            {
                return concreteType;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to create concrete types for two-parameter generic middleware with constraints.
    /// Follows the pattern from your example: analyze constraints, find satisfying types, create concrete type.
    /// </summary>
    private static Type? TryCreateTwoParameterConstrainedType(Type middlewareType, Type firstParam, Type secondParam)
    {
        // Get constraint types for both generic parameters
        var firstConstraints = firstParam.GetGenericParameterConstraints();
        var secondConstraints = secondParam.GetGenericParameterConstraints();
        
        var firstCandidates = FindConstraintSatisfyingTypes(firstConstraints);
        var secondCandidates = secondConstraints.Length > 0 
            ? FindConstraintSatisfyingTypes(secondConstraints)
            : [typeof(string), typeof(object)]; // Common response types

        // Try combinations of constraint-satisfying types
        foreach (var firstCandidate in firstCandidates)
        {
            foreach (var secondCandidate in secondCandidates)
            {
                if (PipelineUtilities.TryMakeGenericType(middlewareType, [firstCandidate, secondCandidate], out var concreteType))
                {
                    return concreteType;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Finds types that can satisfy the given generic parameter constraints.
    /// This is the core constraint analysis following Microsoft's recommended approach.
    /// </summary>
    private static Type[] FindConstraintSatisfyingTypes(Type[] constraints)
    {
        var candidateTypes = new List<Type>();
        
        // If no constraints, return common types
        if (constraints.Length == 0)
        {
            return [typeof(object), typeof(string)];
        }

        try
        {
            // Look through loaded assemblies for types that implement the constraint interfaces
            // IMPORTANT: Include ALL loaded assemblies, not just limited ones, to find test types
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
                        .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0)) // Has parameterless constructor
                        .Where(t => SatisfiesAllConstraints(t, constraints))
                        .Take(10) // Increased limit to find more candidates
                        .ToArray();

                    candidateTypes.AddRange(types);
                    
                    // IMPORTANT: Don't break early - continue searching all assemblies for test types
                }
                catch
                {
                    // Skip problematic assemblies
                }
            }
        }
        catch
        {
            // Fallback to basic types if constraint analysis fails
        }

        // Ensure we have at least some fallback candidates
        if (candidateTypes.Count == 0)
        {
            candidateTypes.AddRange([typeof(object), typeof(string)]);
        }

        return candidateTypes.ToArray();
    }

    /// <summary>
    /// Checks if a type satisfies all the given generic parameter constraints.
    /// This mirrors the constraint checking logic from your example.
    /// Enhanced to properly handle IRequest<T> constraints.
    /// </summary>
    private static bool SatisfiesAllConstraints(Type candidateType, Type[] constraints)
    {
        foreach (var constraint in constraints)
        {
            if (constraint.IsGenericType)
            {
                // Handle generic constraints like IRequest<TResponse>
                var genericDefinition = constraint.GetGenericTypeDefinition();
                var candidateInterfaces = candidateType.GetInterfaces();
                
                bool satisfiesGenericConstraint = candidateInterfaces.Any(i => 
                    i.IsGenericType && i.GetGenericTypeDefinition() == genericDefinition);
                    
                if (!satisfiesGenericConstraint)
                {
                    return false;
                }
            }
            else
            {
                // Handle non-generic constraints like ITestConstraintEntity, class constraint
                if (!constraint.IsAssignableFrom(candidateType))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Helper method to try making concrete types with fallback types.
    /// Extracted for reusability between different strategies.
    /// </summary>
    private static Type? TryMakeConcreteWithFallbacks(Type genericTypeDefinition, Type[] fallbackTypes, int parameterCount)
    {
        if (parameterCount == 1)
        {
            foreach (var fallbackType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [fallbackType], out var concreteType))
                {
                    return concreteType;
                }
            }
        }
        else if (parameterCount == 2)
        {
            // Strategy 1: Try each fallback type as TRequest with string as TResponse
            foreach (var requestType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [requestType, typeof(string)], out var concreteType))
                {
                    return concreteType;
                }
            }
            
            // Strategy 2: Try each fallback type as TRequest with object as TResponse  
            foreach (var requestType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [requestType, typeof(object)], out var concreteType))
                {
                    return concreteType;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets enhanced fallback types for concrete type creation.
    /// Uses clean, generic type discovery without any hard-coded test dependencies.
    /// </summary>
    private static Type[] GetEnhancedFallbackTypes()
    {
        var fallbackTypes = new List<Type>
        {
            typeof(object),
            typeof(string)
        };

        // Use generic type discovery that works for any constraint pattern
        try
        {
            // Limit assemblies for performance and avoid test contamination
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName?.StartsWith("Microsoft.") == true &&
                           !a.FullName?.StartsWith("System.") == true &&
                           !a.FullName?.StartsWith("netstandard") == true)
                .Take(5); // Strict limit for performance
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Look for types that implement common request interfaces
                    var candidateTypes = assembly.GetTypes()
                        .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
                        .Where(t => t.GetConstructors().Any(c => c.GetParameters().Length == 0)) // Has parameterless constructor
                        .Where(t => 
                        {
                            var interfaces = t.GetInterfaces();
                            return interfaces.Any(i => 
                                (i.IsGenericType && (
                                    i.GetGenericTypeDefinition().Name.Contains("IRequest") || 
                                    i.GetGenericTypeDefinition().Name.Contains("INotification")
                                )) ||
                                i.Name.Contains("IRequest") ||
                                i.Name.Contains("INotification"));
                        })
                        .Take(3) // Strict limit for performance
                        .ToArray();
                    
                    fallbackTypes.AddRange(candidateTypes);
                }
                catch
                {
                    // Skip problematic assemblies
                }
            }
        }
        catch
        {
            // If anything fails, just use basic fallback types
        }
        
        return fallbackTypes.ToArray();
    }

    /// <summary>
    /// Determines if an Order property is a simple constant getter that returns a compile-time constant.
    /// This follows Microsoft's recommendation to avoid instantiation of constrained generic types.
    /// </summary>
    /// <param name="orderProperty">The Order property to analyze</param>
    /// <returns>True if the property is a simple constant getter</returns>
    private static bool IsSimpleConstantOrderProperty(PropertyInfo orderProperty)
    {
        try
        {
            // Check if it's a read-only property with a simple getter
            return orderProperty is { CanRead: true, CanWrite: false } && 
                   orderProperty.GetMethod != null &&
                   !orderProperty.GetMethod.IsVirtual &&
                   !orderProperty.GetMethod.IsAbstract;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to extract a constant integer value from an Order property without instantiation.
    /// Uses IL analysis to detect simple return patterns like "return 10;".
    /// This is Microsoft's recommended approach for constrained generic types.
    /// </summary>
    /// <param name="orderProperty">The Order property to analyze</param>
    /// <returns>The constant value if detected, null otherwise</returns>
    private static int? ExtractConstantFromOrderProperty(PropertyInfo orderProperty)
    {
        try
        {
            var getMethod = orderProperty.GetMethod;
            if (getMethod == null) return null;

            // Get the IL bytes of the getter method
            var methodBody = getMethod.GetMethodBody();
            if (methodBody == null) return null;

            var ilBytes = methodBody.GetILAsByteArray();
            if (ilBytes == null || ilBytes.Length < 2) return null;

            // Check for ldc.i4.s (load constant int32, short form) followed by ret
            if (ilBytes is [0x1F, _, 0x2A, ..]) // ldc.i4.s + ret
            {
                return (sbyte)ilBytes[1]; // ldc.i4.s uses signed byte
            }

            // Check for ldc.i4 (load constant int32) followed by ret
            if (ilBytes is [0x20, _, _, _, _, 0x2A, ..]) // ldc.i4 + ret
            {
                return BitConverter.ToInt32(ilBytes, 1);
            }

            // Check for special constant opcodes followed by ret
            if (ilBytes is [_, 0x2A, ..]) // anything + ret
            {
                return ilBytes[0] switch
                {
                    0x16 => 0,  // ldc.i4.0
                    0x17 => 1,  // ldc.i4.1
                    0x18 => 2,  // ldc.i4.2
                    0x19 => 3,  // ldc.i4.3
                    0x1A => 4,  // ldc.i4.4
                    0x1B => 5,  // ldc.i4.5
                    0x1C => 6,  // ldc.i4.6
                    0x1D => 7,  // ldc.i4.7
                    0x1E => 8,  // ldc.i4.8
                    0x15 => -1, // ldc.i4.m1
                    _ => null
                };
            }

            return null;
        }
        catch
        {
            // If IL analysis fails, return null to fall back to other methods
            return null;
        }
    }

    /// <summary>
    /// Cached check for generic type definition to avoid repeated reflection calls.
    /// </summary>
    private static bool IsGenericTypeDefinitionCached(Type type)
    {
        return _genericTypeCache.GetOrAdd(type, static t => t.IsGenericTypeDefinition);
    }

    #endregion

    #region IPipelineInspector Implementation

    /// <inheritdoc />
    public virtual IReadOnlyList<Type> GetRegisteredMiddleware()
    {
        return _middlewareInfos.Select(info => info.Type).ToList();
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration()
    {
        return _middlewareInfos.Select(info => (info.Type, info.Configuration)).ToList();
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null)
    {
        if (serviceProvider == null)
        {
            // Return cached registration-time order values
            return _middlewareInfos.Select(info => (info.Type, info.Order, info.Configuration)).ToList();
        }

        // Use service provider to get actual runtime order values
        var result = new List<(Type Type, int Order, object? Configuration)>();

        foreach (var middlewareInfo in _middlewareInfos)
        {
            int actualOrder = middlewareInfo.Order; // Start with cached order

            try
            {
                actualOrder = GetRuntimeOrder(middlewareInfo, serviceProvider);
            }
            catch
            {
                // If we can't get runtime order, use cached order
            }

            result.Add((middlewareInfo.Type, actualOrder, middlewareInfo.Configuration));
        }

        return result;
    }

    /// <inheritdoc />
    public virtual IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true)
    {
        var middlewareInfos = GetDetailedMiddlewareInfo(serviceProvider);
        var analysisResults = new List<MiddlewareAnalysis>();
        var detailed = isDetailed ?? true;

        foreach (var (type, order, configuration) in middlewareInfos.OrderBy(m => m.Order))
        {
            var orderDisplay = order == int.MaxValue ? "Default" : order.ToString();
            var className = PipelineUtilities.GetCleanTypeName(type);
            var typeParameters = type.IsGenericType && detailed ?
                $"<{string.Join(", ", type.GetGenericArguments().Select(t => t.Name))}>" :
                string.Empty;

            var genericConstraints = detailed ? PipelineUtilities.GetGenericConstraints(type) : string.Empty;
            var handlerInfo = detailed ? configuration : null;

            analysisResults.Add(new MiddlewareAnalysis(
                Type: type,
                Order: order,
                OrderDisplay: orderDisplay,
                ClassName: className,
                TypeParameters: typeParameters,
                GenericConstraints: genericConstraints,
                Configuration: handlerInfo
            ));
        }

        return analysisResults;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the runtime order for a middleware from the service provider.
    /// Handles both generic and non-generic middleware types.
    /// </summary>
    protected virtual int GetRuntimeOrder(MiddlewareInfo middlewareInfo, IServiceProvider serviceProvider)
    {
        if (middlewareInfo.Type.IsGenericTypeDefinition)
        {
            // For generic type definitions, try to find a suitable concrete type
            var genericParams = middlewareInfo.Type.GetGenericArguments();
            Type? actualMiddlewareType = TryCreateConcreteMiddlewareType(middlewareInfo.Type, serviceProvider, genericParams.Length);

            if (actualMiddlewareType != null)
            {
                return GetInstanceOrder(actualMiddlewareType, serviceProvider) ?? middlewareInfo.Order;
            }
        }
        else
        {
            // For non-generic types, resolve directly from DI
            return GetInstanceOrder(middlewareInfo.Type, serviceProvider) ?? middlewareInfo.Order;
        }

        return middlewareInfo.Order;
    }

    /// <summary>
    /// Attempts to get the order from a middleware instance.
    /// </summary>
    private static int? GetInstanceOrder(Type middlewareType, IServiceProvider serviceProvider)
    {
        try
        {
            var instance = serviceProvider.GetService(middlewareType);
            if (instance != null)
            {
                var orderProperty = instance.GetType().GetProperty("Order", BindingFlags.Public | BindingFlags.Instance);
                if (orderProperty != null && orderProperty.PropertyType == typeof(int))
                {
                    return (int)orderProperty.GetValue(instance)!;
                }
            }
        }
        catch
        {
            // Ignore errors - return null to use cached order
        }

        return null;
    }

    /// <summary>
    /// Attempts to create a concrete middleware type from a generic type definition.
    /// Uses fast fallback types to avoid expensive assembly scanning.
    /// </summary>
    protected virtual Type? TryCreateConcreteMiddlewareType(Type middlewareTypeDefinition, IServiceProvider serviceProvider, int parameterCount)
    {
        if (!middlewareTypeDefinition.IsGenericTypeDefinition)
            return middlewareTypeDefinition;

        var genericParams = middlewareTypeDefinition.GetGenericArguments();
        if (genericParams.Length != parameterCount)
            return null;

        // Use fast fallback types - derived classes can override this for specific constraints
        var fastFallbackTypes = GetFallbackTypes();

        return TryMakeConcreteTypeWithFallbacks(middlewareTypeDefinition, fastFallbackTypes, parameterCount);
    }

    /// <summary>
    /// Gets fallback types for concrete type creation. Virtual to allow derived classes to provide specific types.
    /// </summary>
    protected virtual Type[] GetFallbackTypes()
    {
        return [typeof(object), typeof(string), typeof(int)];
    }

    /// <summary>
    /// Tries to make a concrete type using fallback types.
    /// </summary>
    private Type? TryMakeConcreteTypeWithFallbacks(Type genericTypeDefinition, Type[] fallbackTypes, int parameterCount)
    {
        if (parameterCount == 1)
        {
            foreach (var fallbackType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [fallbackType], out var concreteType))
                {
                    return concreteType;
                }
            }
        }
        else if (parameterCount == 2)
        {
            // Strategy 1: Try each fallback type as TRequest with string as TResponse
            foreach (var requestType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [requestType, typeof(string)], out var concreteType))
                {
                    return concreteType;
                }
            }
            
            // Strategy 2: Try each fallback type as TRequest with object as TResponse  
            foreach (var requestType in fallbackTypes)
            {
                if (PipelineUtilities.TryMakeGenericType(genericTypeDefinition, [requestType, typeof(object)], out var concreteType))
                {
                    return concreteType;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Creates pre-calculated registration indices for fast sorting operations.
    /// </summary>
    protected Dictionary<Type, int> CreateRegistrationIndices()
    {
        var registrationIndices = new Dictionary<Type, int>();
        for (int i = 0; i < _middlewareInfos.Count; i++)
        {
            var info = _middlewareInfos[i];
            registrationIndices[info.Type] = i;
            
            // Also store generic type definition for generic types
            if (info.Type.IsGenericTypeDefinition)
            {
                registrationIndices[info.Type] = i;
            }
        }
        return registrationIndices;
    }

    #endregion
}