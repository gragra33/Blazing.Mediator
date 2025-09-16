using Blazing.Mediator.Statistics;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator;

/// <summary>  
/// Provides extension methods for registering Mediator services in the dependency injection container.  
/// </summary>  
public static class ServiceCollectionExtensions
{
    #region AddMediator Methods

    /// <summary>  
    /// Adds mediator services with minimal configuration.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        return AddMediatorCore(services, null, false, false, false, null, null);
    }

    /// <summary>  
    /// Adds mediator services and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [OverloadResolutionPriority(-1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? Array.Empty<Assembly>();
        return AddMediatorCore(services, null, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, bool discoverMiddleware, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? Array.Empty<Assembly>();
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic notification middleware discovery from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from assemblies.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorWithNotificationMiddleware(this IServiceCollection services, bool discoverNotificationMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, false, false, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic notification middleware discovery from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from assemblies.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorWithNotificationMiddleware(this IServiceCollection services, bool discoverNotificationMiddleware, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? Array.Empty<Assembly>();
        return AddMediatorCore(services, null, false, false, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, configureMiddleware, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool enableStatisticsTracking, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, configureMiddleware, enableStatisticsTracking, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? Array.Empty<Assembly>();
        return AddMediatorCore(services, configureMiddleware, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool enableStatisticsTracking, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? Array.Empty<Assembly>();
        return AddMediatorCore(services, configureMiddleware, enableStatisticsTracking, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register request middleware from assemblies. Defaults to false if null.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from assemblies. Defaults to false if null.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, configureMiddleware, false, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="discoverMiddleware">Whether to automatically discover and register request middleware from assemblies. Defaults to false if null.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from assemblies. Defaults to false if null.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool enableStatisticsTracking, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, configureMiddleware, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery, statistics tracking, and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="discoverMiddleware">Whether to automatically discover and register request middleware from assemblies.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, bool enableStatisticsTracking, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, enableStatisticsTracking, discoverMiddleware, discoverMiddleware, assemblies, null);
    }
    #endregion

    #region Private Core Implementation

    /// <summary>
    /// Core implementation for all AddMediator methods to prevent recursion.
    /// </summary>
    private static IServiceCollection AddMediatorCore(
        IServiceCollection services,
        Action<MediatorConfiguration>? configureMiddleware,
        bool enableStatisticsTracking,
        bool? discoverMiddleware,
        bool? discoverNotificationMiddleware,
        Assembly[]? assemblies,
        Assembly? callingAssembly)
    {
        // Convert null values to false for backward compatibility
        bool actualDiscoverMiddleware = discoverMiddleware ?? false;
        bool actualDiscoverNotificationMiddleware = discoverNotificationMiddleware ?? false;

        // Resolve assemblies based on what was provided
        Assembly[] targetAssemblies;
        if (assemblies is { Length: > 0 })
        {
            targetAssemblies = assemblies;
        }
        else if (callingAssembly != null)
        {
            targetAssemblies = new[] { callingAssembly };
        }
        else
        {
            targetAssemblies = Array.Empty<Assembly>();
        }

        // Configure middleware if provided
        var configuration = new MediatorConfiguration(services);
        configureMiddleware?.Invoke(configuration);

        // Determine final statistics tracking setting - prioritize configuration over parameter
        bool finalEnableStatisticsTracking = configuration.EnableStatisticsTracking || enableStatisticsTracking;

        // Register MediatorStatistics with default console renderer if not already registered and statistics tracking is enabled
        if (finalEnableStatisticsTracking && services.All(s => s.ServiceType != typeof(IStatisticsRenderer)))
        {
            services.AddSingleton<IStatisticsRenderer, ConsoleStatisticsRenderer>();
        }

        if (finalEnableStatisticsTracking && services.All(s => s.ServiceType != typeof(MediatorStatistics)))
        {
            services.AddSingleton<MediatorStatistics>();
        }

        // Register Mediator with conditional statistics dependency
        services.AddScoped<IMediator>(provider =>
        {
            var pipelineBuilder = provider.GetRequiredService<IMiddlewarePipelineBuilder>();
            var notificationPipelineBuilder = provider.GetRequiredService<INotificationPipelineBuilder>();
            var statistics = finalEnableStatisticsTracking ? provider.GetRequiredService<MediatorStatistics>() : null;
            return new Mediator(provider, pipelineBuilder, notificationPipelineBuilder, statistics);
        });

        if (actualDiscoverMiddleware || actualDiscoverNotificationMiddleware)
        {
            RegisterMiddleware(configuration, targetAssemblies, actualDiscoverMiddleware, actualDiscoverNotificationMiddleware);
        }

        services.AddSingleton(configuration);

        // Register the configured pipeline builder as the scoped pipeline builder
        services.AddScoped(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);

        // Register the configured notification pipeline builder as the scoped notification pipeline builder
        services.AddScoped(provider =>
            provider.GetRequiredService<MediatorConfiguration>().NotificationPipelineBuilder);

        // Register pipeline inspector for debugging (same instance as pipeline builder)
        services.AddScoped(provider =>
            provider.GetRequiredService<IMiddlewarePipelineBuilder>() as IMiddlewarePipelineInspector
            ?? throw new InvalidOperationException("Pipeline builder must implement IMiddlewarePipelineInspector"));

        // Register notification pipeline inspector for debugging (same instance as notification pipeline builder)
        services.AddScoped(provider =>
            provider.GetRequiredService<INotificationPipelineBuilder>() as INotificationMiddlewarePipelineInspector
            ?? throw new InvalidOperationException("Notification pipeline builder must implement INotificationMiddlewarePipelineInspector"));

        if (targetAssemblies.Length > 0)
        {
            // Deduplicate assemblies to prevent duplicate registrations  
            Assembly[] uniqueAssemblies = targetAssemblies.Distinct().ToArray();

            foreach (Assembly assembly in uniqueAssemblies)
            {
                RegisterHandlers(services, assembly);
            }
        }

        return services;
    }

    #endregion

    #region AddMediatorFromCallingAssembly Methods

    /// <summary>  
    /// Adds mediator services and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, null, false, false, false, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from the assembly.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, bool discoverMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Action to configure middleware pipeline.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration> configureMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, configureMiddleware, false, false, false, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from the assembly.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool discoverMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, configureMiddleware, false, discoverMiddleware, discoverMiddleware, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="discoverMiddleware">Whether to automatically discover and register request middleware from the assembly.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from the assembly.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool enableStatisticsTracking, bool discoverMiddleware, bool discoverNotificationMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, configureMiddleware, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, null, callingAssembly);
    }

    #endregion

    #region AddMediatorFromLoadedAssemblies Methods

    /// <summary>  
    /// Adds mediator services and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return AddMediatorCore(services, null, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblyFilter">Assembly filter function.</param>
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Func<Assembly, bool> assemblyFilter)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        assemblies = assemblies.Where(assemblyFilter).ToArray();
        return AddMediatorCore(services, null, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from all loaded assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, bool discoverMiddleware)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Action to configure middleware pipeline.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration> configureMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, configureMiddleware, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool discoverMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, configureMiddleware, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">Whether to enable statistics tracking for Send commands.</param>
    /// <param name="discoverMiddleware">Whether to automatically discover and register request middleware from assemblies.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to automatically discover and register notification middleware from assemblies.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool enableStatisticsTracking, bool discoverMiddleware, bool discoverNotificationMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, configureMiddleware, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>  
    /// Registers middleware from the specified assemblies.  
    /// </summary>  
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assemblies">Assemblies to scan for middleware.</param>  
    /// <param name="discoverMiddleware">Whether to discover request middleware.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to discover notification middleware.</param>  
    private static void RegisterMiddleware(MediatorConfiguration configuration, Assembly[] assemblies, bool discoverMiddleware = true, bool discoverNotificationMiddleware = true)
    {
        // Deduplicate assemblies
        Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

        foreach (Assembly assembly in uniqueAssemblies)
        {
            RegisterMiddlewareFromAssembly(configuration, assembly, discoverMiddleware, discoverNotificationMiddleware);
        }
    }

    /// <summary>  
    /// Registers middleware from a single assembly.  
    /// </summary>  
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assembly">Assembly to scan for middleware.</param>  
    /// <param name="discoverMiddleware">Whether to discover request middleware.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to discover notification middleware.</param>  
    private static void RegisterMiddlewareFromAssembly(MediatorConfiguration configuration, Assembly assembly, bool discoverMiddleware = true, bool discoverNotificationMiddleware = true)
    {
        List<Type> middlewareTypes = assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                t.GetInterfaces().Any(i => IsMiddlewareType(i, discoverMiddleware, discoverNotificationMiddleware)))
            .ToList();

        foreach (Type middlewareType in middlewareTypes)
        {
            // Check if it's a notification middleware type
            bool isNotificationMiddleware = middlewareType.GetInterfaces().Any(i =>
                i == typeof(INotificationMiddleware) ||
                i == typeof(IConditionalNotificationMiddleware));

            if (isNotificationMiddleware)
            {
                configuration.AddNotificationMiddleware(middlewareType);
            }
            else
            {
                configuration.AddMiddleware(middlewareType);
            }
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
    /// Registers handler types from the specified assembly into the service collection.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assembly">The assembly to scan for handler types.</param>  
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        List<Type> handlerTypes = assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                t.GetInterfaces().Any(IsHandlerType))
            .ToList();

        foreach (Type handlerType in handlerTypes)
        {
            // First register the handler type itself as scoped  
            if (services.All(s => s.ImplementationType != handlerType))
            {
                services.AddScoped(handlerType);
            }

            // Register all handler interfaces - but skip if ANY implementation is already registered
            // This prevents multiple handlers for the same request type
            handlerType.GetInterfaces()
                .Where(IsHandlerType)
                .Where(@interface => !services.Any(s => s.ServiceType == @interface)) // Changed: check if ANY handler for this interface exists
                .ToList()
                .ForEach(@interface => services.AddScoped(@interface, serviceProvider => serviceProvider.GetRequiredService(handlerType)));
        }

        return;

        static bool IsHandlerType(Type i) =>
        i.IsGenericType &&
        (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
         i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
         i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>));

    }

    #endregion
}
