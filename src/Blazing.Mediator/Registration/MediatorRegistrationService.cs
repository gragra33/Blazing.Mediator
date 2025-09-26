using Blazing.Mediator.Pipeline;
using Blazing.Mediator.Statistics;
using Blazing.Mediator.OpenTelemetry;
using System.Reflection;

namespace Blazing.Mediator.Registration;

/// <summary>
/// Static service responsible for registering mediator services, handlers, and middleware in the dependency injection container.
/// This class encapsulates the core registration logic separated from the extension methods.
/// </summary>
internal static class MediatorRegistrationService
{
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
    public static IServiceCollection RegisterMediatorServices(
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
        RegisterMediator(services, shouldRegisterStatistics, hasTelemetryOptions, hasLoggingOptions);
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
                return new MediatorStatistics(renderer, statisticsOptions);
            });
        }
    }

    /// <summary>
    /// Registers Mediator with conditional statistics dependency.
    /// </summary>
    private static void RegisterMediator(IServiceCollection services, bool shouldRegisterStatistics, bool hasTelemetryOptions, bool hasLoggingOptions)
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
        services.AddSingleton(configuration);

        // Register the configured pipeline builder as the scoped pipeline builder
        services.AddScoped(provider =>
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);

        // Register the configured notification pipeline builder as the scoped notification pipeline builder with constraint validation
        services.AddScoped(provider =>
        {
            var config = provider.GetRequiredService<MediatorConfiguration>();
            var notificationPipelineBuilder = config.NotificationPipelineBuilder;
            
            // Apply constraint validation options if configured
            if (notificationPipelineBuilder is NotificationPipelineBuilder builder && config.ConstraintValidationOptions != null)
            {
                builder.SetConstraintValidationOptions(config.ConstraintValidationOptions);
            }
            
            return notificationPipelineBuilder;
        });

        // Register constraint validation options if configured
        if (configuration.ConstraintValidationOptions != null)
        {
            services.AddSingleton(configuration.ConstraintValidationOptions);
        }

        // Register constraint compatibility checker as a singleton service
        services.AddSingleton<ConstraintCompatibilityChecker>();

        // Register pipeline inspector for debugging (same instance as pipeline builder)
        services.AddScoped(provider =>
            provider.GetRequiredService<IMiddlewarePipelineBuilder>() as IMiddlewarePipelineInspector
            ?? throw new InvalidOperationException("Pipeline builder must implement IMiddlewarePipelineInspector"));

        // Register notification pipeline inspector for debugging (same instance as notification pipeline builder)
        services.AddScoped(provider =>
            provider.GetRequiredService<INotificationPipelineBuilder>() as INotificationMiddlewarePipelineInspector
            ?? throw new InvalidOperationException("Notification pipeline builder must implement INotificationMiddlewarePipelineInspector"));
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
            // Get constraint validation options for enhanced discovery
            var constraintValidationOptions = configuration.ConstraintValidationOptions;
            bool strictValidation = constraintValidationOptions?.Strictness == ConstraintValidationOptions.ValidationStrictness.Strict;
            bool constraintValidationEnabled = constraintValidationOptions?.Strictness != ConstraintValidationOptions.ValidationStrictness.Disabled;

            // Check if constrained middleware discovery is enabled
            bool discoverConstrainedMiddleware = configuration.DiscoverConstrainedMiddleware;

            List<Type> middlewareTypes = assembly.GetTypes()
                .Where(t =>
                    t is { IsAbstract: false, IsInterface: false } &&
                    t.GetInterfaces().Any(i => IsMiddlewareType(i, discoverMiddleware, discoverNotificationMiddleware, discoverConstrainedMiddleware)))
                .ToList();

            foreach (Type middlewareType in middlewareTypes)
            {
                // Enhanced discovery for type-constrained notification middleware
                var notificationMiddlewareInterfaces = middlewareType.GetInterfaces()
                    .Where(i => i == typeof(INotificationMiddleware) ||
                               i == typeof(IConditionalNotificationMiddleware) ||
                               (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>) && discoverConstrainedMiddleware))
                    .ToList();

                bool isNotificationMiddleware = notificationMiddlewareInterfaces.Any();

                if (isNotificationMiddleware)
                {
                    // Register the middleware type in DI if not already registered
                    if (services.All(s => s.ImplementationType != middlewareType))
                    {
                        services.AddScoped(middlewareType);
                    }

                    // Enhanced constraint validation for type-constrained middleware
                    foreach (var interfaceType in notificationMiddlewareInterfaces)
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))
                        {
                            var constraintType = interfaceType.GetGenericArguments()[0];
                            
                            // Validate constraint compatibility if enabled
                            if (constraintValidationEnabled)
                            {
                                var validationResult = ValidateConstraintType(constraintType, middlewareType, constraintValidationOptions!, strictValidation);
                                if (!validationResult.IsValid && strictValidation)
                                {
                                    throw new InvalidOperationException($"Constraint validation failed for middleware {middlewareType.Name} with constraint {constraintType.Name}: {validationResult.ErrorMessage}");
                                }
                            }
                        }
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
                $"Loader exceptions: {string.Join(", ", ex.LoaderExceptions?.Select(e => e?.Message) ?? [])}", ex);
        }

        return;

        static bool IsMiddlewareType(Type i, bool includeRequestMiddleware, bool includeNotificationMiddleware, bool includeConstrainedMiddleware) =>
            (includeRequestMiddleware && i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalStreamRequestMiddleware<,>))) ||
             (includeNotificationMiddleware &&
             (i == typeof(INotificationMiddleware) ||
             i == typeof(IConditionalNotificationMiddleware) ||
             (includeConstrainedMiddleware && i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationMiddleware<>))));
    }

    /// <summary>
    /// Validates a constraint type for type-constrained notification middleware.
    /// </summary>
    /// <param name="constraintType">The constraint type to validate.</param>
    /// <param name="middlewareType">The middleware type implementing the constraint.</param>
    /// <param name="options">Constraint validation options.</param>
    /// <param name="strictValidation">Whether to use strict validation.</param>
    /// <returns>Validation result with success status and error message.</returns>
    private static (bool IsValid, string? ErrorMessage) ValidateConstraintType(Type constraintType, Type middlewareType, ConstraintValidationOptions options, bool strictValidation)
    {
        try
        {
            // Check if the constraint type is excluded from validation
            if (options.ExcludedTypes.Contains(constraintType))
            {
                return (true, null);
            }

            // Basic constraint validation: must implement INotification
            if (!typeof(INotification).IsAssignableFrom(constraintType))
            {
                return (false, $"Constraint type {constraintType.Name} must implement INotification");
            }

            // Validate nested generic constraints if enabled
            if (options.ValidateNestedGenericConstraints && constraintType.IsGenericType)
            {
                var result = ValidateNestedGenericConstraints(constraintType, options.MaxConstraintInheritanceDepth);
                if (!result.IsValid)
                {
                    return result;
                }
            }

            // Validate circular dependencies if enabled
            if (options.ValidateCircularDependencies)
            {
                var result = ValidateCircularDependencies(constraintType, new HashSet<Type>(), options.MaxConstraintInheritanceDepth);
                if (!result.IsValid)
                {
                    return result;
                }
            }

            // Apply custom validation rules if any exist
            if (options.CustomValidationRules.TryGetValue(constraintType, out var customRule))
            {
                try
                {
                    if (!customRule(constraintType, middlewareType))
                    {
                        return (false, $"Custom validation rule failed for constraint {constraintType.Name} on middleware {middlewareType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Custom validation rule threw exception: {ex.Message}");
                }
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Validation error: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates nested generic constraints in complex type hierarchies.
    /// </summary>
    private static (bool IsValid, string? ErrorMessage) ValidateNestedGenericConstraints(Type type, int maxDepth, int currentDepth = 0)
    {
        if (currentDepth >= maxDepth)
        {
            return (false, $"Maximum constraint inheritance depth ({maxDepth}) exceeded for type {type.Name}");
        }

        if (type.IsGenericType)
        {
            var genericArguments = type.GetGenericArguments();
            foreach (var arg in genericArguments)
            {
                if (!typeof(INotification).IsAssignableFrom(arg))
                {
                    return (false, $"Generic argument {arg.Name} in {type.Name} must implement INotification");
                }

                if (arg.IsGenericType)
                {
                    var result = ValidateNestedGenericConstraints(arg, maxDepth, currentDepth + 1);
                    if (!result.IsValid)
                    {
                        return result;
                    }
                }
            }
        }

        return (true, null);
    }

    /// <summary>
    /// Validates that there are no circular dependencies in constraint type hierarchies.
    /// </summary>
    private static (bool IsValid, string? ErrorMessage) ValidateCircularDependencies(Type type, HashSet<Type> visited, int maxDepth)
    {
        if (visited.Count >= maxDepth)
        {
            return (false, $"Maximum constraint inheritance depth ({maxDepth}) exceeded while checking for circular dependencies");
        }

        if (visited.Contains(type))
        {
            return (false, $"Circular dependency detected in constraint hierarchy involving {type.Name}");
        }

        visited.Add(type);

        try
        {
            // Check base type
            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                var result = ValidateCircularDependencies(type.BaseType, visited, maxDepth);
                if (!result.IsValid)
                {
                    return result;
                }
            }

            // Check implemented interfaces
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && typeof(INotification).IsAssignableFrom(interfaceType))
                {
                    var result = ValidateCircularDependencies(interfaceType, visited, maxDepth);
                    if (!result.IsValid)
                    {
                        return result;
                    }
                }
            }

            return (true, null);
        }
        finally
        {
            visited.Remove(type);
        }
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
            // Skip assemblies that can't be loaded completely
            // This can happen with reference assemblies or incomplete dependencies
            throw new InvalidOperationException($"Failed to load types from assembly '{assembly.FullName}' during handler discovery. " +
                $"Loader exceptions: {string.Join(", ", ex.LoaderExceptions?.Select(e => e?.Message) ?? [])}", ex);
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
                services.AddScoped(interfaceType, serviceProvider => (object)serviceProvider.GetRequiredService(handlerType));
            }
            else
            {
                // For request handlers, register only if no implementation exists (single handler per request)
                if (services.All(s => s.ServiceType != interfaceType))
                {
                    services.AddScoped(interfaceType, serviceProvider => (object)serviceProvider.GetRequiredService(handlerType));
                }
            }
        }
    }
}