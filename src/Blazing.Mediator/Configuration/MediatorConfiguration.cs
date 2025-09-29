namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration for the mediator, including middleware pipeline setup.
/// </summary>
public sealed class MediatorConfiguration : IEnvironmentConfigurationOptions<MediatorConfiguration>
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
    [Obsolete(
        "Use StatisticsOptions for granular control over statistics tracking. This property will be removed in a future version.")]
    public bool EnableStatisticsTracking { get; set; }

    /// <summary>
    /// Gets the statistics tracking options.
    /// Provides granular control over what statistics are collected and how they are managed.
    /// </summary>
    public StatisticsOptions? StatisticsOptions { get; set; }

    /// <summary>
    /// Gets the telemetry options for OpenTelemetry integration.
    /// Provides granular control over what telemetry data is collected and how it is configured.
    /// </summary>
    public TelemetryOptions? TelemetryOptions { get; private set; }

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
        _assemblies.Add(assembly);

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

        _assemblies.Add(assembly);

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
        TelemetryOptions = new TelemetryOptions();
        return this;
    }

    /// <summary>
    /// Enables telemetry tracking with granular configuration options.
    /// </summary>
    /// <param name="configure">Action to configure the telemetry options.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    public MediatorConfiguration WithTelemetry(Action<TelemetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new TelemetryOptions();
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
    public MediatorConfiguration WithTelemetry(TelemetryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        TelemetryOptions = options;
        return this;
    }

    /// <summary>
    /// Enables notification telemetry tracking with default options.
    /// Sets up comprehensive notification handler and subscriber telemetry.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithNotificationTelemetry()
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CaptureNotificationHandlerDetails = true;
        TelemetryOptions.CreateHandlerChildSpans = true;
        TelemetryOptions.CaptureSubscriberMetrics = true;
        TelemetryOptions.CaptureNotificationMiddlewareDetails = true;
        return this;
    }

    /// <summary>
    /// Enables notification telemetry tracking with granular configuration options.
    /// </summary>
    /// <param name="configure">Action to configure the notification telemetry options.</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when configure is null.</exception>
    public MediatorConfiguration WithNotificationTelemetry(Action<TelemetryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        TelemetryOptions ??= new TelemetryOptions();
        configure(TelemetryOptions);
        return this;
    }

    /// <summary>
    /// Enables creation of child spans for individual notification handlers.
    /// This provides detailed per-handler visibility in distributed tracing.
    /// </summary>
    /// <param name="enabled">Whether to create handler child spans. Default is true.</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithHandlerChildSpans(bool enabled = true)
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CreateHandlerChildSpans = enabled;
        return this;
    }

    /// <summary>
    /// Enables capture of notification subscriber metrics.
    /// Tracks manual subscriber performance and registration status.
    /// </summary>
    /// <param name="enabled">Whether to capture subscriber metrics. Default is true.</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithSubscriberMetrics(bool enabled = true)
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CaptureSubscriberMetrics = enabled;
        return this;
    }

    /// <summary>
    /// Enables capture of detailed notification handler information.
    /// Includes handler execution details, performance metrics, and error tracking.
    /// </summary>
    /// <param name="enabled">Whether to capture notification handler details. Default is true.</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithNotificationHandlerDetails(bool enabled = true)
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CaptureNotificationHandlerDetails = enabled;
        return this;
    }

    /// <summary>
    /// Enables capture of notification middleware execution details.
    /// Tracks middleware performance, execution order, and error handling.
    /// </summary>
    /// <param name="enabled">Whether to capture notification middleware details. Default is true.</param>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithNotificationMiddlewareDetails(bool enabled = true)
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CaptureNotificationMiddlewareDetails = enabled;
        return this;
    }

    /// <summary>
    /// Disables all notification-specific telemetry tracking.
    /// This turns off child spans, subscriber metrics, handler details, and middleware details.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutNotificationTelemetry()
    {
        TelemetryOptions ??= new TelemetryOptions();
        TelemetryOptions.CaptureNotificationHandlerDetails = false;
        TelemetryOptions.CreateHandlerChildSpans = false;
        TelemetryOptions.CaptureSubscriberMetrics = false;
        TelemetryOptions.CaptureNotificationMiddlewareDetails = false;
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
        // Filter out warnings - only throw for actual errors
        var actualErrors = validationErrors.Where(e => !e.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (actualErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Logging configuration validation failed: {string.Join(", ", actualErrors)}");
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
        // Filter out warnings - only throw for actual errors
        var actualErrors = validationErrors.Where(e => !e.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (actualErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Logging configuration validation failed: {string.Join(", ", actualErrors)}");
        }

        LoggingOptions = options;
        return this;
    }

    /// <summary>
    /// Disables telemetry tracking by clearing telemetry options.
    /// This prevents OpenTelemetry metrics and tracing from being collected.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutTelemetry()
    {
        TelemetryOptions = null;
        return this;
    }

    /// <summary>
    /// Disables statistics tracking by clearing statistics options.
    /// This prevents runtime statistics collection for queries, commands, and notifications.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutStatistics()
    {
        StatisticsOptions = null;
#pragma warning disable CS0618 // For backwards compatibility
        EnableStatisticsTracking = false;
#pragma warning restore CS0618
        return this;
    }

    /// <summary>
    /// Disables debug logging by clearing logging options.
    /// This prevents detailed debug logging from being generated.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutLogging()
    {
        LoggingOptions = null;
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
    /// Disables automatic discovery and registration of request middleware from assemblies.
    /// Only manually registered request middleware will be available.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutMiddlewareDiscovery()
    {
        DiscoverMiddleware = false;
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
    /// Disables automatic discovery and registration of notification middleware from assemblies.
    /// Only manually registered notification middleware will be available.
    /// </summary>
    /// <returns>The configuration for chaining</returns>
    public MediatorConfiguration WithoutNotificationMiddlewareDiscovery()
    {
        DiscoverNotificationMiddleware = false;
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

    #region Validation and Utility Methods

    /// <summary>
    /// Validates the current configuration and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Validate sub-configurations
        if (StatisticsOptions != null)
        {
            var statisticsErrors = StatisticsOptions.Validate();
            foreach (var error in statisticsErrors)
            {
                errors.Add($"Statistics: {error}");
            }
        }

        if (TelemetryOptions != null)
        {
            var telemetryErrors = TelemetryOptions.Validate();
            foreach (var error in telemetryErrors)
            {
                errors.Add($"Telemetry: {error}");
            }
        }

        if (LoggingOptions != null)
        {
            var loggingErrors = LoggingOptions.Validate();
            foreach (var error in loggingErrors)
            {
                errors.Add($"Logging: {error}");
            }
        }

        // Validate assembly configuration
        if (!_assemblies.Any() && !DiscoverMiddleware && !DiscoverNotificationMiddleware)
        {
            errors.Add(
                "No assemblies configured and no middleware discovery enabled. At least one assembly should be added or middleware discovery should be enabled.");
        }

        return errors;
    }

    /// <summary>
    /// Validates the configuration and throws an exception if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the configuration is invalid.</exception>
    public void ValidateAndThrow()
    {
        var errors = Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException($"Invalid MediatorConfiguration: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Creates a copy of the current configuration with all the same values.
    /// Note: This creates a new configuration instance but does not clone the IServiceCollection reference.
    /// </summary>
    /// <returns>A new MediatorConfiguration instance with the same configuration.</returns>
    public MediatorConfiguration Clone()
    {
        var clone = new MediatorConfiguration(_services);

        // Copy assemblies
        foreach (var assembly in _assemblies)
        {
            clone._assemblies.Add(assembly);
        }

        // Copy properties
        clone.DiscoverMiddleware = DiscoverMiddleware;
        clone.DiscoverNotificationMiddleware = DiscoverNotificationMiddleware;
        clone.DiscoverConstrainedMiddleware = DiscoverConstrainedMiddleware;
        clone.DiscoverNotificationHandlers = DiscoverNotificationHandlers;

#pragma warning disable CS0618 // For backwards compatibility
        clone.EnableStatisticsTracking = EnableStatisticsTracking;
#pragma warning restore CS0618

        // Clone sub-configurations
        clone.StatisticsOptions = StatisticsOptions?.Clone();
        clone.TelemetryOptions = TelemetryOptions?.Clone();
        clone.LoggingOptions = LoggingOptions?.Clone();

        // Note: Pipeline builders are not cloned as they contain middleware registrations
        // that should be set up fresh in the new configuration

        return clone;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Helper method to create a configuration with assemblies added.
    /// Implements DRY principle by centralizing assembly addition logic.
    /// </summary>
    /// <param name="config">The configuration to add assemblies to.</param>
    /// <param name="assemblies">Assemblies to add to the configuration.</param>
    /// <returns>The configuration with assemblies added.</returns>
    private static MediatorConfiguration WithAssemblies(MediatorConfiguration config, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            config.AddAssembly(assembly);
        }

        return config;
    }

    /// <summary>
    /// Creates a configuration suitable for development environments.
    /// Enables comprehensive features with detailed information for debugging.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration configured for development scenarios.</returns>
    public static MediatorConfiguration Development(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(StatisticsOptions.Development())
            .WithTelemetry(TelemetryOptions.Development())
            .WithLogging(LoggingOptions.CreateVerbose())
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    /// <summary>
    /// Creates a configuration suitable for production environments.
    /// Enables essential features with optimized performance settings.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration configured for production scenarios.</returns>
    public static MediatorConfiguration Production(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(StatisticsOptions.Production())
            .WithTelemetry(TelemetryOptions.Production())
            .WithLogging(LoggingOptions.CreateMinimal())
            .WithMiddlewareDiscovery()
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    /// <summary>
    /// Creates a configuration with all optional features disabled.
    /// Useful for high-performance scenarios where only basic mediator functionality is needed.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration with all optional features disabled.</returns>
    public static MediatorConfiguration Disabled(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithoutStatistics()
            .WithTelemetry(TelemetryOptions.Disabled())
            .WithoutLogging()
            .WithoutConstrainedMiddlewareDiscovery()
            .WithoutNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    /// <summary>
    /// Creates a minimal configuration with basic features only.
    /// Suitable for high-performance scenarios with minimal overhead.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration configured for minimal overhead scenarios.</returns>
    public static MediatorConfiguration Minimal(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(StatisticsOptions.Disabled())
            .WithTelemetry(TelemetryOptions.Minimal())
            .WithoutLogging()
            .WithoutConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    /// <summary>
    /// Creates a configuration optimized for notification-centric applications.
    /// Enables comprehensive notification features while optimizing other areas.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration optimized for notification scenarios.</returns>
    public static MediatorConfiguration NotificationOptimized(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = false;
                options.EnableNotificationMetrics = true;
                options.EnableMiddlewareMetrics = true;
                options.EnableDetailedAnalysis = true;
            })
            .WithTelemetry(TelemetryOptions.NotificationOnly())
            .WithLogging(options =>
            {
                options.EnableSend = false;
                options.EnablePublish = true;
                options.EnableNotificationMiddleware = true;
                options.EnableSubscriberDetails = true;
                options.EnableConstraintLogging = true;
            })
            .WithNotificationMiddlewareDiscovery()
            .WithConstrainedMiddlewareDiscovery()
            .WithNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    /// <summary>
    /// Creates a configuration optimized for streaming applications.
    /// Enables comprehensive streaming features while optimizing other areas.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for handlers and middleware.</param>
    /// <returns>A new MediatorConfiguration optimized for streaming scenarios.</returns>
    public static MediatorConfiguration StreamingOptimized(params Assembly[] assemblies)
    {
        var config = new MediatorConfiguration()
            .WithStatisticsTracking(options =>
            {
                options.EnableRequestMetrics = true;
                options.EnableNotificationMetrics = false;
                options.EnablePerformanceCounters = true;
                options.EnableDetailedAnalysis = true;
            })
            .WithTelemetry(TelemetryOptions.StreamingOnly())
            .WithLogging(options =>
            {
                options.EnableSendStream = true;
                options.EnableSend = false;
                options.EnablePublish = false;
                options.EnablePerformanceTiming = true;
            })
            .WithMiddlewareDiscovery()
            .WithoutNotificationMiddlewareDiscovery()
            .WithoutConstrainedMiddlewareDiscovery()
            .WithoutNotificationHandlerDiscovery();

        return WithAssemblies(config, assemblies);
    }

    #region IEnvironmentConfigurationOptions<MediatorConfiguration> Implementation

    /// <summary>
    /// Creates a configuration suitable for development environments.
    /// Enables comprehensive features with detailed information for debugging.
    /// This overload is required by IEnvironmentConfigurationOptions interface.
    /// </summary>
    /// <returns>A new MediatorConfiguration configured for development scenarios.</returns>
    static MediatorConfiguration IEnvironmentConfigurationOptions<MediatorConfiguration>.Development()
    {
        return Development();
    }

    /// <summary>
    /// Creates a configuration suitable for production environments.
    /// Enables essential features with optimized performance settings.
    /// This overload is required by IEnvironmentConfigurationOptions interface.
    /// </summary>
    /// <returns>A new MediatorConfiguration configured for production scenarios.</returns>
    static MediatorConfiguration IEnvironmentConfigurationOptions<MediatorConfiguration>.Production()
    {
        return Production();
    }

    /// <summary>
    /// Creates a configuration with all optional features disabled.
    /// Useful for high-performance scenarios where only basic mediator functionality is needed.
    /// This overload is required by IEnvironmentConfigurationOptions interface.
    /// </summary>
    /// <returns>A new MediatorConfiguration with all optional features disabled.</returns>
    static MediatorConfiguration IEnvironmentConfigurationOptions<MediatorConfiguration>.Disabled()
    {
        return Disabled();
    }

    #endregion

    #endregion

}
