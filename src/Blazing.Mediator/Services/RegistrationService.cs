using Blazing.Mediator.Statistics;
using System.Collections.Frozen;

namespace Blazing.Mediator.Services;

/// <summary>
/// Static service responsible for registering mediator services, handlers, and middleware in the dependency injection container.
/// This class encapsulates the core registration logic separated from the extension methods.
/// Enhanced with registration-time performance optimizations and pre-caching capabilities.
/// </summary>
internal static class RegistrationService
{
    // Advanced high-impact pre-caching during DI registration
    private static readonly ConcurrentDictionary<Type, MiddlewareMetadata> _middlewareCache = new();
    private static readonly ConcurrentDictionary<Type, HandlerMetadata> _handlerCache = new();
    
    // Compile-time optimizations using frozen collections for better performance
    private static readonly ConcurrentDictionary<Assembly, FrozenSet<Type>> _assemblyTypeCache = new();
    private static readonly ConcurrentDictionary<Type, FrozenSet<Type>> _interfaceCache = new();
    
    // Pre-calculated common type lookups for maximum performance
    private static readonly FrozenDictionary<string, Type> _commonMiddlewareInterfaces = new Dictionary<string, Type>
    {
        ["IRequestMiddleware<>"] = typeof(IRequestMiddleware<>),
        ["IRequestMiddleware<,>"] = typeof(IRequestMiddleware<,>),
        ["IConditionalMiddleware<>"] = typeof(IConditionalMiddleware<>),
        ["IConditionalMiddleware<,>"] = typeof(IConditionalMiddleware<,>),
        ["IStreamRequestMiddleware<,>"] = typeof(IStreamRequestMiddleware<,>),
        ["IConditionalStreamRequestMiddleware<,>"] = typeof(IConditionalStreamRequestMiddleware<,>),
        ["INotificationMiddleware"] = typeof(INotificationMiddleware),
        ["INotificationMiddleware<>"] = typeof(INotificationMiddleware<>),
        ["IConditionalNotificationMiddleware"] = typeof(IConditionalNotificationMiddleware)
    }.ToFrozenDictionary();
    
    private static readonly FrozenDictionary<string, Type> _commonHandlerInterfaces = new Dictionary<string, Type>
    {
        ["IRequestHandler<>"] = typeof(IRequestHandler<>),
        ["IRequestHandler<,>"] = typeof(IRequestHandler<,>),
        ["IStreamRequestHandler<,>"] = typeof(IStreamRequestHandler<,>),
        ["INotificationHandler<>"] = typeof(INotificationHandler<>)
    }.ToFrozenDictionary();

    /// <summary>
    /// Cached metadata for middleware types to minimize runtime reflection calls.
    /// Enhanced with compile-time optimizations and comprehensive pre-caching.
    /// </summary>
    internal sealed record MiddlewareMetadata(
        int Order,                          // Pre-calculated at registration
        string CleanTypeName,               // Cached to avoid repeated reflection
        string[] InterfaceNames,            // Pre-calculated interface names
        bool IsGenericTypeDefinition,       // Cached boolean check
        Type[]? GenericConstraints,         // Pre-analyzed constraints
        bool IsConditionalMiddleware,       // Pre-calculated conditional check
        bool IsNotificationMiddleware,      // Pre-calculated notification middleware check
        bool IsStreamMiddleware,            // Pre-calculated stream middleware check
        Type? ConcreteImplementationType,   // Pre-resolved concrete type for generics
        FrozenSet<string> CachedInterfacePatterns  // Pre-analyzed interface patterns for fast lookup
    );

    /// <summary>
    /// Cached metadata for handler types to minimize runtime reflection calls.
    /// </summary>
    internal sealed record HandlerMetadata(
        string CleanTypeName,               // Cached handler name
        bool IsNotificationHandler,         // Pre-calculated notification handler check
        bool IsStreamHandler,               // Pre-calculated stream handler check
        Type[] SupportedRequestTypes,       // Pre-calculated supported request types
        FrozenSet<Type> CachedInterfaces    // Pre-cached interface types
    );

    /// <summary>
    /// Core implementation for all AddMediator methods to prevent recursion.
    /// Handles configuration creation, assembly resolution, and service registration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Optional action to configure middleware pipeline.</param>
    /// <param name="enableStatisticsTracking">Legacy parameter for statistics tracking (use configuration.StatisticsOptions instead).</param>
    /// <param name="discoverMiddleware">Legacy parameter for middleware discovery (use configuration.DiscoverMiddleware instead).</param>
    /// <param name="discoverNotificationMiddleware">Legacy parameter for notification middleware discovery (use configuration.DiscoverNotificationMiddleware instead).</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <param name="callingAssembly">Optional calling assembly to include in scanning.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection RegisterMediatorServices(
        IServiceCollection services,
        Action<MediatorConfiguration>? options,
        bool enableStatisticsTracking,
        bool? discoverMiddleware,
        bool? discoverNotificationMiddleware,
        Assembly[]? assemblies,
        Assembly? callingAssembly)
    {
        // Pre-warm assembly type cache for better performance
        if (assemblies is { Length: > 0 })
        {
            PreWarmAssemblyTypeCache(assemblies);
        }
        if (callingAssembly != null)
        {
            PreWarmAssemblyTypeCache([callingAssembly]);
        }

        // Check if there's already a MediatorConfiguration registered
        var existingConfigurationDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(MediatorConfiguration));
        MediatorConfiguration configuration;

        if (existingConfigurationDescriptor != null)
        {
            // Get the existing configuration instance
            if (existingConfigurationDescriptor.ImplementationInstance is MediatorConfiguration existingConfig)
            {
                configuration = existingConfig;
            }
            else
            {
                // If it's not an instance registration, create a new one and merge later
                configuration = new MediatorConfiguration(services);
            }
        }
        else
        {
            // Create new configuration
            configuration = new MediatorConfiguration(services);
        }

        // Apply configuration options if provided
        options?.Invoke(configuration);

        // Resolve assemblies based on what was provided
        Assembly[] targetAssemblies;
        
        // First, add assemblies from configuration if any
        var configurationAssemblies = configuration.Assemblies.ToArray();
        
        // Then merge with assemblies passed as parameters
        if (assemblies is { Length: > 0 })
        {
            targetAssemblies = configurationAssemblies.Concat(assemblies).Distinct().ToArray();
        }
        else if (callingAssembly != null)
        {
            targetAssemblies = configurationAssemblies.Concat([callingAssembly]).Distinct().ToArray();
        }
        else if (configurationAssemblies.Length > 0)
        {
            targetAssemblies = configurationAssemblies;
        }
        else
        {
            targetAssemblies = [];
        }

        // Add any additional assemblies from the parameters to the configuration
        if (assemblies is { Length: > 0 })
        {
            foreach (var assembly in assemblies)
            {
                configuration.AddFromAssembly(assembly);
            }
        }
        else if (callingAssembly != null)
        {
            configuration.AddFromAssembly(callingAssembly);
        }

        // Register the mediator services using the resolved configuration and assemblies
        return RegisterMediatorServicesCore(
            services,
            configuration,
            enableStatisticsTracking,
            discoverMiddleware,
            discoverNotificationMiddleware,
            targetAssemblies);
    }

    /// <summary>
    /// Registers the core mediator services in the dependency injection container.
    /// Optimized registration flow with pre-caching.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The mediator configuration.</param>
    /// <param name="enableStatisticsTracking">Legacy parameter for statistics tracking (use configuration.StatisticsOptions instead).</param>
    /// <param name="discoverMiddleware">Legacy parameter for middleware discovery (use configuration.DiscoverMiddleware instead).</param>
    /// <param name="discoverNotificationMiddleware">Legacy parameter for notification middleware discovery (use configuration.DiscoverNotificationMiddleware instead).</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection RegisterMediatorServicesCore(
        IServiceCollection services,
        MediatorConfiguration configuration,
        bool enableStatisticsTracking,
        bool? discoverMiddleware,
        bool? discoverNotificationMiddleware,
        Assembly[]? assemblies)
    {
        // Convert null values to false for backward compatibility
        bool actualDiscoverMiddleware = discoverMiddleware ?? false;
        bool actualDiscoverNotificationMiddleware = discoverNotificationMiddleware ?? false;

        // Support legacy parameter overrides (double setting values for backward compatibility)
        // Set legacy parameter values first
        bool finalEnableStatisticsTracking = enableStatisticsTracking;
        bool finalDiscoverMiddleware = actualDiscoverMiddleware;
        bool finalDiscoverNotificationMiddleware = actualDiscoverNotificationMiddleware;

        // Then override with configuration property values if they were set
#pragma warning disable CS0618 // Type or member is obsolete
        if (configuration.EnableStatisticsTracking)
            finalEnableStatisticsTracking = true;
#pragma warning restore CS0618
        if (configuration.DiscoverMiddleware)
            finalDiscoverMiddleware = true;
        if (configuration.DiscoverNotificationMiddleware)
            finalDiscoverNotificationMiddleware = true;

        // New approach: Check if StatisticsOptions is configured
        bool hasStatisticsOptions = configuration.StatisticsOptions != null;
        bool shouldRegisterStatistics = finalEnableStatisticsTracking || hasStatisticsOptions;

        // Check if TelemetryOptions is configured
        bool hasTelemetryOptions = configuration.TelemetryOptions != null;

        // Check if LoggingOptions is configured
        bool hasLoggingOptions = configuration.LoggingOptions != null;

        RegisterTelemetryOptions(services, configuration, hasTelemetryOptions);
        RegisterLoggingOptions(services, configuration, hasLoggingOptions);
        RegisterStatistics(services, configuration, shouldRegisterStatistics, hasStatisticsOptions);
        RegisterMediator(services, shouldRegisterStatistics);
        RegisterMiddleware(services, configuration, assemblies, finalDiscoverMiddleware, finalDiscoverNotificationMiddleware);
        RegisterPipelineComponents(services, configuration);
        RegisterHandlers(services, assemblies, configuration.DiscoverNotificationHandlers);

        // Pre-cache utilities for runtime performance
        if (assemblies is { Length: > 0 })
        {
            var middlewareTypes = GetCachedMiddlewareTypes(assemblies);
            PipelineUtilities.PreCacheMiddlewareCompatibility(middlewareTypes);
        }

        return services;
    }

    /// <summary>
    /// Registers telemetry options if configured and not already registered.
    /// </summary>
    private static void RegisterTelemetryOptions(IServiceCollection services, MediatorConfiguration configuration, bool hasTelemetryOptions)
    {
        if (hasTelemetryOptions && services.All(s => s.ServiceType != typeof(TelemetryOptions)))
        {
            services.AddSingleton(configuration.TelemetryOptions!);
        }
    }

    /// <summary>
    /// Registers logging options if configured and not already registered.
    /// </summary>
    private static void RegisterLoggingOptions(IServiceCollection services, MediatorConfiguration configuration, bool hasLoggingOptions)
    {
        if (hasLoggingOptions && services.All(s => s.ServiceType != typeof(LoggingOptions)))
        {
            services.AddSingleton(configuration.LoggingOptions!);
        }
    }

    /// <summary>
    /// Registers MediatorStatistics with default console renderer if not already registered and statistics tracking is enabled.
    /// </summary>
    private static void RegisterStatistics(IServiceCollection services, MediatorConfiguration configuration, bool shouldRegisterStatistics, bool hasStatisticsOptions)
    {
        // Register subscriber tracking services for enhanced notification statistics
        if (services.All(s => s.ServiceType != typeof(ISubscriberTracker)))
        {
            services.AddSingleton<ISubscriberTracker, SubscriberTracker>();
        }

        if (services.All(s => s.ServiceType != typeof(INotificationPatternDetector)))
        {
            services.AddSingleton<INotificationPatternDetector, NotificationPatternDetector>();
        }

        if (shouldRegisterStatistics && services.All(s => s.ServiceType != typeof(IStatisticsRenderer)))
        {
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
        }

        if (shouldRegisterStatistics && services.All(s => s.ServiceType != typeof(MediatorStatistics)))
        {
            services.AddSingleton<MediatorStatistics>(provider =>
            {
                var renderer = provider.GetRequiredService<IStatisticsRenderer>();
                var statisticsOptions = hasStatisticsOptions ? configuration.StatisticsOptions : null;
                var baseLogger = provider.GetService<ILogger<Mediator>>();
                var loggingOptions = provider.GetService<LoggingOptions>();
                // Only create MediatorLogger if logging options are configured (not null)
                var mediatorLogger = baseLogger != null && loggingOptions != null 
                    ? new MediatorLogger(baseLogger, loggingOptions) 
                    : null;
                return new MediatorStatistics(renderer, statisticsOptions, mediatorLogger);
            });
        }
    }

    /// <summary>
    /// Registers Mediator with conditional statistics dependency.
    /// </summary>
    private static void RegisterMediator(IServiceCollection services, bool shouldRegisterStatistics)
    {
        // Only register IMediator if it's not already registered
        if (services.All(s => s.ServiceType != typeof(IMediator)))
        {
            services.AddScoped<IMediator>(provider =>
            {
                var pipelineBuilder = provider.GetRequiredService<IMiddlewarePipelineBuilder>();
                var notificationPipelineBuilder = provider.GetRequiredService<INotificationPipelineBuilder>();
                var statistics = shouldRegisterStatistics ? provider.GetRequiredService<MediatorStatistics>() : null;
                var telemetryOptions = provider.GetService<TelemetryOptions>();
                var baseLogger = provider.GetService<ILogger<Mediator>>();
                var loggingOptions = provider.GetService<LoggingOptions>();
                // Only create MediatorLogger if logging options are configured (not null)
                var granularLogger = baseLogger != null && loggingOptions != null 
                    ? new MediatorLogger(baseLogger, loggingOptions) 
                    : null;
                return new Mediator(provider, pipelineBuilder, notificationPipelineBuilder, statistics, telemetryOptions, granularLogger);
            });
        }
    }

    /// <summary>
    /// Registers middleware from the specified assemblies with enhanced support for constrained middleware.
    /// Optimized with pre-cached assembly scanning.
    /// </summary>
    private static void RegisterMiddleware(IServiceCollection services, MediatorConfiguration configuration, Assembly[]? assemblies, bool finalDiscoverMiddleware, bool finalDiscoverNotificationMiddleware)
    {
        if (assemblies?.Length > 0 && (finalDiscoverMiddleware || finalDiscoverNotificationMiddleware))
        {
            // Deduplicate assemblies to prevent duplicate registrations  
            Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

            foreach (Assembly assembly in uniqueAssemblies)
            {
                RegisterMiddlewareFromAssembly(services, configuration, assembly, finalDiscoverMiddleware, finalDiscoverNotificationMiddleware);
            }
        }
    }

    /// <summary>
    /// Registers pipeline components (configuration, builders, and inspectors).
    /// </summary>
    private static void RegisterPipelineComponents(IServiceCollection services, MediatorConfiguration configuration)
    {
        // Only register the configuration if it's not already registered
        if (services.All(s => s.ServiceType != typeof(MediatorConfiguration)))
        {
            services.AddSingleton(configuration);
        }

        // Only register pipeline builder if not already registered
        if (services.All(s => s.ServiceType != typeof(IMiddlewarePipelineBuilder)))
        {
            services.AddScoped<IMiddlewarePipelineBuilder>(provider =>
            {
                var config = provider.GetRequiredService<MediatorConfiguration>();
                return config.PipelineBuilder; // Return the actual config instance
            });
        }

        // Only register notification pipeline builder if not already registered
        if (services.All(s => s.ServiceType != typeof(INotificationPipelineBuilder)))
        {
            services.AddScoped<INotificationPipelineBuilder>(provider =>
            {
                var config = provider.GetRequiredService<MediatorConfiguration>();
                return config.NotificationPipelineBuilder; // Return the actual config instance
            });
        }

        // Register pipeline inspector for debugging (same instance as pipeline builder)
        if (services.All(s => s.ServiceType != typeof(IMiddlewarePipelineInspector)))
        {
            services.AddScoped(provider =>
                provider.GetRequiredService<IMiddlewarePipelineBuilder>() as IMiddlewarePipelineInspector
                ?? throw new InvalidOperationException("Pipeline builder must implement IMiddlewarePipelineInspector"));
        }

        // Register notification pipeline inspector for debugging (same instance as notification pipeline builder)
        if (services.All(s => s.ServiceType != typeof(INotificationMiddlewarePipelineInspector)))
        {
            services.AddScoped(provider =>
                provider.GetRequiredService<INotificationPipelineBuilder>() as INotificationMiddlewarePipelineInspector
                ?? throw new InvalidOperationException("Notification pipeline builder must implement INotificationMiddlewarePipelineInspector"));
        }
    }

    /// <summary>
    /// Registers handlers from the specified assemblies.
    /// Optimized with pre-cached handler discovery.
    /// </summary>
    private static void RegisterHandlers(IServiceCollection services, Assembly[]? assemblies, bool discoverNotificationHandlers)
    {
        if (assemblies?.Length > 0)
        {
            // Deduplicate assemblies to prevent duplicate registrations  
            Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

            foreach (Assembly assembly in uniqueAssemblies)
            {
                // Pass the configuration's DiscoverNotificationHandlers setting to RegisterHandlers
                RegisterHandlersFromAssembly(services, assembly, discoverNotificationHandlers);
            }
        }
    }

    #region Compile-Time Optimizations

    /// <summary>
    /// Pre-warms assembly type cache for better performance.
    /// This compile-time optimization reduces reflection overhead during registration.
    /// </summary>
    /// <param name="assemblies">Assemblies to pre-warm cache for.</param>
    private static void PreWarmAssemblyTypeCache(Assembly[] assemblies)
    {
        Parallel.ForEach(assemblies, assembly =>
        {
            if (!_assemblyTypeCache.ContainsKey(assembly))
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t is { IsAbstract: false, IsInterface: false })
                        .ToFrozenSet();
                    
                    _assemblyTypeCache.TryAdd(assembly, types);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip assemblies that can't be loaded - cache empty set
                    _assemblyTypeCache.TryAdd(assembly, FrozenSet<Type>.Empty);
                }
            }
        });
    }

    /// <summary>
    /// Gets cached types from assembly for better performance.
    /// </summary>
    /// <param name="assembly">Assembly to get types from.</param>
    /// <returns>Cached frozen set of types.</returns>
    private static FrozenSet<Type> GetCachedAssemblyTypes(Assembly assembly)
    {
        if (_assemblyTypeCache.TryGetValue(assembly, out var cachedTypes))
        {
            return cachedTypes;
        }

        // Fallback to live loading if not cached
        try
        {
            var types = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .ToFrozenSet();
            
            _assemblyTypeCache.TryAdd(assembly, types);
            return types;
        }
        catch (ReflectionTypeLoadException)
        {
            var emptySet = FrozenSet<Type>.Empty;
            _assemblyTypeCache.TryAdd(assembly, emptySet);
            return emptySet;
        }
    }

    /// <summary>
    /// Gets cached middleware types from assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for middleware types.</param>
    /// <returns>Collection of middleware types found.</returns>
    private static IEnumerable<Type> GetCachedMiddlewareTypes(Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = GetCachedAssemblyTypes(assembly);
            foreach (var type in types)
            {
                if (IsMiddlewareTypeCached(type))
                {
                    yield return type;
                }
            }
        }
    }

    /// <summary>
    /// Cached check if type is middleware using pre-calculated interface lookups.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if type is middleware.</returns>
    private static bool IsMiddlewareTypeCached(Type type)
    {
        if (_interfaceCache.TryGetValue(type, out var cachedInterfaces))
        {
            return cachedInterfaces.Any(IsMiddlewareInterface);
        }

        var interfaces = type.GetInterfaces().ToFrozenSet();
        _interfaceCache.TryAdd(type, interfaces);
        
        return interfaces.Any(IsMiddlewareInterface);
    }

    /// <summary>
    /// Optimized middleware interface check using frozen dictionary lookups.
    /// </summary>
    /// <param name="interfaceType">Interface type to check.</param>
    /// <returns>True if interface is a middleware interface.</returns>
    private static bool IsMiddlewareInterface(Type interfaceType)
    {
        if (interfaceType.IsGenericType)
        {
            var genericDefinition = interfaceType.GetGenericTypeDefinition();
            var genericDefName = genericDefinition.Name;
            
            return _commonMiddlewareInterfaces.ContainsKey(genericDefName) &&
                   _commonMiddlewareInterfaces[genericDefName] == genericDefinition;
        }
        
        return _commonMiddlewareInterfaces.ContainsKey(interfaceType.Name) &&
               _commonMiddlewareInterfaces[interfaceType.Name] == interfaceType;
    }

    #endregion

    /// <summary>  
    /// Registers middleware from a single assembly with enhanced support for type-constrained notification middleware.
    /// Enhanced with registration-time pre-caching for optimal runtime performance.
    /// Optimized with compile-time caching and frozen collections.
    /// </summary>  
    /// <param name="services">The service collection for DI registration.</param>
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assembly">Assembly to scan for middleware.</param>  
    /// <param name="discoverMiddleware">Whether to discover request middleware.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to discover notification middleware.</param>  
    internal static void RegisterMiddlewareFromAssembly(IServiceCollection services, MediatorConfiguration configuration, Assembly assembly, bool discoverMiddleware = true, bool discoverNotificationMiddleware = true)
    {
        try
        {
            // Use cached assembly types for better performance
            var assemblyTypes = GetCachedAssemblyTypes(assembly);
            
            List<Type> middlewareTypes = assemblyTypes
                .Where(t => t.GetInterfaces().Any(i => IsMiddlewareType(i, discoverMiddleware, discoverNotificationMiddleware)))
                .ToList();

            foreach (Type middlewareType in middlewareTypes)
            {
                // Pre-cache middleware metadata during registration for runtime performance
                PreCacheMiddlewareMetadata(middlewareType);

                // Enhanced discovery for type-constrained notification middleware
                var notificationMiddlewareInterfaces = middlewareType.GetInterfaces()
                    .Where(i => i == typeof(INotificationMiddleware) ||
                               i == typeof(IConditionalNotificationMiddleware) ||
                               (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>) && discoverMiddleware))
                    .ToList();

                bool isNotificationMiddleware = notificationMiddlewareInterfaces.Any();

                if (isNotificationMiddleware)
                {
                    // Register the middleware type in DI if not already registered
                    if (services.All(s => s.ImplementationType != middlewareType))
                    {
                        services.AddScoped(middlewareType);
                    }

                    // Add to configuration pipeline with optimized middleware info
                    configuration.AddNotificationMiddleware(middlewareType);
                }
                else
                {
                    // Register the middleware type in DI if not already registered
                    if (services.All(s => s.ImplementationType != middlewareType))
                    {
                        services.AddScoped(middlewareType);
                    }

                    configuration.AddMiddleware(middlewareType);
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Skip assemblies that can't be loaded completely
            // This can happen with reference assemblies or incomplete dependencies
            throw new InvalidOperationException($"Failed to load types from assembly '{assembly.FullName}'. " +
                $"Loader exceptions: {string.Join(", ", ex.LoaderExceptions.Select(e => e?.Message))}", ex);
        }

        return;

        static bool IsMiddlewareType(Type i, bool includeRequestMiddleware, bool includeNotificationMiddleware) =>
            (includeRequestMiddleware && i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalStreamRequestMiddleware<,>))) ||
             (includeNotificationMiddleware &&
             (i == typeof(INotificationMiddleware) ||
             i == typeof(IConditionalNotificationMiddleware)));
    }

    /// <summary>
    /// Pre-caches middleware metadata during DI registration to minimize runtime reflection calls.
    /// This is a high-impact performance optimization that moves expensive operations from runtime to startup.
    /// Enhanced with comprehensive compile-time metadata caching.
    /// </summary>
    /// <param name="middlewareType">The middleware type to cache metadata for.</param>
    private static void PreCacheMiddlewareMetadata(Type middlewareType)
    {
        if (_middlewareCache.ContainsKey(middlewareType))
        {
            return; // Already cached
        }

        // Comprehensive metadata pre-calculation
        var interfaces = middlewareType.GetInterfaces();
        var interfaceNames = interfaces.Select(PipelineUtilities.FormatTypeName).ToArray();
        var interfacePatterns = interfaces.Select(i => i.IsGenericType ? i.GetGenericTypeDefinition().Name : i.Name).ToFrozenSet();
        
        var metadata = new MiddlewareMetadata(
            Order: CalculateOrderOnce(middlewareType),
            CleanTypeName: PipelineUtilities.GetCleanTypeName(middlewareType),
            InterfaceNames: interfaceNames,
            IsGenericTypeDefinition: middlewareType.IsGenericTypeDefinition,
            GenericConstraints: GetConstraintsOnce(middlewareType),
            // Additional pre-calculated properties
            IsConditionalMiddleware: IsConditionalMiddleware(interfaces),
            IsNotificationMiddleware: IsNotificationMiddleware(interfaces),
            IsStreamMiddleware: IsStreamMiddleware(interfaces),
            ConcreteImplementationType: middlewareType.IsGenericTypeDefinition ? null : middlewareType,
            CachedInterfacePatterns: interfacePatterns
        );

        _middlewareCache.TryAdd(middlewareType, metadata);
    }

    /// <summary>
    /// Pre-caches handler metadata during registration.
    /// </summary>
    /// <param name="handlerType">The handler type to cache metadata for.</param>
    private static void PreCacheHandlerMetadata(Type handlerType)
    {
        if (_handlerCache.ContainsKey(handlerType))
        {
            return; // Already cached
        }

        var interfaces = handlerType.GetInterfaces().ToFrozenSet();
        var requestTypes = interfaces
            .Where(i => i.IsGenericType)
            .SelectMany(i => i.GetGenericArguments())
            .Distinct()
            .ToArray();

        var metadata = new HandlerMetadata(
            CleanTypeName: PipelineUtilities.GetCleanTypeName(handlerType),
            IsNotificationHandler: interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)),
            IsStreamHandler: interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)),
            SupportedRequestTypes: requestTypes,
            CachedInterfaces: interfaces
        );

        _handlerCache.TryAdd(handlerType, metadata);
    }

    /// <summary>
    /// Pre-calculates if middleware is conditional.
    /// </summary>
    private static bool IsConditionalMiddleware(Type[] interfaces)
    {
        return interfaces.Any(i => i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalStreamRequestMiddleware<,>))) ||
            interfaces.Contains(typeof(IConditionalNotificationMiddleware));
    }

    /// <summary>
    /// Pre-calculates if middleware is notification middleware.
    /// </summary>
    private static bool IsNotificationMiddleware(Type[] interfaces)
    {
        return interfaces.Contains(typeof(INotificationMiddleware)) ||
               interfaces.Contains(typeof(IConditionalNotificationMiddleware)) ||
               interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>));
    }

    /// <summary>
    /// Pre-calculates if middleware is stream middleware.
    /// </summary>
    private static bool IsStreamMiddleware(Type[] interfaces)
    {
        return interfaces.Any(i => i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalStreamRequestMiddleware<,>)));
    }

    /// <summary>
    /// Pre-calculates middleware order at registration time.
    /// </summary>
    private static int CalculateOrderOnce(Type middlewareType)
    {
        // Try to get order from a static Order property or field first
        var orderProperty = middlewareType.GetProperty("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderProperty != null && orderProperty.PropertyType == typeof(int))
        {
            return (int)orderProperty.GetValue(null)!;
        }

        var orderField = middlewareType.GetField("Order", BindingFlags.Public | BindingFlags.Static);
        if (orderField != null && orderField.FieldType == typeof(int))
        {
            return (int)orderField.GetValue(null)!;
        }

        // Check for OrderAttribute if it exists (common pattern)
        var orderAttribute = middlewareType.GetCustomAttributes(false)
            .FirstOrDefault(attr => attr.GetType().Name == "OrderAttribute");
        if (orderAttribute != null)
        {
            var orderProp = orderAttribute.GetType().GetProperty("Order");
            if (orderProp != null && orderProp.PropertyType == typeof(int))
            {
                return (int)orderProp.GetValue(orderAttribute)!;
            }
        }

        // For fallback ordering, use a placeholder that will be resolved at runtime
        return int.MaxValue - 1000000;
    }

    /// <summary>
    /// Pre-calculates interface names at registration time.
    /// </summary>
    private static string[] GetInterfaceNamesOnce(Type middlewareType)
    {
        return middlewareType.GetInterfaces()
            .Select(PipelineUtilities.FormatTypeName)
            .ToArray();
    }

    /// <summary>
    /// Pre-analyzes generic constraints at registration time.
    /// </summary>
    private static Type[]? GetConstraintsOnce(Type middlewareType)
    {
        if (!middlewareType.IsGenericTypeDefinition)
            return null;

        var genericParameters = middlewareType.GetGenericArguments();
        return genericParameters.SelectMany(p => p.GetGenericParameterConstraints()).ToArray();
    }

    /// <summary>  
    /// Registers handler types from the specified assembly into the service collection.  
    /// Implements enhanced assembly scanning for INotificationHandler with proper error handling.
    /// Optimized with pre-cached handler discovery and metadata caching.
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assembly">The assembly to scan for handler types.</param>  
    /// <param name="discoverNotificationHandlers">Whether to discover and register INotificationHandler implementations.</param>
    internal static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly, bool discoverNotificationHandlers = true)
    {
        try
        {
            // Use cached assembly types for better performance
            var assemblyTypes = GetCachedAssemblyTypes(assembly);
            
            List<Type> handlerTypes = assemblyTypes
                .Where(t => t.GetInterfaces().Any(i => IsHandlerType(i, discoverNotificationHandlers)))
                .ToList();

            // Group handlers by interface type to handle multiple implementations
            var handlersByInterface = new Dictionary<Type, List<Type>>();
            
            foreach (Type handlerType in handlerTypes)
            {
                // Pre-cache handler metadata
                PreCacheHandlerMetadata(handlerType);

                // First register the handler type itself as scoped  
                if (services.All(s => s.ImplementationType != handlerType))
                {
                    services.AddScoped(handlerType);
                }

                // Group handlers by their interface types
                var interfaces = handlerType.GetInterfaces()
                    .Where(i => IsHandlerType(i, discoverNotificationHandlers))
                    .ToList();

                foreach (var @interface in interfaces)
                {
                    if (!handlersByInterface.ContainsKey(@interface))
                    {
                        handlersByInterface[@interface] = new List<Type>();
                    }
                    handlersByInterface[@interface].Add(handlerType);
                }
            }

            // Register the best handler for each interface
            foreach (var kvp in handlersByInterface)
            {
                var interfaceType = kvp.Key;
                var implementationTypes = kvp.Value;

                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                {
                    // For notification handlers, register all implementations (multiple handlers per notification)
                    foreach (var handlerType in implementationTypes)
                    {
                        services.AddScoped(interfaceType, serviceProvider => serviceProvider.GetRequiredService(handlerType));
                    }
                }
                else
                {
                    // For request handlers, select the best implementation (single handler per request)
                    var bestHandler = SelectPrimaryHandler(implementationTypes);
                    
                    // Only register if no implementation exists yet
                    if (services.All(s => s.ServiceType != interfaceType))
                    {
                        services.AddScoped(interfaceType, serviceProvider => serviceProvider.GetRequiredService(bestHandler));
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Enhanced error handling for assembly scanning
            // Skip assemblies that can't be loaded
            // This can happen with reference assemblies or incomplete dependencies
            throw new InvalidOperationException($"Failed to load types from assembly '{assembly.FullName}' during handler discovery. " +
                $"Loader exceptions: {string.Join(", ", ex.LoaderExceptions.Select(e => e?.Message))}", ex);
        }
        catch (Exception ex)
        {
            // General error handling for unexpected issues during scanning
            throw new InvalidOperationException($"Unexpected error occurred while scanning assembly '{assembly.FullName}' for handlers: {ex.Message}", ex);
        }

        return;

        static bool IsHandlerType(Type i, bool includeNotificationHandlers) =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>) ||
             (includeNotificationHandlers && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)));
    }

    /// <summary>
    /// Selects the primary handler from a list of handler implementations.
    /// Prioritizes handlers from the Handlers namespace/directory over test handlers.
    /// </summary>
    /// <param name="handlerTypes">List of handler types to choose from.</param>
    /// <returns>The primary handler type to register.</returns>
    private static Type SelectPrimaryHandler(List<Type> handlerTypes)
    {
        if (handlerTypes.Count == 1)
        {
            return handlerTypes[0];
        }

        // Priority 1: Handlers in ".Handlers" namespace (main handlers)
        var mainHandlers = handlerTypes.Where(t => t.Namespace != null && t.Namespace.Contains(".Handlers")).ToList();
        if (mainHandlers.Count > 0)
        {
            return mainHandlers.First();
        }

        // Priority 2: Handlers not in ".Tests" namespace (non-test handlers)
        var nonTestHandlers = handlerTypes.Where(t => t.Namespace != null && !t.Namespace.Contains(".Tests")).ToList();
        if (nonTestHandlers.Count > 0)
        {
            return nonTestHandlers.First();
        }

        // Priority 3: Handlers that don't start with "Second" or "Third" (primary handlers)
        var primaryHandlers = handlerTypes.Where(t => !t.Name.StartsWith("Second") && !t.Name.StartsWith("Third")).ToList();
        if (primaryHandlers.Count > 0)
        {
            return primaryHandlers.First();
        }

        // Priority 4: Check if there's a naming convention match (e.g., TestCommandHandler for TestCommand)
        var conventionBasedHandlers = handlerTypes.Where(t => 
        {
            // Extract request type from handler interfaces
            var handlerInterfaces = t.GetInterfaces()
                .Where(i => i.IsGenericType && 
                           (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                            i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)));
            
            foreach (var @interface in handlerInterfaces)
            {
                var requestType = @interface.GetGenericArguments()[0];
                var expectedHandlerName = $"{requestType.Name}Handler";
                if (t.Name.Equals(expectedHandlerName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }).ToList();
        
        if (conventionBasedHandlers.Count > 0)
        {
            return conventionBasedHandlers.First();
        }

        // Fallback: Return the first handler (deterministic behavior)
        return handlerTypes.First();
    }
}