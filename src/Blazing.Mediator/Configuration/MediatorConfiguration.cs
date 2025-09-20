namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration for the mediator, including middleware pipeline setup.
/// </summary>
public sealed class MediatorConfiguration
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
    /// Gets or sets whether statistics tracking is enabled for Send commands.
    /// This property is deprecated. Use StatisticsOptions for granular control.
    /// </summary>
    [Obsolete("Use StatisticsOptions for granular control over statistics tracking. This property will be removed in a future version.")]
    public bool EnableStatisticsTracking { get; set; }

    /// <summary>
    /// Gets the statistics tracking options.
    /// Provides granular control over what statistics are collected and how they are managed.
    /// </summary>
    public StatisticsOptions? StatisticsOptions { get; private set; }

    /// <summary>
    /// Gets or sets whether to automatically discover and register request middleware from assemblies.
    /// </summary>
    public bool DiscoverMiddleware { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically discover and register notification middleware from assemblies.
    /// </summary>
    public bool DiscoverNotificationMiddleware { get; set; }

    /// <summary>
    /// Initializes a new instance of the MediatorConfiguration class.
    /// </summary>
    /// <param name="services">The service collection for automatic DI registration of middleware</param>
    public MediatorConfiguration(IServiceCollection? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// Enables statistics tracking for Send commands.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithStatisticsTracking()
    {
        EnableStatisticsTracking = true;
        StatisticsOptions = new StatisticsOptions();
        return this;
    }

    /// <summary>
    /// Enables statistics tracking with granular configuration options.
    /// </summary>
    /// <param name="configure">Action to configure the statistics options.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    public MediatorConfiguration WithStatisticsTracking(Action<StatisticsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new StatisticsOptions();
        configure(options);
        options.ValidateAndThrow(); // Validate the configuration

        StatisticsOptions = options;
#pragma warning disable CS0618 // For backwards compatibility
        EnableStatisticsTracking = options.IsEnabled;
#pragma warning restore CS0618
        return this;
    }

    /// <summary>
    /// Enables statistics tracking with pre-configured options.
    /// </summary>
    /// <param name="options">The statistics options to use.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public MediatorConfiguration WithStatisticsTracking(StatisticsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.ValidateAndThrow(); // Validate the configuration
        StatisticsOptions = options.Clone();
#pragma warning disable CS0618 // For backwards compatibility
        EnableStatisticsTracking = options.IsEnabled;
#pragma warning restore CS0618
        return this;
    }

    /// <summary>
    /// Enables automatic discovery and registration of request middleware from assemblies.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithMiddlewareDiscovery()
    {
        DiscoverMiddleware = true;
        return this;
    }

    /// <summary>
    /// Enables automatic discovery and registration of notification middleware from assemblies.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithNotificationMiddlewareDiscovery()
    {
        DiscoverNotificationMiddleware = true;
        return this;
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
