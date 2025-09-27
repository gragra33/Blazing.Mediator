using Blazing.Mediator.Statistics;
using Blazing.Mediator.OpenTelemetry;

namespace Blazing.Mediator.Services;

/// <summary>
/// Static service responsible for registering mediator services, handlers, and middleware in the dependency injection container.
/// This class encapsulates the core registration logic separated from the extension methods.
/// </summary>
internal static class MediatorRegistrationService
{
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

        return services;
    }

    /// <summary>
    /// Registers telemetry options if configured and not already registered.
    /// </summary>
    private static void RegisterTelemetryOptions(IServiceCollection services, MediatorConfiguration configuration, bool hasTelemetryOptions)
    {
        if (hasTelemetryOptions && services.All(s => s.ServiceType != typeof(MediatorTelemetryOptions)))
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
                var mediatorLogger = baseLogger != null ? new MediatorLogger(baseLogger, loggingOptions) : null;
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
                var telemetryOptions = provider.GetService<MediatorTelemetryOptions>();
                var baseLogger = provider.GetService<ILogger<Mediator>>();
                var loggingOptions = provider.GetService<LoggingOptions>();
                var granularLogger = baseLogger != null ? new MediatorLogger(baseLogger, loggingOptions) : null;
                return new Mediator(provider, pipelineBuilder, notificationPipelineBuilder, statistics, telemetryOptions, granularLogger);
            });
        }
    }

    /// <summary>
    /// Registers middleware from the specified assemblies with enhanced support for constrained middleware.
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
                var baseLogger = provider.GetService<ILogger<Mediator>>();
                var loggingOptions = provider.GetService<LoggingOptions>();
                var mediatorLogger = baseLogger != null ? new MediatorLogger(baseLogger, loggingOptions) : null;
                
                // Create a new instance with the logger instead of using the config's cached instance
                var pipelineBuilder = new MiddlewarePipelineBuilder(mediatorLogger);
                
                // Copy middleware registrations from the config's pipeline builder
                if (config.PipelineBuilder is IMiddlewarePipelineInspector configPipelineBuilder)
                {
                    var middlewareInfos = configPipelineBuilder.GetDetailedMiddlewareInfo();
                    foreach (var (type, _, o) in middlewareInfos)
                    {
                        if (o != null)
                        {
                            // Use reflection to call the generic method with configuration
                            var addMethod = typeof(MiddlewarePipelineBuilder).GetMethod("AddMiddleware", [typeof(object)]);
                            var genericMethod = addMethod?.MakeGenericMethod(type);
                            genericMethod?.Invoke(pipelineBuilder, [o]);
                        }
                        else
                        {
                            pipelineBuilder.AddMiddleware(type);
                        }
                    }
                }
                
                return pipelineBuilder;
            });
        }

        // Only register notification pipeline builder if not already registered
        if (services.All(s => s.ServiceType != typeof(INotificationPipelineBuilder)))
        {
            services.AddScoped<INotificationPipelineBuilder>(provider =>
            {
                var config = provider.GetRequiredService<MediatorConfiguration>();
                var baseLogger = provider.GetService<ILogger<Mediator>>();
                var loggingOptions = provider.GetService<LoggingOptions>();
                var mediatorLogger = baseLogger != null ? new MediatorLogger(baseLogger, loggingOptions) : null;
                
                // Create a new instance with the logger instead of using the config's cached instance
                var notificationPipelineBuilder = new NotificationPipelineBuilder(mediatorLogger);
                
                // Copy middleware registrations from the config's notification pipeline builder
                if (config.NotificationPipelineBuilder is INotificationMiddlewarePipelineInspector configNotificationPipelineBuilder)
                {
                    var middlewareInfos = configNotificationPipelineBuilder.GetDetailedMiddlewareInfo();
                    foreach (var (type, _, o) in middlewareInfos)
                    {
                        if (o != null)
                        {
                            // Use reflection to call the generic method with configuration
                            var addMethod = typeof(NotificationPipelineBuilder).GetMethod("AddMiddleware", [typeof(object)]);
                            var genericMethod = addMethod?.MakeGenericMethod(type);
                            genericMethod?.Invoke(notificationPipelineBuilder, [o]);
                        }
                        else
                        {
                            notificationPipelineBuilder.AddMiddleware(type);
                        }
                    }
                }
                
                return notificationPipelineBuilder;
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

    /// <summary>  
    /// Registers middleware from a single assembly with enhanced support for type-constrained notification middleware.
    /// </summary>  
    /// <param name="services">The service collection for DI registration.</param>
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assembly">Assembly to scan for middleware.</param>  
    /// <param name="discoverMiddleware">Whether to discover request middleware.</param>  
    /// <param name="discoverNotificationMiddleware">Whether to discover notification middleware.</param>  
    private static void RegisterMiddlewareFromAssembly(IServiceCollection services, MediatorConfiguration configuration, Assembly assembly, bool discoverMiddleware = true, bool discoverNotificationMiddleware = true)
    {
        try
        {
            List<Type> middlewareTypes = assembly.GetTypes()
                .Where(t =>
                    t is { IsAbstract: false, IsInterface: false } &&
                    t.GetInterfaces().Any(i => IsMiddlewareType(i, discoverMiddleware, discoverNotificationMiddleware)))
                .ToList();

            foreach (Type middlewareType in middlewareTypes)
            {
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

                    // Add to configuration pipeline
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
    /// Registers handler types from the specified assembly into the service collection.  
    /// Implements enhanced assembly scanning for INotificationHandler with proper error handling.
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assembly">The assembly to scan for handler types.</param>  
    /// <param name="discoverNotificationHandlers">Whether to discover and register INotificationHandler implementations.</param>
    private static void RegisterHandlersFromAssembly(IServiceCollection services, Assembly assembly, bool discoverNotificationHandlers = true)
    {
        try
        {
            // Enhanced assembly scanning implementation
            List<Type> handlerTypes = assembly.GetTypes()
                .Where(t =>
                    t is { IsAbstract: false, IsInterface: false } &&
                    t.GetInterfaces().Any(i => IsHandlerType(i, discoverNotificationHandlers)))
                .ToList();

            foreach (Type handlerType in handlerTypes)
            {
                // First register the handler type itself as scoped  
                if (services.All(s => s.ImplementationType != handlerType))
                {
                    services.AddScoped(handlerType);
                }

                // Register all handler interfaces
                handlerType.GetInterfaces()
                    .Where(i => IsHandlerType(i, discoverNotificationHandlers))
                    .ToList()
                    .ForEach(@interface => RegisterHandlerInterface(services, @interface, handlerType));
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

        static void RegisterHandlerInterface(IServiceCollection services, Type interfaceType, Type handlerType)
        {
            // Enhanced registration logic for INotificationHandler
            // For INotificationHandler, register as multiple implementations (multiple handlers per notification)
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            {
                // Always register notification handlers - multiple handlers per notification are allowed
                // Use factory pattern for better performance and DI integration
                services.AddScoped(interfaceType, serviceProvider => serviceProvider.GetRequiredService(handlerType));
            }
            else
            {
                // For request handlers, register only if no implementation exists (single handler per request)
                if (services.All(s => s.ServiceType != interfaceType))
                {
                    services.AddScoped(interfaceType, serviceProvider => serviceProvider.GetRequiredService(handlerType));
                }
            }
        }
    }
}