using Blazing.Mediator.OpenTelemetry;

namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration for the mediator, including middleware pipeline setup.
/// </summary>
public sealed class MediatorConfiguration
{
    private readonly IServiceCollection? _services;
    private readonly HashSet<Assembly> _assemblies = new();

    /// <summary>
    /// Gets the middleware pipeline builder.
    /// </summary>
    public IMiddlewarePipelineBuilder PipelineBuilder { get; } = new MiddlewarePipelineBuilder();

    /// <summary>
    /// Gets the notification middleware pipeline builder.
    /// </summary>
    public INotificationPipelineBuilder NotificationPipelineBuilder { get; } = new NotificationPipelineBuilder();

    /// <summary>
    /// Gets the assemblies configured for handler and middleware scanning.
    /// </summary>
    public IReadOnlyList<Assembly> Assemblies => _assemblies.ToList().AsReadOnly();

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
    /// Gets the telemetry options for OpenTelemetry integration.
    /// Provides granular control over what telemetry data is collected and how it is configured.
    /// </summary>
    public MediatorTelemetryOptions? TelemetryOptions { get; private set; }

    /// <summary>
    /// Gets the logging options for debug logging.
    /// Provides granular control over what debug information is logged.
    /// </summary>
    public LoggingOptions? LoggingOptions { get; private set; }

    /// <summary>
    /// Gets or sets whether to automatically discover and register request middleware from assemblies.
    /// </summary>
    public bool DiscoverMiddleware { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically discover and register notification middleware from assemblies.
    /// </summary>
    public bool DiscoverNotificationMiddleware { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically discover and register type-constrained notification middleware from assemblies.
    /// When enabled, middleware implementing INotificationMiddleware{T} will be automatically discovered and registered.
    /// This is separate from DiscoverNotificationMiddleware to allow fine-grained control over discovery behavior.
    /// </summary>
    public bool DiscoverConstrainedMiddleware { get; set; } = true; // Default to true for new functionality

    /// <summary>
    /// Gets or sets whether to automatically discover and register notification handlers from assemblies.
    /// </summary>
    public bool DiscoverNotificationHandlers { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the MediatorConfiguration class.
    /// </summary>
    /// <param name="services">The service collection for automatic DI registration of middleware</param>
    public MediatorConfiguration(IServiceCollection? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// Adds an assembly to scan for handlers and middleware using a marker type.
    /// </summary>
    /// <param name="assemblyMarkerType">A type from the assembly to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblyMarkerType is null.</exception>
    public MediatorConfiguration AddFromAssembly(Type assemblyMarkerType)
    {
        ArgumentNullException.ThrowIfNull(assemblyMarkerType);
        
        var assembly = assemblyMarkerType.Assembly;
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
        
        return this;
    }

    /// <summary>
    /// Adds an assembly to scan for handlers and middleware using a marker type.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">A type from the assembly to scan.</typeparam>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddFromAssembly<TAssemblyMarker>()
    {
        return AddFromAssembly(typeof(TAssemblyMarker));
    }

    /// <summary>
    /// Adds an assembly to scan for handlers and middleware.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assembly is null.</exception>
    public MediatorConfiguration AddFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
        
        return this;
    }

    /// <summary>
    /// Adds multiple assemblies to scan for handlers and middleware using marker types.
    /// </summary>
    /// <param name="assemblyMarkerTypes">Types from assemblies to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblyMarkerTypes is null.</exception>
    public MediatorConfiguration AddFromAssemblies(params Type[] assemblyMarkerTypes)
    {
        ArgumentNullException.ThrowIfNull(assemblyMarkerTypes);
        
        foreach (var markerType in assemblyMarkerTypes)
        {
            AddFromAssembly(markerType);
        }
        
        return this;
    }

    /// <summary>
    /// Adds multiple assemblies to scan for handlers and middleware.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null.</exception>
    public MediatorConfiguration AddFromAssemblies(params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        
        foreach (var assembly in assemblies)
        {
            AddFromAssembly(assembly);
        }
        
        return this;
    }

    /// <summary>
    /// Convenience alias for AddFromAssembly using a marker type.
    /// </summary>
    /// <param name="assemblyMarkerType">A type from the assembly to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblyMarkerType is null.</exception>
    public MediatorConfiguration AddAssembly(Type assemblyMarkerType)
    {
        return AddFromAssembly(assemblyMarkerType);
    }

    /// <summary>
    /// Convenience alias for AddFromAssembly using a marker type.
    /// </summary>
    /// <typeparam name="TAssemblyMarker">A type from the assembly to scan.</typeparam>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration AddAssembly<TAssemblyMarker>()
    {
        return AddFromAssembly<TAssemblyMarker>();
    }

    /// <summary>
    /// Convenience alias for AddFromAssembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assembly is null.</exception>
    public MediatorConfiguration AddAssembly(Assembly assembly)
    {
        return AddFromAssembly(assembly);
    }

    /// <summary>
    /// Convenience alias for AddFromAssemblies using marker types.
    /// </summary>
    /// <param name="assemblyMarkerTypes">Types from assemblies to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblyMarkerTypes is null.</exception>
    public MediatorConfiguration AddAssemblies(params Type[] assemblyMarkerTypes)
    {
        return AddFromAssemblies(assemblyMarkerTypes);
    }

    /// <summary>
    /// Convenience alias for AddFromAssemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null.</exception>
    public MediatorConfiguration AddAssemblies(params Assembly[] assemblies)
    {
        return AddFromAssemblies(assemblies);
    }

    /// <summary>
    /// Enables statistics tracking for Send commands.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithStatisticsTracking()
    {
#pragma warning disable CS0618 // For backwards compatibility
        EnableStatisticsTracking = true;
#pragma warning restore CS0618
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
    /// Enables telemetry tracking with default options.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithTelemetry()
    {
        TelemetryOptions = new MediatorTelemetryOptions();
        return this;
    }

    /// <summary>
    /// Enables telemetry tracking with granular configuration options.
    /// </summary>
    /// <param name="configure">Action to configure the telemetry options.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    public MediatorConfiguration WithTelemetry(Action<MediatorTelemetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new MediatorTelemetryOptions();
        configure(options);

        TelemetryOptions = options;
        return this;
    }

    /// <summary>
    /// Enables telemetry tracking with pre-configured options.
    /// </summary>
    /// <param name="options">The telemetry options to use.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public MediatorConfiguration WithTelemetry(MediatorTelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        TelemetryOptions = options;
        return this;
    }

    /// <summary>
    /// Enables debug logging with default configuration.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithLogging()
    {
        LoggingOptions = new LoggingOptions();
        return this;
    }

    /// <summary>
    /// Enables debug logging with granular configuration options.
    /// </summary>
    /// <param name="configure">Action to configure the logging options.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    public MediatorConfiguration WithLogging(Action<LoggingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new LoggingOptions();
        configure(options);

        var validationErrors = options.Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException($"Logging configuration validation failed: {string.Join(", ", validationErrors)}");
        }

        LoggingOptions = options;
        return this;
    }

    /// <summary>
    /// Enables debug logging with pre-configured options.
    /// </summary>
    /// <param name="options">The logging options to use.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public MediatorConfiguration WithLogging(LoggingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var validationErrors = options.Validate();
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException($"Logging configuration validation failed: {string.Join(", ", validationErrors)}");
        }

        LoggingOptions = options;
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
    /// Enables automatic discovery and registration of type-constrained notification middleware from assemblies.
    /// This allows middleware implementing INotificationMiddleware{T} to be automatically discovered and registered.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithConstrainedMiddlewareDiscovery()
    {
        DiscoverConstrainedMiddleware = true;
        return this;
    }

    /// <summary>
    /// Disables automatic discovery and registration of type-constrained notification middleware from assemblies.
    /// Only manually registered constrained middleware will be available.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutConstrainedMiddlewareDiscovery()
    {
        DiscoverConstrainedMiddleware = false;
        return this;
    }

    /// <summary>
    /// Enables automatic discovery and registration of notification handlers from assemblies.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithNotificationHandlerDiscovery()
    {
        DiscoverNotificationHandlers = true;
        return this;
    }

    /// <summary>
    /// Disables automatic discovery and registration of notification handlers from assemblies.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutNotificationHandlerDiscovery()
    {
        DiscoverNotificationHandlers = false;
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
