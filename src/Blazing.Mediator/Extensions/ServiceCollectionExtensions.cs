using Blazing.Mediator.Services;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator;

/// <summary>
/// Extension methods for IServiceCollection to register Blazing.Mediator services.
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
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? [];
        return AddMediatorCore(services, null, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, false, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [OverloadResolutionPriority(0)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? [];
        return AddMediatorCore(services, options, false, false, false, assemblies, null);
    }

    /// <summary>
    /// Adds Blazing.Mediator services with automatic static resource cleanup service.
    /// The cleanup service can be manually called during application shutdown to dispose static OpenTelemetry resources.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Configuration options for the mediator.</param>
    /// <param name="enableStaticResourceCleanup">Whether to register a cleanup service for manual disposal of static resources.</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorWithStaticCleanup(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStaticResourceCleanup, params Assembly[]? assemblies)
    {
        // Register core mediator services
        services.AddMediator(options, assemblies);

        // Optionally register cleanup service for manual disposal
        if (enableStaticResourceCleanup)
        {
            services.AddSingleton<MediatorCleanupService>();
        }

        return services;
    }

    /// <summary>
    /// Adds Blazing.Mediator services with automatic static resource cleanup service.
    /// The cleanup service can be manually called during application shutdown to dispose static OpenTelemetry resources.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="options">Configuration options for the mediator.</param>
    /// <param name="enableStaticResourceCleanup">Whether to register a cleanup service for manual disposal of static resources.</param>
    /// <param name="assemblyMarkerTypes">Types from assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    [OverloadResolutionPriority(0)]
    public static IServiceCollection AddMediatorWithStaticCleanup(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStaticResourceCleanup, params Type[]? assemblyMarkerTypes)
    {
        // Register core mediator services
        services.AddMediator(options, assemblyMarkerTypes);

        // Optionally register cleanup service for manual disposal
        if (enableStaticResourceCleanup)
        {
            services.AddSingleton<MediatorCleanupService>();
        }

        return services;
    }

    #endregion

    #region Obsolete AddMediator Overloads

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
    [OverloadResolutionPriority(0)]
    public static IServiceCollection AddMediator(this IServiceCollection services, bool discoverMiddleware, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? [];
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic notification middleware discovery from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorWithNotificationMiddleware(this IServiceCollection services, bool discoverNotificationMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, false, false, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with automatic notification middleware discovery from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.")]
    [OverloadResolutionPriority(0)]
    public static IServiceCollection AddMediatorWithNotificationMiddleware(this IServiceCollection services, bool discoverNotificationMiddleware, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? [];
        return AddMediatorCore(services, null, false, false, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithStatisticsTracking() instead.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, enableStatisticsTracking, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with middleware configuration and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithStatisticsTracking() instead.")]
    [OverloadResolutionPriority(0)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, params Type[]? assemblyMarkerTypes)
    {
        Assembly[] assemblies = assemblyMarkerTypes?.Select(t => t.Assembly).Distinct().ToArray() ?? [];
        return AddMediatorCore(services, options, enableStatisticsTracking, false, false, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration fluent methods instead.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, false, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() ??.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() ??.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration fluent methods ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery, statistics tracking, and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() ??.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration fluent methods ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediator(this IServiceCollection services, bool enableStatisticsTracking, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, enableStatisticsTracking, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    #endregion

    #region Private Core Implementation

    /// <summary>
    /// Core implementation for all AddMediator methods to prevent recursion.
    /// Uses the MediatorRegistrationService to handle the actual registration logic.
    /// </summary>
    private static IServiceCollection AddMediatorCore(
        IServiceCollection services,
        Action<MediatorConfiguration>? options,
        bool enableStatisticsTracking,
        bool? discoverMiddleware,
        bool? discoverNotificationMiddleware,
        Assembly[]? assemblies,
        Assembly? callingAssembly)
    {
        // Delegate all logic to the MediatorRegistrationService
        return MediatorRegistrationService.RegisterMediatorServices(
            services,
            options,
            enableStatisticsTracking,
            discoverMiddleware,
            discoverNotificationMiddleware,
            assemblies,
            callingAssembly);
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
    /// Adds mediator services with middleware configuration and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Action to configure middleware pipeline.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration> options)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, options, false, false, false, null, callingAssembly);
    }

    #region Obsolete AddMediatorFromCallingAssembly Overloads

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, bool discoverMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration>? options, bool discoverMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, options, false, discoverMiddleware, discoverMiddleware, null, callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() ??.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from the assembly. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() ??.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration fluent methods ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, bool discoverMiddleware, bool discoverNotificationMiddleware)
    {
        Assembly callingAssembly = Assembly.GetCallingAssembly();
        return AddMediatorCore(services, options, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, null, callingAssembly);
    }

    #endregion

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
    /// Adds mediator services with middleware configuration and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Action to configure middleware pipeline.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration> options, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, options, false, false, false, assemblies, null);
    }

    #region Obsolete AddMediatorFromLoadedAssemblies Overloads

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from all loaded assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, bool discoverMiddleware)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return AddMediatorCore(services, null, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration>? options, bool discoverMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, options, false, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() ??.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() ??.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() ??.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration fluent methods ??.")]
    [OverloadResolutionPriority(1)]
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, bool discoverMiddleware, bool discoverNotificationMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return AddMediatorCore(services, options, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    #endregion

    #endregion
}
