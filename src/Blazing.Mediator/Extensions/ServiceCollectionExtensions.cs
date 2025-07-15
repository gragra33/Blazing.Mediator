namespace Blazing.Mediator;

/// <summary>  
/// Provides extension methods for registering Mediator services in the dependency injection container.  
/// </summary>  
public static class ServiceCollectionExtensions
{
    #region AddMediator Methods

    /// <summary>  
    /// Adds mediator services with default configuration.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        return services.AddMediator(null, (Assembly[])null!);
    }

    /// <summary>  
    /// Adds mediator services and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[]? assemblies)
    {
        return services.AddMediator(null, assemblies);
    }

    /// <summary>  
    /// Adds mediator services and registers handlers from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, params Type[]? assemblyMarkerTypes)
    {
        return services.AddMediator(null, assemblyMarkerTypes);
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
        return services.AddMediator(null, discoverMiddleware, assemblies);
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
        return services.AddMediator(null, discoverMiddleware, assemblyMarkerTypes);
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
        services.AddScoped<IMediator, Mediator>();

        // Configure middleware if provided
        MediatorConfiguration configuration = new(services);
        configureMiddleware?.Invoke(configuration);
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

        if (assemblies is not { Length: > 0 })
        {
            return services;
        }

        // Deduplicate assemblies to prevent duplicate registrations  
        Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

        foreach (Assembly assembly in uniqueAssemblies)
        {
            RegisterHandlers(services, assembly);
        }

        return services;
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
        if (assemblyMarkerTypes == null || assemblyMarkerTypes.Length == 0)
        {
            return services.AddMediator(configureMiddleware, (Assembly[])null!);
        }

        Assembly[] assemblies = assemblyMarkerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return services.AddMediator(configureMiddleware, assemblies);
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery and registers handlers from multiple assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblies">Assemblies to scan for handlers and optionally middleware.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool discoverMiddleware, params Assembly[]? assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        // Configure middleware
        MediatorConfiguration configuration = new(services);

        // Auto-discover middleware from assemblies if requested
        if (discoverMiddleware && assemblies is { Length: > 0 })
        {
            RegisterMiddleware(configuration, assemblies);
        }

        // Apply user configuration after auto-discovery to allow overrides
        configureMiddleware?.Invoke(configuration);

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

        if (assemblies is not { Length: > 0 })
        {
            return services;
        }

        // Deduplicate assemblies to prevent duplicate registrations  
        Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

        foreach (Assembly assembly in uniqueAssemblies)
        {
            RegisterHandlers(services, assembly);
        }

        return services;
    }

    /// <summary>  
    /// Adds mediator services with optional middleware auto-discovery from assemblies containing the specified types.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="configureMiddleware">Optional action to configure middleware pipeline.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblyMarkerTypes">Types used to identify assemblies to scan.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorConfiguration>? configureMiddleware, bool discoverMiddleware, params Type[]? assemblyMarkerTypes)
    {
        if (assemblyMarkerTypes == null || assemblyMarkerTypes.Length == 0)
        {
            return services.AddMediator(configureMiddleware, discoverMiddleware, (Assembly[])null!);
        }

        Assembly[] assemblies = assemblyMarkerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return services.AddMediator(configureMiddleware, discoverMiddleware, assemblies);
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
        return services.AddMediator(callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from the assembly.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services, bool discoverMiddleware)
    {
        return services.AddMediatorFromCallingAssembly(null, discoverMiddleware);
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
        return services.AddMediator(configureMiddleware, callingAssembly);
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
        return services.AddMediator(configureMiddleware, discoverMiddleware, callingAssembly);
    }

    #endregion

    #region AddMediatorFromLoadedAssemblies Methods

    /// <summary>  
    /// Adds mediator services and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Func<Assembly, bool>? assemblyFilter = null)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return services.AddMediator(assemblies);
    }

    /// <summary>  
    /// Adds mediator services with automatic middleware discovery from all loaded assemblies.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="discoverMiddleware">Whether to automatically discover and register middleware from assemblies.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, bool discoverMiddleware, Func<Assembly, bool>? assemblyFilter = null)
    {
        return services.AddMediatorFromLoadedAssemblies(null, discoverMiddleware, assemblyFilter);
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

        return services.AddMediator(configureMiddleware, assemblies);
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

        return services.AddMediator(configureMiddleware, discoverMiddleware, assemblies);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>  
    /// Registers middleware from the specified assemblies.  
    /// </summary>  
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assemblies">Assemblies to scan for middleware.</param>  
    private static void RegisterMiddleware(MediatorConfiguration configuration, Assembly[] assemblies)
    {
        // Deduplicate assemblies
        Assembly[] uniqueAssemblies = assemblies.Distinct().ToArray();

        foreach (Assembly assembly in uniqueAssemblies)
        {
            RegisterMiddlewareFromAssembly(configuration, assembly);
        }
    }

    /// <summary>  
    /// Registers middleware from a single assembly.  
    /// </summary>  
    /// <param name="configuration">The mediator configuration.</param>  
    /// <param name="assembly">Assembly to scan for middleware.</param>  
    private static void RegisterMiddlewareFromAssembly(MediatorConfiguration configuration, Assembly assembly)
    {
        List<Type> middlewareTypes = assembly.GetTypes()
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                t.GetInterfaces().Any(IsMiddlewareType))
            .ToList();

        foreach (Type middlewareType in middlewareTypes)
        {
            configuration.AddMiddleware(middlewareType);
        }

        return;

        static bool IsMiddlewareType(Type i) =>
            (i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<>) ||
             i.GetGenericTypeDefinition() == typeof(IConditionalMiddleware<,>) ||
             i.GetGenericTypeDefinition() == typeof(IStreamRequestMiddleware<,>))) ||
             typeof(INotificationMiddleware).IsAssignableFrom(i) ||
             typeof(IConditionalNotificationMiddleware).IsAssignableFrom(i);
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

            // Register all handler interfaces using LINQ
            handlerType.GetInterfaces()
                .Where(IsHandlerType)
                .Where(@interface => !services.Any(s => s.ServiceType == @interface && s.ImplementationType == handlerType))
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
