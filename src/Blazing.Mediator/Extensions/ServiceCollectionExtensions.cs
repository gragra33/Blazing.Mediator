using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Configuration;
using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator;

/// <summary>  
/// Provides extension methods for registering Mediator services in the dependency injection container.  
/// </summary>  
public static class ServiceCollectionExtensions
{
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
        MediatorConfiguration configuration = new MediatorConfiguration(services);
        configureMiddleware?.Invoke(configuration);
        services.AddSingleton(configuration);
        
        // Register the configured pipeline builder as the scoped pipeline builder
        services.AddScoped<IMiddlewarePipelineBuilder>(provider => 
            provider.GetRequiredService<MediatorConfiguration>().PipelineBuilder);

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

            IEnumerable<Type> interfaces = handlerType.GetInterfaces()
                .Where(IsHandlerType);

            foreach (Type @interface in interfaces)
            {
                // Only register if not already registered  
                if (!services.Any(s => s.ServiceType == @interface && s.ImplementationType == handlerType))
                {
                    services.AddScoped(@interface, serviceProvider => serviceProvider.GetRequiredService(handlerType));
                }
            }
        }

        static bool IsHandlerType(Type i) =>
            i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
             i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
    }
}
