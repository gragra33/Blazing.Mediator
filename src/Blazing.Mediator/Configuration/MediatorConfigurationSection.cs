namespace Blazing.Mediator.Configuration;

/// <summary>
/// Represents the root configuration section for Blazing.Mediator.
/// Provides access to statistics, telemetry, logging, and discovery options.
/// </summary>
public class MediatorConfigurationSection
{
    /// <summary>
    /// Gets or sets the configuration options for mediator statistics tracking.
    /// Controls what statistics are collected and how they are managed.
    /// </summary>
    public StatisticsOptions? Statistics { get; set; }   

    /// <summary>
    /// Gets or sets the configuration options for OpenTelemetry integration.
    /// Controls what telemetry data is collected and how it is configured.
    /// </summary>
    public TelemetryOptions? Telemetry { get; set; }     

    /// <summary>
    /// Gets or sets the configuration options for mediator debug logging.
    /// Provides granular control over what debug information is logged and at what level.
    /// </summary>
    public LoggingOptions? Logging { get; set; }

    /// <summary>
    /// Gets or sets the configuration options for handler and middleware discovery.
    /// Controls how and what components are discovered at runtime.
    /// </summary>
    public DiscoveryOptions? Discovery { get; set; }
}