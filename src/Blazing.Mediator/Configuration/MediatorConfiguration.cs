namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration for the mediator, including middleware pipeline setup.
/// </summary>
public class MediatorConfiguration
{
    private readonly IServiceCollection? _services;

    /// <summary>
    /// Gets the middleware pipeline builder.
    /// </summary>
    public IMiddlewarePipelineBuilder PipelineBuilder { get; } = new MiddlewarePipelineBuilder();

    /// <summary>
    /// Gets the notification middleware pipeline builder.
    /// </summary>
    public INotificationPipelineBuilder NotificationPipelineBuilder { get; } = new NotificationPipelineBuilder();

    /// <summary>
    /// Initializes a new instance of the MediatorConfiguration class.
    /// </summary>
    /// <param name="services">The service collection for automatic DI registration of middleware</param>
    public MediatorConfiguration(IServiceCollection? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// Adds a middleware to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type</typeparam>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddMiddleware<TMiddleware>()
        where TMiddleware : class
    {
        PipelineBuilder.AddMiddleware<TMiddleware>();
        
        // Automatically register the middleware in DI if services collection is available
        if (_services != null)
        {
            RegisterMiddlewareInDI(typeof(TMiddleware));
        }
        
        return this;
    }

    /// <summary>
    /// Adds an open generic middleware type to the pipeline.
    /// </summary>
    /// <param name="middlewareType">The open generic middleware type</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddMiddleware(Type middlewareType)
    {
        PipelineBuilder.AddMiddleware(middlewareType);
        
        // Automatically register the middleware in DI if services collection is available
        if (_services != null)
        {
            RegisterMiddlewareInDI(middlewareType);
        }
        
        return this;
    }

    /// <summary>
    /// Adds multiple middleware types to the pipeline, maintaining their relative order.
    /// </summary>
    /// <param name="middlewareTypes">The middleware types to add in order</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddMiddleware(params Type[] middlewareTypes)
    {
        foreach (Type middlewareType in middlewareTypes)
        {
            AddMiddleware(middlewareType);
        }
        
        return this;
    }

    /// <summary>
    /// Adds a notification middleware to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The notification middleware type</typeparam>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddNotificationMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware
    {
        NotificationPipelineBuilder.AddMiddleware<TMiddleware>();
        
        // Automatically register the middleware in DI if services collection is available
        if (_services != null)
        {
            RegisterMiddlewareInDI(typeof(TMiddleware));
        }
        
        return this;
    }

    /// <summary>
    /// Adds a notification middleware to the pipeline with configuration.
    /// </summary>
    /// <typeparam name="TMiddleware">The notification middleware type</typeparam>
    /// <param name="configuration">Optional configuration object for the middleware</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddNotificationMiddleware<TMiddleware>(object? configuration)
        where TMiddleware : class, INotificationMiddleware
    {
        NotificationPipelineBuilder.AddMiddleware<TMiddleware>(configuration);
        
        // Automatically register the middleware in DI if services collection is available
        if (_services != null)
        {
            RegisterMiddlewareInDI(typeof(TMiddleware));
        }
        
        return this;
    }

    /// <summary>
    /// Adds a notification middleware type to the pipeline.
    /// </summary>
    /// <param name="middlewareType">The notification middleware type</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddNotificationMiddleware(Type middlewareType)
    {
        NotificationPipelineBuilder.AddMiddleware(middlewareType);
        
        // Automatically register the middleware in DI if services collection is available
        if (_services != null)
        {
            RegisterMiddlewareInDI(middlewareType);
        }
        
        return this;
    }

    /// <summary>
    /// Adds multiple notification middleware types to the pipeline, maintaining their relative order.
    /// </summary>
    /// <param name="middlewareTypes">The notification middleware types to add in order</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddNotificationMiddleware(params Type[] middlewareTypes)
    {
        foreach (Type middlewareType in middlewareTypes)
        {
            AddNotificationMiddleware(middlewareType);
        }
        
        return this;
    }

    /// <summary>
    /// Registers middleware in the dependency injection container.
    /// </summary>
    /// <param name="middlewareType">The middleware type to register</param>
    private void RegisterMiddlewareInDI(Type middlewareType)
    {
        if (_services == null)
        {
            return;
        }

        // Check if already registered to avoid duplicates
        if (_services.Any(s => s.ServiceType == middlewareType))
        {
            return;
        }

        // Register as scoped service
        _services.AddScoped(middlewareType);
    }
}
