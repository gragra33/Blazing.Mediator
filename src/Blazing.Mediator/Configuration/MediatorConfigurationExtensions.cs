using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blazing.Mediator.Configuration;

/// <summary>
/// Extension methods for MediatorConfiguration to support appsettings.json configuration.
/// </summary>
public static class MediatorConfigurationExtensions
{
    /// <summary>
    /// Configures the mediator using settings from IConfiguration.
    /// Looks for configuration under "Blazing:Mediator" section.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithConfiguration(
        this MediatorConfiguration config, 
        IConfiguration configuration)
    {
        return config.WithConfiguration(configuration, "Blazing:Mediator");
    }

    /// <summary>
    /// Configures the mediator using settings from IConfiguration with custom section path.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="sectionPath">The configuration section path</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithConfiguration(
        this MediatorConfiguration config, 
        IConfiguration configuration, 
        string sectionPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(sectionPath);

        var section = configuration.GetSection(sectionPath);
        if (!section.Exists()) 
        {
            // Section doesn't exist - return config unchanged
            return config;
        }

        // Bind directly to existing StatisticsOptions class
        var statisticsSection = section.GetSection("Statistics");
        if (statisticsSection.Exists())
        {
            var statisticsOptions = new StatisticsOptions();
            statisticsSection.Bind(statisticsOptions);
            
            // Leverage existing validation in StatisticsOptions
            statisticsOptions.ValidateAndThrow();
            config.WithStatisticsTracking(statisticsOptions);
        }

        // Bind directly to existing TelemetryOptions class
        var telemetrySection = section.GetSection("Telemetry");
        if (telemetrySection.Exists())
        {
            var telemetryOptions = new TelemetryOptions();
            telemetrySection.Bind(telemetryOptions);
            
            // Leverage existing validation in TelemetryOptions
            var telemetryErrors = telemetryOptions.Validate();
            if (telemetryErrors.Any())
            {
                throw new InvalidOperationException($"Telemetry configuration validation failed: {string.Join(", ", telemetryErrors)}");
            }
            config.WithTelemetry(telemetryOptions);
        }

        // Bind directly to existing LoggingOptions class
        var loggingSection = section.GetSection("Logging");
        if (loggingSection.Exists())
        {
            var loggingOptions = new LoggingOptions();
            loggingSection.Bind(loggingOptions);
            
            // Leverage existing validation in LoggingOptions
            var loggingErrors = loggingOptions.Validate();
            var actualErrors = loggingErrors.Where(e => !e.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase)).ToList();
            if (actualErrors.Any())
            {
                throw new InvalidOperationException($"Logging configuration validation failed: {string.Join(", ", actualErrors)}");
            }
            config.WithLogging(loggingOptions);
        }

        // Apply discovery options (only new configuration needed)
        var discoverySection = section.GetSection("Discovery");
        if (discoverySection.Exists())
        {
            var discoveryOptions = new DiscoveryOptions();
            discoverySection.Bind(discoveryOptions);
            ApplyDiscoveryOptions(config, discoveryOptions, discoverySection);
        }
        // Note: If Discovery section doesn't exist, we don't apply any discovery options,
        // preserving whatever was set by presets or previous configuration

        return config;
    }

    /// <summary>
    /// Configures the mediator using a pre-bound configuration section.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="configSection">The pre-bound configuration section</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithConfiguration(
        this MediatorConfiguration config, 
        MediatorConfigurationSection configSection)
    {
        ArgumentNullException.ThrowIfNull(configSection);

        // Apply Statistics configuration
        if (configSection.Statistics != null)
        {
            configSection.Statistics.ValidateAndThrow();
            config.WithStatisticsTracking(configSection.Statistics);
        }

        // Apply Telemetry configuration
        if (configSection.Telemetry != null)
        {
            var telemetryErrors = configSection.Telemetry.Validate();
            if (telemetryErrors.Any())
            {
                throw new InvalidOperationException($"Telemetry configuration validation failed: {string.Join(", ", telemetryErrors)}");
            }
            config.WithTelemetry(configSection.Telemetry);
        }

        // Apply Logging configuration
        if (configSection.Logging != null)
        {
            var loggingErrors = configSection.Logging.Validate();
            var actualErrors = loggingErrors.Where(e => !e.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase)).ToList();
            if (actualErrors.Any())
            {
                throw new InvalidOperationException($"Logging configuration validation failed: {string.Join(", ", actualErrors)}");
            }
            config.WithLogging(configSection.Logging);
        }

        // Apply Discovery configuration
        if (configSection.Discovery != null)
        {
            ApplyDiscoveryOptions(config, configSection.Discovery);
        }

        return config;
    }

    /// <summary>
    /// Configures the mediator using environment-aware configuration with automatic preset selection.
    /// Automatically detects the environment and applies the appropriate configuration and preset.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="configuration">The application configuration</param>
    /// <param name="environment">The hosting environment</param>
    /// <param name="sectionPath">The configuration section path (optional, defaults to "Blazing:Mediator")</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithEnvironmentConfiguration(
        this MediatorConfiguration config,
        IConfiguration configuration,
        IHostEnvironment environment,
        string sectionPath = "Blazing:Mediator")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        // First apply environment-specific preset
        if (environment.IsDevelopment())
        {
            config.WithDevelopmentPreset();
        }
        else if (environment.IsProduction())
        {
            config.WithProductionPreset();
        }
        else if (environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            config.WithDisabledPreset();
        }
        else
        {
            config.WithMinimalPreset();
        }

        // Then apply any configuration overrides from appsettings.json
        return config.WithConfiguration(configuration, sectionPath);
    }

    /// <summary>
    /// Applies discovery options to the mediator configuration.
    /// Only applies settings that are explicitly configured in the discovery options.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="discovery">The discovery options</param>
    /// <param name="configurationSection">The configuration section to check for explicit values</param>
    private static void ApplyDiscoveryOptions(MediatorConfiguration config, DiscoveryOptions discovery, IConfigurationSection? configurationSection = null)
    {
        // If we have a configuration section, only apply values that were explicitly set
        if (configurationSection != null)
        {
            if (configurationSection["DiscoverMiddleware"] != null)
            {
                if (discovery.DiscoverMiddleware)
                    config.WithMiddlewareDiscovery();
                else
                    config.WithoutMiddlewareDiscovery();
            }

            if (configurationSection["DiscoverNotificationMiddleware"] != null)
            {
                if (discovery.DiscoverNotificationMiddleware)
                    config.WithNotificationMiddlewareDiscovery();
                else
                    config.WithoutNotificationMiddlewareDiscovery();
            }

            if (configurationSection["DiscoverConstrainedMiddleware"] != null)
            {
                if (discovery.DiscoverConstrainedMiddleware)
                    config.WithConstrainedMiddlewareDiscovery();
                else
                    config.WithoutConstrainedMiddlewareDiscovery();
            }

            if (configurationSection["DiscoverNotificationHandlers"] != null)
            {
                if (discovery.DiscoverNotificationHandlers)
                    config.WithNotificationHandlerDiscovery();
                else
                    config.WithoutNotificationHandlerDiscovery();
            }
        }
        else
        {
            // No configuration section provided, apply all values (used for preset application)
            if (discovery.DiscoverMiddleware)
                config.WithMiddlewareDiscovery();
            else
                config.WithoutMiddlewareDiscovery();

            if (discovery.DiscoverNotificationMiddleware)
                config.WithNotificationMiddlewareDiscovery();
            else
                config.WithoutNotificationMiddlewareDiscovery();

            if (discovery.DiscoverConstrainedMiddleware)
                config.WithConstrainedMiddlewareDiscovery();
            else
                config.WithoutConstrainedMiddlewareDiscovery();

            if (discovery.DiscoverNotificationHandlers)
                config.WithNotificationHandlerDiscovery();
            else
                config.WithoutNotificationHandlerDiscovery();
        }
    }

    /// <summary>
    /// Applies a pre-configured environment preset to the current configuration.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="preset">The environment preset configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithEnvironmentPreset(
        this MediatorConfiguration config, 
        MediatorConfiguration preset)
    {
        ArgumentNullException.ThrowIfNull(preset);

        // Apply StatisticsOptions from preset (reuse existing class)
        if (preset.StatisticsOptions != null)
        {
            config.WithStatisticsTracking(preset.StatisticsOptions.Clone());
        }
        else
        {
            config.WithoutStatistics();
        }

        // Apply TelemetryOptions from preset (reuse existing class)
        if (preset.TelemetryOptions != null)
        {
            config.WithTelemetry(preset.TelemetryOptions.Clone());
        }
        else
        {
            config.WithoutTelemetry();
        }

        // Apply LoggingOptions from preset (reuse existing class)
        if (preset.LoggingOptions != null)
        {
            config.WithLogging(preset.LoggingOptions.Clone());
        }
        else
        {
            config.WithoutLogging();
        }

        // Apply discovery settings
        if (preset.DiscoverMiddleware)
            config.WithMiddlewareDiscovery();
        else
            config.WithoutMiddlewareDiscovery();

        if (preset.DiscoverNotificationMiddleware)
            config.WithNotificationMiddlewareDiscovery();
        else
            config.WithoutNotificationMiddlewareDiscovery();

        if (preset.DiscoverConstrainedMiddleware)
            config.WithConstrainedMiddlewareDiscovery();
        else
            config.WithoutConstrainedMiddlewareDiscovery();

        if (preset.DiscoverNotificationHandlers)
            config.WithNotificationHandlerDiscovery();
        else
            config.WithoutNotificationHandlerDiscovery();

        return config;
    }

    /// <summary>
    /// Applies development environment configuration with comprehensive features and detailed debugging.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithDevelopmentPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.Development());
    }

    /// <summary>
    /// Applies production environment configuration with essential features and optimized performance.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithProductionPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.Production());
    }

    /// <summary>
    /// Applies disabled configuration with all optional features disabled for maximum performance.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithDisabledPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.Disabled());
    }

    /// <summary>
    /// Applies minimal configuration with basic features only.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithMinimalPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.Minimal());
    }

    /// <summary>
    /// Applies notification-optimized configuration for event-driven architectures.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithNotificationOptimizedPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.NotificationOptimized());
    }

    /// <summary>
    /// Applies streaming-optimized configuration for real-time data processing.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <returns>The configuration for chaining</returns>
    public static MediatorConfiguration WithStreamingOptimizedPreset(this MediatorConfiguration config)
    {
        return config.WithEnvironmentPreset(MediatorConfiguration.StreamingOptimized());
    }

    /// <summary>
    /// Validates the configuration against environment-specific requirements.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="environment">The hosting environment</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid for the environment</exception>
    public static MediatorConfiguration ValidateForEnvironment(this MediatorConfiguration config, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var errors = new List<string>();

        // Environment-specific validations
        if (environment.IsProduction())
        {
            // Production shouldn't have verbose logging enabled
            if (config.LoggingOptions?.EnableDetailedHandlerInfo == true ||
                config.LoggingOptions?.EnableDetailedTypeClassification == true ||
                config.LoggingOptions?.EnableConstraintLogging == true)
            {
                errors.Add("Production environment should not have detailed logging enabled for performance reasons");
            }

            // Production should have reasonable retention periods
            if (config.StatisticsOptions?.MetricsRetentionPeriod < TimeSpan.FromHours(1))
            {
                errors.Add("Production environment should have metrics retention period of at least 1 hour");
            }

            // Production should not have packet-level telemetry enabled
            if (config.TelemetryOptions?.PacketLevelTelemetryEnabled == true)
            {
                errors.Add("Production environment should not have packet-level telemetry enabled for performance reasons");
            }
        }

        if (environment.IsDevelopment())
        {
            // Development should have some form of logging enabled
            if (config.LoggingOptions == null)
            {
                errors.Add("Development environment should have logging enabled for debugging purposes");
            }
        }

        // General validation
        config.ValidateAndThrow();

        if (errors.Any())
        {
            throw new InvalidOperationException($"Configuration validation failed for environment '{environment.EnvironmentName}': {string.Join("; ", errors)}");
        }

        return config;
    }

    /// <summary>
    /// Validates the configuration against environment-specific requirements with custom validation rules.
    /// Allows developers to add their own validation logic through a fluent interface.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="environment">The hosting environment</param>
    /// <param name="customValidation">Custom validation action that adds validation rules</param>
    /// <returns>The configuration for chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid for the environment</exception>
    public static MediatorConfiguration ValidateForEnvironment(this MediatorConfiguration config, IHostEnvironment environment, Action<ConfigurationValidationBuilder> customValidation)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(customValidation);

        var validationBuilder = new ConfigurationValidationBuilder(config, environment);

        // Apply built-in environment-specific validations first
        validationBuilder.ApplyBuiltInValidations();

        // Apply custom validation rules
        customValidation(validationBuilder);

        // Execute all validations
        validationBuilder.ValidateAndThrow();

        return config;
    }

    /// <summary>
    /// Creates a configuration diagnostics report for troubleshooting.
    /// </summary>
    /// <param name="config">The mediator configuration</param>
    /// <param name="environment">The hosting environment (optional)</param>
    /// <returns>A detailed diagnostics report</returns>
    public static ConfigurationDiagnostics GetDiagnostics(this MediatorConfiguration config, IHostEnvironment? environment = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        var diagnostics = new ConfigurationDiagnostics
        {
            Environment = environment?.EnvironmentName ?? "Unknown",
            Timestamp = DateTimeOffset.UtcNow,
            AssemblyCount = config.Assemblies.Count,
            HasStatistics = config.StatisticsOptions != null,
            HasTelemetry = config.TelemetryOptions != null,
            HasLogging = config.LoggingOptions != null,
            DiscoverySettings = new DiscoveryDiagnostics
            {
                DiscoverMiddleware = config.DiscoverMiddleware,
                DiscoverNotificationMiddleware = config.DiscoverNotificationMiddleware,
                DiscoverConstrainedMiddleware = config.DiscoverConstrainedMiddleware,
                DiscoverNotificationHandlers = config.DiscoverNotificationHandlers
            }
        };

        // Add validation results
        var validationErrors = config.Validate();
        diagnostics.ValidationErrors = validationErrors.ToList();
        diagnostics.IsValid = !validationErrors.Any();

        // Add configuration details
        if (config.StatisticsOptions != null)
        {
            diagnostics.StatisticsEnabled = config.StatisticsOptions.IsEnabled;
            diagnostics.StatisticsRetentionPeriod = config.StatisticsOptions.MetricsRetentionPeriod;
        }

        if (config.TelemetryOptions != null)
        {
            diagnostics.TelemetryEnabled = config.TelemetryOptions.Enabled;
            diagnostics.StreamingTelemetryEnabled = config.TelemetryOptions.IsStreamingTelemetryEnabled;
        }

        if (config.LoggingOptions != null)
        {
            var loggingValidation = config.LoggingOptions.Validate();
            diagnostics.LoggingWarnings = loggingValidation.Where(e => e.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return diagnostics;
    }
}

/// <summary>
/// Configuration diagnostics information for troubleshooting.
/// </summary>
public class ConfigurationDiagnostics
{
    public string Environment { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> LoggingWarnings { get; set; } = new();
    public int AssemblyCount { get; set; }
    public bool HasStatistics { get; set; }
    public bool HasTelemetry { get; set; }
    public bool HasLogging { get; set; }
    public bool StatisticsEnabled { get; set; }
    public bool TelemetryEnabled { get; set; }
    public bool StreamingTelemetryEnabled { get; set; }
    public TimeSpan? StatisticsRetentionPeriod { get; set; }
    public DiscoveryDiagnostics DiscoverySettings { get; set; } = new();
}

/// <summary>
/// Discovery settings diagnostics information.
/// </summary>
public class DiscoveryDiagnostics
{
    public bool DiscoverMiddleware { get; set; }
    public bool DiscoverNotificationMiddleware { get; set; }
    public bool DiscoverConstrainedMiddleware { get; set; }
    public bool DiscoverNotificationHandlers { get; set; }
}

/// <summary>
/// Fluent validation builder for MediatorConfiguration.
/// Provides a fluent interface for adding custom validation rules to configuration validation.
/// </summary>
public class ConfigurationValidationBuilder
{
    private readonly MediatorConfiguration _config;
    private readonly IHostEnvironment _environment;
    private readonly List<string> _errors = new();

    /// <summary>
    /// Initializes a new instance of the ConfigurationValidationBuilder class.
    /// </summary>
    /// <param name="config">The mediator configuration to validate</param>
    /// <param name="environment">The hosting environment</param>
    internal ConfigurationValidationBuilder(MediatorConfiguration config, IHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
    }

    /// <summary>
    /// Gets the mediator configuration being validated.
    /// </summary>
    public MediatorConfiguration Configuration => _config;

    /// <summary>
    /// Gets the hosting environment.
    /// </summary>
    public IHostEnvironment Environment => _environment;

    /// <summary>
    /// Adds a validation rule with a custom condition and error message.
    /// </summary>
    /// <param name="condition">The condition that must be true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the condition fails</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(bool condition, string errorMessage)
    {
        if (!condition)
        {
            _errors.Add(errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule with a custom predicate and error message.
    /// </summary>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(Func<MediatorConfiguration, bool> predicate, string errorMessage)
    {
        if (!predicate(_config))
        {
            _errors.Add(errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule with a custom predicate that includes environment context.
    /// </summary>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(Func<MediatorConfiguration, IHostEnvironment, bool> predicate, string errorMessage)
    {
        if (!predicate(_config, _environment))
        {
            _errors.Add(errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule for a specific environment.
    /// </summary>
    /// <param name="environmentName">The environment name to validate against</param>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder ForEnvironment(string environmentName, Func<MediatorConfiguration, bool> predicate, string errorMessage)
    {
        if (_environment.EnvironmentName.Equals(environmentName, StringComparison.OrdinalIgnoreCase))
        {
            Rule(predicate, errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule that only applies to production environments.
    /// </summary>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder ForProduction(Func<MediatorConfiguration, bool> predicate, string errorMessage)
    {
        if (_environment.IsProduction())
        {
            Rule(predicate, errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule that only applies to development environments.
    /// </summary>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder ForDevelopment(Func<MediatorConfiguration, bool> predicate, string errorMessage)
    {
        if (_environment.IsDevelopment())
        {
            Rule(predicate, errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds a validation rule that only applies to staging environments.
    /// </summary>
    /// <param name="predicate">The predicate function that must return true for validation to pass</param>
    /// <param name="errorMessage">The error message to add if the predicate returns false</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder ForStaging(Func<MediatorConfiguration, bool> predicate, string errorMessage)
    {
        if (_environment.EnvironmentName.Equals("Staging", StringComparison.OrdinalIgnoreCase))
        {
            Rule(predicate, errorMessage);
        }
        return this;
    }

    /// <summary>
    /// Adds validation rules for statistics configuration.
    /// </summary>
    /// <param name="statisticsValidation">Action to configure statistics-specific validation rules</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Statistics(Action<StatisticsValidationBuilder> statisticsValidation)
    {
        ArgumentNullException.ThrowIfNull(statisticsValidation);

        var statisticsBuilder = new StatisticsValidationBuilder(this, _config.StatisticsOptions);
        statisticsValidation(statisticsBuilder);
        return this;
    }

    /// <summary>
    /// Adds validation rules for telemetry configuration.
    /// </summary>
    /// <param name="telemetryValidation">Action to configure telemetry-specific validation rules</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Telemetry(Action<TelemetryValidationBuilder> telemetryValidation)
    {
        ArgumentNullException.ThrowIfNull(telemetryValidation);

        var telemetryBuilder = new TelemetryValidationBuilder(this, _config.TelemetryOptions);
        telemetryValidation(telemetryBuilder);
        return this;
    }

    /// <summary>
    /// Adds validation rules for logging configuration.
    /// </summary>
    /// <param name="loggingValidation">Action to configure logging-specific validation rules</param>
    /// <returns>The validation builder for chaining</returns>
    public ConfigurationValidationBuilder Logging(Action<LoggingValidationBuilder> loggingValidation)
    {
        ArgumentNullException.ThrowIfNull(loggingValidation);

        var loggingBuilder = new LoggingValidationBuilder(this, _config.LoggingOptions);
        loggingValidation(loggingBuilder);
        return this;
    }

    /// <summary>
    /// Applies the built-in environment-specific validation rules.
    /// </summary>
    internal void ApplyBuiltInValidations()
    {
        if (_environment.IsProduction())
        {
            // Production shouldn't have verbose logging enabled
            if (_config.LoggingOptions?.EnableDetailedHandlerInfo == true ||
                _config.LoggingOptions?.EnableDetailedTypeClassification == true ||
                _config.LoggingOptions?.EnableConstraintLogging == true)
            {
                _errors.Add("Production environment should not have detailed logging enabled for performance reasons");
            }

            // Production should have reasonable retention periods
            if (_config.StatisticsOptions?.MetricsRetentionPeriod < TimeSpan.FromHours(1))
            {
                _errors.Add("Production environment should have metrics retention period of at least 1 hour");
            }

            // Production should not have packet-level telemetry enabled
            if (_config.TelemetryOptions?.PacketLevelTelemetryEnabled == true)
            {
                _errors.Add("Production environment should not have packet-level telemetry enabled for performance reasons");
            }
        }

        if (_environment.IsDevelopment())
        {
            // Development should have some form of logging enabled
            if (_config.LoggingOptions == null)
            {
                _errors.Add("Development environment should have logging enabled for debugging purposes");
            }
        }
    }

    /// <summary>
    /// Adds an error message to the validation results.
    /// </summary>
    /// <param name="errorMessage">The error message to add</param>
    internal void AddError(string errorMessage)
    {
        _errors.Add(errorMessage);
    }

    /// <summary>
    /// Validates all rules and throws an exception if any validation errors occurred.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation errors are found</exception>
    internal void ValidateAndThrow()
    {
        // General validation
        _config.ValidateAndThrow();

        if (_errors.Any())
        {
            throw new InvalidOperationException($"Configuration validation failed for environment '{_environment.EnvironmentName}': {string.Join("; ", _errors)}");
        }
    }
}

/// <summary>
/// Fluent validation builder for StatisticsOptions.
/// </summary>
public class StatisticsValidationBuilder
{
    private readonly ConfigurationValidationBuilder _parent;
    private readonly StatisticsOptions? _options;

    internal StatisticsValidationBuilder(ConfigurationValidationBuilder parent, StatisticsOptions? options)
    {
        _parent = parent;
        _options = options;
    }

    /// <summary>
    /// Validates that statistics are enabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder MustBeEnabled(string? errorMessage = null)
    {
        if (_options == null || !_options.IsEnabled)
        {
            _parent.AddError(errorMessage ?? "Statistics must be enabled");
        }
        return _parent;
    }

    /// <summary>
    /// Validates that statistics are disabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder MustBeDisabled(string? errorMessage = null)
    {
        if (_options?.IsEnabled == true)
        {
            _parent.AddError(errorMessage ?? "Statistics must be disabled");
        }
        return _parent;
    }

    /// <summary>
    /// Validates the retention period meets minimum requirements.
    /// </summary>
    /// <param name="minimumRetention">The minimum retention period</param>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder RetentionPeriodAtLeast(TimeSpan minimumRetention, string? errorMessage = null)
    {
        if (_options?.MetricsRetentionPeriod < minimumRetention)
        {
            _parent.AddError(errorMessage ?? $"Statistics retention period must be at least {minimumRetention}");
        }
        return _parent;
    }

    /// <summary>
    /// Applies a custom validation rule to statistics options.
    /// </summary>
    /// <param name="predicate">The validation predicate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(Func<StatisticsOptions?, bool> predicate, string errorMessage)
    {
        if (!predicate(_options))
        {
            _parent.AddError(errorMessage);
        }
        return _parent;
    }
}

/// <summary>
/// Fluent validation builder for TelemetryOptions.
/// </summary>
public class TelemetryValidationBuilder
{
    private readonly ConfigurationValidationBuilder _parent;
    private readonly TelemetryOptions? _options;

    internal TelemetryValidationBuilder(ConfigurationValidationBuilder parent, TelemetryOptions? options)
    {
        _parent = parent;
        _options = options;
    }

    /// <summary>
    /// Validates that telemetry is enabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder MustBeEnabled(string? errorMessage = null)
    {
        if (_options?.Enabled != true)
        {
            _parent.AddError(errorMessage ?? "Telemetry must be enabled");
        }
        return _parent;
    }

    /// <summary>
    /// Validates that telemetry is disabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder MustBeDisabled(string? errorMessage = null)
    {
        if (_options?.Enabled == true)
        {
            _parent.AddError(errorMessage ?? "Telemetry must be disabled");
        }
        return _parent;
    }

    /// <summary>
    /// Validates that packet-level telemetry is disabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder PacketLevelMustBeDisabled(string? errorMessage = null)
    {
        if (_options?.PacketLevelTelemetryEnabled == true)
        {
            _parent.AddError(errorMessage ?? "Packet-level telemetry must be disabled");
        }
        return _parent;
    }

    /// <summary>
    /// Applies a custom validation rule to telemetry options.
    /// </summary>
    /// <param name="predicate">The validation predicate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(Func<TelemetryOptions?, bool> predicate, string errorMessage)
    {
        if (!predicate(_options))
        {
            _parent.AddError(errorMessage);
        }
        return _parent;
    }
}

/// <summary>
/// Fluent validation builder for LoggingOptions.
/// </summary>
public class LoggingValidationBuilder
{
    private readonly ConfigurationValidationBuilder _parent;
    private readonly LoggingOptions? _options;

    internal LoggingValidationBuilder(ConfigurationValidationBuilder parent, LoggingOptions? options)
    {
        _parent = parent;
        _options = options;
    }

    /// <summary>
    /// Validates that logging is enabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder MustBeEnabled(string? errorMessage = null)
    {
        if (_options == null)
        {
            _parent.AddError(errorMessage ?? "Logging must be enabled");
        }
        return _parent;
    }

    /// <summary>
    /// Validates that detailed logging features are disabled.
    /// </summary>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder DetailedFeaturesMustBeDisabled(string? errorMessage = null)
    {
        if (_options?.EnableDetailedHandlerInfo == true ||
            _options?.EnableDetailedTypeClassification == true ||
            _options?.EnableConstraintLogging == true)
        {
            _parent.AddError(errorMessage ?? "Detailed logging features must be disabled");
        }
        return _parent;
    }

    /// <summary>
    /// Applies a custom validation rule to logging options.
    /// </summary>
    /// <param name="predicate">The validation predicate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>The parent validation builder for chaining</returns>
    public ConfigurationValidationBuilder Rule(Func<LoggingOptions?, bool> predicate, string errorMessage)
    {
        if (!predicate(_options))
        {
            _parent.AddError(errorMessage);
        }
        return _parent;
    }
}