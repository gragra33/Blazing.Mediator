using Blazing.Mediator.Statistics;
using Blazing.Mediator.OpenTelemetry;
using Blazing.Mediator.Services;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Extensions;

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

    #region Obsolete AddMediator Overloads

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
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
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, false, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="options">Optional action to configure middleware pipeline.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration fluent methods instead.")]
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? options, bool enableStatisticsTracking, bool? discoverMiddleware = null, bool? discoverNotificationMiddleware = null, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, options, enableStatisticsTracking, discoverMiddleware, discoverNotificationMiddleware, assemblies, null);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery, statistics tracking, and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediator with configuration action and MediatorConfiguration fluent methods instead.")]
    public static IServiceCollection AddMediator(this IServiceCollection services, bool enableStatisticsTracking, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        return AddMediatorCore(services, null, enableStatisticsTracking, discoverMiddleware, discoverMiddleware, assemblies, null);
    }

    #endregion

    #endregion

    #region Private Core Implementation

    /// <summary>
    /// Core implementation for all AddMediator methods to prevent recursion.
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
            targetAssemblies = [callingAssembly];
        }
        else
        {
            targetAssemblies = [];
        }

        // Configure middleware if provided
        var configuration = new MediatorConfiguration(services);
        options?.Invoke(configuration);

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

        // Register telemetry options if configured and not already registered
        if (hasTelemetryOptions && services.All(s => s.ServiceType != typeof(MediatorTelemetryOptions)))
        {
            services.AddSingleton(configuration.TelemetryOptions!);
        }

        // Register logging options if configured and not already registered
        if (hasLoggingOptions && services.All(s => s.ServiceType != typeof(LoggingOptions)))
        {
            services.AddSingleton(configuration.LoggingOptions!);
        }

        // Register MediatorStatistics with default console renderer if not already registered and statistics tracking is enabled
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
                return new MediatorStatistics(renderer, statisticsOptions);
            });
        }

        // Register Mediator with conditional statistics dependency
        services.AddScoped<IMediator>(provider =>
        {
            var pipelineBuilder = provider.GetRequiredService<IMiddlewarePipelineBuilder>();
            var notificationPipelineBuilder = provider.GetRequiredService<INotificationPipelineBuilder>();
            var statistics = shouldRegisterStatistics ? provider.GetRequiredService<MediatorStatistics>() : null;
            var telemetryOptions = provider.GetService<MediatorTelemetryOptions>();
            var baseLogger = provider.GetService<ILogger<Mediator>>();
            var loggingOptions = provider.GetService<LoggingOptions>();
            var granularLogger = baseLogger != null ? new MediatorLogger(baseLogger, loggingOptions) : null;
            return new Mediator(provider, pipelineBuilder, notificationPipelineBuilder, statistics, telemetryOptions, granularLogger);
        });

        if (finalDiscoverMiddleware || finalDiscoverNotificationMiddleware)
        {
            RegisterMiddleware(configuration, targetAssemblies, finalDiscoverMiddleware, finalDiscoverNotificationMiddleware);
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
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
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
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
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
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from the assembly. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from the assembly. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromCallingAssembly with configuration action and MediatorConfiguration fluent methods instead.")]
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
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
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
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration.WithMiddlewareDiscovery() instead.")]
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
    /// <param name="enableStatisticsTracking">[Obsolete] Whether to enable statistics tracking for Send commands. Use MediatorConfiguration.WithStatisticsTracking() instead.</param>
    /// <param name="discoverMiddleware">[Obsolete] Whether to automatically discover and register request middleware from assemblies. Use MediatorConfiguration.WithMiddlewareDiscovery() instead.</param>  
    /// <param name="discoverNotificationMiddleware">[Obsolete] Whether to automatically discover and register notification middleware from assemblies. Use MediatorConfiguration.WithNotificationMiddlewareDiscovery() instead.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    [Obsolete("Use AddMediatorFromLoadedAssemblies with configuration action and MediatorConfiguration fluent methods instead.")]
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
                .Where(@interface => services.All(s => s.ServiceType != @interface))
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
