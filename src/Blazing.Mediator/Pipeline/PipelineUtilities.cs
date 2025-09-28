namespace Blazing.Mediator.Pipeline;

/// <summary>
/// High-performance utility methods shared across pipeline builders.
/// Includes advanced thread-safe caching, registration-time optimizations,
/// and performance-critical path optimizations to eliminate duplicate work and maximize runtime performance.
/// </summary>
internal static class PipelineUtilities
{
    // Performance-optimized with concurrent caching
    private static readonly ConcurrentDictionary<Type, string> _cleanTypeNameCache = new();
    private static readonly ConcurrentDictionary<Type, string> _formattedTypeNameCache = new();
    private static readonly ConcurrentDictionary<Type, string> _genericConstraintsCache = new();
    
    // Advanced caching for performance-critical operations
    private static readonly ConcurrentDictionary<Type, bool> _isGenericTypeCache = new();
    private static readonly ConcurrentDictionary<Type, bool> _isGenericTypeDefinitionCache = new();
    private static readonly ConcurrentDictionary<Type, Type[]> _interfaceCache = new();
    private static readonly ConcurrentDictionary<Type, Type[]> _genericArgumentsCache = new();
    private static readonly ConcurrentDictionary<Type, Type> _genericTypeDefinitionCache = new();
    
    // Constraint satisfaction caching for expensive reflection operations
    private static readonly ConcurrentDictionary<string, bool> _constraintSatisfactionCache = new();
    private static readonly ConcurrentDictionary<string, Type?> _genericTypeCreationCache = new();
    
    // Interface pattern caching for fast middleware compatibility checks
    private static readonly ConcurrentDictionary<Type, MiddlewareCompatibilityInfo> _middlewareCompatibilityCache = new();
    
    /// <summary>
    /// Cached middleware compatibility information for fast runtime lookups
    /// </summary>
    internal sealed record MiddlewareCompatibilityInfo(
        bool IsGenericTypeDefinition,
        int GenericArgumentCount,
        Type[] CachedInterfaces,
        string[] InterfaceNames,
        bool IsRequestMiddleware,
        bool IsNotificationMiddleware,
        bool IsStreamMiddleware)
    {
        public static MiddlewareCompatibilityInfo Create(Type middlewareType)
        {
            var interfaces = middlewareType.GetInterfaces();
            var interfaceNames = interfaces.Select(i => i.Name).ToArray();
            
            bool isRequestMiddleware = interfaces.Any(i => 
                i.IsGenericType && (
                    i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<>) ||
                    i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>)
                ));
                
            bool isNotificationMiddleware = interfaces.Any(i => 
                i == typeof(INotificationMiddleware) ||
                (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>)));
                
            bool isStreamMiddleware = interfaces.Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>));
            
            return new MiddlewareCompatibilityInfo(
                IsGenericTypeDefinition: middlewareType.IsGenericTypeDefinition,
                GenericArgumentCount: middlewareType.IsGenericTypeDefinition ? middlewareType.GetGenericArguments().Length : 0,
                CachedInterfaces: interfaces,
                InterfaceNames: interfaceNames,
                IsRequestMiddleware: isRequestMiddleware,
                IsNotificationMiddleware: isNotificationMiddleware,
                IsStreamMiddleware: isStreamMiddleware);
        }
    }

    /// <summary>
    /// Gets the type name preserving generic arity notation for display purposes.
    /// Uses thread-safe caching for performance optimization.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>The clean type name without backtick notation (e.g., "ErrorHandlingMiddleware" instead of "ErrorHandlingMiddleware`1").</returns>
    public static string GetCleanTypeName(Type type)
    {
        return _cleanTypeNameCache.GetOrAdd(type, static t =>
        {
            var typeName = t.Name;
            var backtickIndex = typeName.IndexOf('`');
            return backtickIndex > 0 ? typeName[..backtickIndex] : typeName;
        });
    }

    /// <summary>
    /// Formats a type name for display, handling generic types nicely.
    /// Uses thread-safe caching for performance optimization.
    /// </summary>
    /// <param name="type">The type to format.</param>
    /// <returns>A formatted type name string.</returns>
    public static string FormatTypeName(Type type)
    {
        return _formattedTypeNameCache.GetOrAdd(type, static t =>
        {
            if (!t.IsGenericType)
                return t.Name;

            var genericTypeName = t.Name;
            var backtickIndex = genericTypeName.IndexOf('`');
            if (backtickIndex > 0)
            {
                genericTypeName = genericTypeName[..backtickIndex];
            }

            var genericArgs = t.GetGenericArguments();
            var genericArgNames = genericArgs.Select(arg => arg.IsGenericParameter ? arg.Name : FormatTypeName(arg));

            return $"{genericTypeName}<{string.Join(", ", genericArgNames)}>";
        });
    }

    /// <summary>
    /// Extracts generic constraints from a type for display purposes.
    /// Uses thread-safe caching for performance optimization.
    /// </summary>
    /// <param name="type">The type to analyze for generic constraints.</param>
    /// <returns>A formatted string describing the generic constraints.</returns>
    public static string GetGenericConstraints(Type type)
    {
        return _genericConstraintsCache.GetOrAdd(type, static t =>
        {
            if (!t.IsGenericTypeDefinition)
                return string.Empty;

            var genericParameters = t.GetGenericArguments();
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
        });
    }

    /// <summary>
    /// Safely attempts to create a generic type with the given type arguments.
    /// Uses comprehensive caching to avoid repeated expensive type creation operations.
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition.</param>
    /// <param name="typeArguments">The type arguments to apply.</param>
    /// <param name="concreteType">The resulting concrete type if successful.</param>
    /// <returns>True if the type was successfully created, false otherwise.</returns>
    public static bool TryMakeGenericType(Type genericTypeDefinition, Type[] typeArguments, out Type? concreteType)
    {
        concreteType = null;

        // Create cache key for type creation
        var cacheKey = CreateGenericTypeCreationCacheKey(genericTypeDefinition, typeArguments);
        
        // Check cache first
        if (_genericTypeCreationCache.TryGetValue(cacheKey, out var cachedResult))
        {
            concreteType = cachedResult;
            return cachedResult != null;
        }

        try
        {
            // First check if the constraints can be satisfied
            if (!CanSatisfyGenericConstraints(genericTypeDefinition, typeArguments))
            {
                _genericTypeCreationCache.TryAdd(cacheKey, null);
                return false;
            }

            // Try to create the concrete type
            concreteType = genericTypeDefinition.MakeGenericType(typeArguments);
            _genericTypeCreationCache.TryAdd(cacheKey, concreteType);
            return true;
        }
        catch (ArgumentException)
        {
            // Constraints were not satisfied
            _genericTypeCreationCache.TryAdd(cacheKey, null);
            return false;
        }
        catch
        {
            // Other error
            _genericTypeCreationCache.TryAdd(cacheKey, null);
            return false;
        }
    }

    /// <summary>
    /// Fast lookup for registration index using pre-calculated dictionary.
    /// Optimized for minimal overhead and maximum throughput.
    /// </summary>
    /// <param name="middlewareType">The middleware type to find the index for.</param>
    /// <param name="registrationIndices">Pre-calculated dictionary of type to index mappings.</param>
    /// <returns>The registration index, or int.MaxValue if not found.</returns>
    public static int GetRegistrationIndex(Type middlewareType, Dictionary<Type, int> registrationIndices)
    {
        if (registrationIndices.TryGetValue(middlewareType, out int index))
        {
            return index;
        }

        // For generic types, try to find by generic type definition
        if (IsGenericTypeCached(middlewareType))
        {
            var genericTypeDef = GetGenericTypeDefinitionCached(middlewareType);
            if (registrationIndices.TryGetValue(genericTypeDef, out int genericIndex))
            {
                return genericIndex;
            }
        }

        return int.MaxValue; // Fallback for not found
    }

    /// <summary>
    /// Checks if a generic type definition can be instantiated with the given type arguments
    /// by validating all generic constraints.
    /// Uses aggressive caching to minimize reflection overhead.
    /// </summary>
    /// <param name="genericTypeDefinition">The generic type definition to check.</param>
    /// <param name="typeArguments">The type arguments to validate against the constraints.</param>
    /// <returns>True if the type can be instantiated with the given arguments, false otherwise.</returns>
    public static bool CanSatisfyGenericConstraints(Type genericTypeDefinition, params Type[] typeArguments)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            return false;

        // Create cache key for constraint satisfaction
        var cacheKey = CreateConstraintSatisfactionCacheKey(genericTypeDefinition, typeArguments);
        
        if (_constraintSatisfactionCache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        var result = PerformConstraintValidation(genericTypeDefinition, typeArguments);
        _constraintSatisfactionCache.TryAdd(cacheKey, result);
        return result;
    }

    /// <summary>
    /// High-performance middleware compatibility checker.
    /// Pre-caches all middleware compatibility information for O(1) runtime lookups.
    /// </summary>
    /// <param name="middlewareType">The middleware type to analyze.</param>
    /// <returns>Cached compatibility information for the middleware.</returns>
    public static MiddlewareCompatibilityInfo GetMiddlewareCompatibilityInfo(Type middlewareType)
    {
        return _middlewareCompatibilityCache.GetOrAdd(middlewareType, MiddlewareCompatibilityInfo.Create);
    }

    /// <summary>
    /// Fast cached check for generic type.
    /// Eliminates repeated reflection calls for this commonly used property.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a generic type.</returns>
    public static bool IsGenericTypeCached(Type type)
    {
        return _isGenericTypeCache.GetOrAdd(type, static t => t.IsGenericType);
    }

    /// <summary>
    /// Fast cached check for generic type definition.
    /// Eliminates repeated reflection calls for this commonly used property.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a generic type definition.</returns>
    public static bool IsGenericTypeDefinitionCached(Type type)
    {
        return _isGenericTypeDefinitionCache.GetOrAdd(type, static t => t.IsGenericTypeDefinition);
    }

    /// <summary>
    /// Fast cached access to type interfaces.
    /// Avoids expensive GetInterfaces() calls during pipeline execution.
    /// </summary>
    /// <param name="type">The type to get interfaces for.</param>
    /// <returns>Cached array of interfaces implemented by the type.</returns>
    public static Type[] GetInterfacesCached(Type type)
    {
        return _interfaceCache.GetOrAdd(type, static t => t.GetInterfaces());
    }

    /// <summary>
    /// Fast cached access to generic arguments.
    /// Optimizes generic type analysis during pipeline execution.
    /// </summary>
    /// <param name="type">The type to get generic arguments for.</param>
    /// <returns>Cached array of generic arguments.</returns>
    public static Type[] GetGenericArgumentsCached(Type type)
    {
        return _genericArgumentsCache.GetOrAdd(type, static t => t.GetGenericArguments());
    }

    /// <summary>
    /// Fast cached access to generic type definition.
    /// Eliminates repeated GetGenericTypeDefinition() calls.
    /// </summary>
    /// <param name="type">The generic type.</param>
    /// <returns>The cached generic type definition.</returns>
    public static Type GetGenericTypeDefinitionCached(Type type)
    {
        return _genericTypeDefinitionCache.GetOrAdd(type, static t => t.IsGenericType ? t.GetGenericTypeDefinition() : t);
    }

    /// <summary>
    /// Registration-time optimization method to populate cache fields.
    /// This should be called during DI registration to minimize runtime reflection calls.
    /// Now includes comprehensive pre-caching of all performance-critical metadata.
    /// </summary>
    /// <param name="info">The middleware info to optimize.</param>
    /// <returns>A new MiddlewareInfo instance with populated cache fields.</returns>
    public static MiddlewareInfo OptimizeMiddlewareInfo(MiddlewareInfo info)
    {
        // Pre-populate all relevant caches for this middleware type
        _ = GetCleanTypeName(info.Type);
        _ = FormatTypeName(info.Type);
        _ = IsGenericTypeCached(info.Type);
        _ = GetInterfacesCached(info.Type);
        _ = GetMiddlewareCompatibilityInfo(info.Type);
        
        if (info.Type.IsGenericTypeDefinition)
        {
            _ = GetGenericArgumentsCached(info.Type);
            _ = GetGenericConstraints(info.Type);
        }
        
        if (info.Type.IsGenericType)
        {
            _ = GetGenericTypeDefinitionCached(info.Type);
        }

        return info.WithCache();
    }

    /// <summary>
    /// Pre-sorts middleware collection for optimal runtime performance.
    /// Uses optimized sorting algorithm with cached comparison values.
    /// </summary>
    /// <param name="middleware">The middleware collection to sort.</param>
    /// <returns>A pre-sorted read-only list of middleware.</returns>
    public static IReadOnlyList<MiddlewareInfo> PreSortMiddleware(IReadOnlyList<MiddlewareInfo> middleware)
    {
        var sorted = new List<MiddlewareInfo>(middleware);
        
        // Pre-calculate registration indices for sorting
        var registrationIndices = new Dictionary<Type, int>();
        for (int i = 0; i < sorted.Count; i++)
        {
            var info = sorted[i];
            registrationIndices[info.Type] = i;
            
            // Also store generic type definition for generic types
            if (IsGenericTypeCached(info.Type))
            {
                var genericTypeDef = GetGenericTypeDefinitionCached(info.Type);
                registrationIndices[genericTypeDef] = i;
            }
        }

        // Optimized sorting with pre-cached values
        sorted.Sort((a, b) =>
        {
            int orderComparison = a.Order.CompareTo(b.Order);
            if (orderComparison != 0) return orderComparison;

            int indexA = GetRegistrationIndex(a.Type, registrationIndices);
            int indexB = GetRegistrationIndex(b.Type, registrationIndices);
            return indexA.CompareTo(indexB);
        });

        return sorted;
    }

    /// <summary>
    /// Batch pre-caching method for registration-time optimization.
    /// Pre-caches compatibility information for multiple middleware types at once for maximum efficiency.
    /// This should be called during MediatorRegistrationService to minimize runtime reflection.
    /// </summary>
    /// <param name="middlewareTypes">The middleware types to pre-cache.</param>
    public static void PreCacheMiddlewareCompatibility(IEnumerable<Type> middlewareTypes)
    {
        var types = middlewareTypes.ToArray();
        
        // Use parallel processing for large batches to speed up registration
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        
        Parallel.ForEach(types, options, middlewareType =>
        {
            try
            {
                // Pre-populate all caches for this middleware type
                _ = GetCleanTypeName(middlewareType);
                _ = FormatTypeName(middlewareType);
                _ = IsGenericTypeCached(middlewareType);
                _ = GetInterfacesCached(middlewareType);
                _ = GetMiddlewareCompatibilityInfo(middlewareType);
                
                if (middlewareType.IsGenericTypeDefinition)
                {
                    _ = GetGenericArgumentsCached(middlewareType);
                    _ = GetGenericConstraints(middlewareType);
                }
                
                if (middlewareType.IsGenericType)
                {
                    _ = GetGenericTypeDefinitionCached(middlewareType);
                }
            }
            catch
            {
                // Skip problematic types during pre-caching
                // They will be handled normally at runtime
            }
        });
    }

    /// <summary>
    /// Cache metrics and health monitoring.
    /// Provides insights into cache effectiveness for performance tuning.
    /// </summary>
    /// <returns>Cache performance metrics.</returns>
    public static CacheMetrics GetCacheMetrics()
    {
        return new CacheMetrics(
            CleanTypeNameCacheSize: _cleanTypeNameCache.Count,
            FormattedTypeNameCacheSize: _formattedTypeNameCache.Count,
            GenericConstraintsCacheSize: _genericConstraintsCache.Count,
            IsGenericTypeDefinitionCacheSize: _isGenericTypeDefinitionCache.Count,
            InterfaceCacheSize: _interfaceCache.Count,
            GenericArgumentsCacheSize: _genericArgumentsCache.Count,
            GenericTypeDefinitionCacheSize: _genericTypeDefinitionCache.Count,
            ConstraintSatisfactionCacheSize: _constraintSatisfactionCache.Count,
            GenericTypeCreationCacheSize: _genericTypeCreationCache.Count,
            MiddlewareCompatibilityCacheSize: _middlewareCompatibilityCache.Count
        );
    }

    /// <summary>
    /// Cache metrics record for performance monitoring.
    /// </summary>
    public sealed record CacheMetrics(
        int CleanTypeNameCacheSize,
        int FormattedTypeNameCacheSize,
        int GenericConstraintsCacheSize,
        int IsGenericTypeDefinitionCacheSize,
        int InterfaceCacheSize,
        int GenericArgumentsCacheSize,
        int GenericTypeDefinitionCacheSize,
        int ConstraintSatisfactionCacheSize,
        int GenericTypeCreationCacheSize,
        int MiddlewareCompatibilityCacheSize)
    {
        public int TotalCacheEntries => CleanTypeNameCacheSize + FormattedTypeNameCacheSize + GenericConstraintsCacheSize +
                                       IsGenericTypeDefinitionCacheSize + InterfaceCacheSize + GenericArgumentsCacheSize +
                                       GenericTypeDefinitionCacheSize + ConstraintSatisfactionCacheSize +
                                       GenericTypeCreationCacheSize + MiddlewareCompatibilityCacheSize;
    }

    #region Private Helper Methods

    /// <summary>
    /// Performs the actual constraint validation logic without caching.
    /// </summary>
    private static bool PerformConstraintValidation(Type genericTypeDefinition, Type[] typeArguments)
    {
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
    /// Creates a cache key for constraint satisfaction caching.
    /// </summary>
    private static string CreateConstraintSatisfactionCacheKey(Type genericTypeDefinition, Type[] typeArguments)
    {
        var typeArgNames = typeArguments.Select(t => t.FullName ?? t.Name);
        return $"{genericTypeDefinition.FullName}[{string.Join(",", typeArgNames)}]";
    }

    /// <summary>
    /// Creates a cache key for generic type creation caching.
    /// </summary>
    private static string CreateGenericTypeCreationCacheKey(Type genericTypeDefinition, Type[] typeArguments)
    {
        var typeArgNames = typeArguments.Select(t => t.FullName ?? t.Name);
        return $"CREATE:{genericTypeDefinition.FullName}[{string.Join(",", typeArgNames)}]";
    }

    #endregion
}