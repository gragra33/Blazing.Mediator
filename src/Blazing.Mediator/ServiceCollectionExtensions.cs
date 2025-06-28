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
        services.AddScoped<IMediator, Mediator>();

        if (assemblies is { Length: > 0 })
        {
            // Deduplicate assemblies to prevent duplicate registrations  
            var uniqueAssemblies = assemblies.Distinct().ToArray();

            foreach (var assembly in uniqueAssemblies)
            {
                RegisterHandlers(services, assembly);
            }
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
        if (assemblyMarkerTypes == null || assemblyMarkerTypes.Length == 0)
        {
            return services.AddMediator((Assembly[])null!);
        }
        
        var assemblies = assemblyMarkerTypes.Select(t => t.Assembly).Distinct().ToArray();
        return services.AddMediator(assemblies);
    }

    /// <summary>  
    /// Adds mediator services and automatically scans the calling assembly.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromCallingAssembly(this IServiceCollection services)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        return services.AddMediator(callingAssembly);
    }

    /// <summary>  
    /// Adds mediator services and scans all loaded assemblies for handlers.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assemblyFilter">Optional filter to include specific assemblies.</param>  
    /// <returns>The service collection for chaining.</returns>  
    public static IServiceCollection AddMediatorFromLoadedAssemblies(this IServiceCollection services, Func<Assembly, bool>? assemblyFilter = null)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        if (assemblyFilter != null)
        {
            assemblies = assemblies.Where(assemblyFilter).ToArray();
        }

        return services.AddMediator(assemblies);
    }

    /// <summary>  
    /// Registers handler types from the specified assembly into the service collection.  
    /// </summary>  
    /// <param name="services">The service collection.</param>  
    /// <param name="assembly">The assembly to scan for handler types.</param>  
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && Enumerable.Any(t.GetInterfaces(), i => i.IsGenericType &&
                                             (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                                              i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            // First register the handler type itself as scoped  
            if (services.All(s => s.ImplementationType != handlerType))
            {
                services.AddScoped(handlerType);
            }

            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                           (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>) ||
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            foreach (var @interface in interfaces)
            {
                // Only register if not already registered  
                if (!services.Any(s => s.ServiceType == @interface && s.ImplementationType == handlerType))
                {
                    services.AddScoped(@interface, serviceProvider => serviceProvider.GetRequiredService(handlerType));
                }
            }
        }
    }
}
