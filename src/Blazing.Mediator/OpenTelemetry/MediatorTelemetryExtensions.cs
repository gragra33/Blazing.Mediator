namespace Blazing.Mediator.OpenTelemetry;

/// <summary>
/// Extension methods for configuring Blazing.Mediator OpenTelemetry integration.
/// </summary>
public static class MediatorTelemetryExtensions
{
    /// <summary>
    /// Configures Blazing.Mediator telemetry options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for telemetry options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ConfigureMediatorTelemetry(
        this IServiceCollection services,
        Action<MediatorTelemetryOptions> configure)
    {
        var options = new MediatorTelemetryOptions();
        configure(options);
        
        // Apply configuration to static properties
        Mediator.TelemetryEnabled = options.Enabled;
        
        services.AddSingleton(options);
        return services;
    }
    
    /// <summary>
    /// Enables Blazing.Mediator telemetry with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorTelemetry(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(_ => { /* Use defaults */ });
    }
    
    /// <summary>
    /// Disables Blazing.Mediator telemetry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection DisableMediatorTelemetry(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(options => options.Enabled = false);
    }
}