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
        Mediator.PacketLevelTelemetryEnabled = options.PacketLevelTelemetryEnabled;
        Mediator.PacketTelemetryBatchSize = options.PacketTelemetryBatchSize;

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
    /// Enables Blazing.Mediator telemetry with enhanced streaming visibility.
    /// This configuration provides maximum telemetry data but may impact performance in high-throughput scenarios.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorTelemetryWithFullVisibility(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(options =>
        {
            options.Enabled = true;
            options.CaptureMiddlewareDetails = true;
            options.CaptureHandlerDetails = true;
            options.CaptureExceptionDetails = true;
            options.EnableHealthChecks = true;
            options.PacketLevelTelemetryEnabled = true;
            options.PacketTelemetryBatchSize = 1; // Individual packet events for maximum visibility
            options.EnableStreamingMetrics = true;
            options.CapturePacketSize = true;
            options.EnableStreamingPerformanceClassification = true;
        });
    }

    /// <summary>
    /// Enables Blazing.Mediator telemetry optimized for production environments.
    /// Provides good visibility while maintaining performance with batched packet telemetry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorTelemetryForProduction(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(options =>
        {
            options.Enabled = true;
            options.CaptureMiddlewareDetails = true;
            options.CaptureHandlerDetails = true;
            options.CaptureExceptionDetails = true;
            options.EnableHealthChecks = true;
            options.PacketLevelTelemetryEnabled = false; // Disabled for performance
            options.PacketTelemetryBatchSize = 50; // Larger batches for efficiency
            options.EnableStreamingMetrics = true;
            options.CapturePacketSize = false; // Disabled for performance
            options.EnableStreamingPerformanceClassification = true;
        });
    }

    /// <summary>
    /// Enables Blazing.Mediator telemetry optimized for development and debugging.
    /// Provides comprehensive telemetry data for debugging purposes.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorTelemetryForDevelopment(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(options =>
        {
            options.Enabled = true;
            options.CaptureMiddlewareDetails = true;
            options.CaptureHandlerDetails = true;
            options.CaptureExceptionDetails = true;
            options.EnableHealthChecks = true;
            options.PacketLevelTelemetryEnabled = true;
            options.PacketTelemetryBatchSize = 5; // Small batches for detailed visibility
            options.EnableStreamingMetrics = true;
            options.CapturePacketSize = true;
            options.EnableStreamingPerformanceClassification = true;
            options.MaxExceptionMessageLength = 500; // More detailed exception info
            options.MaxStackTraceLines = 10; // More detailed stack traces
        });
    }

    /// <summary>
    /// Disables Blazing.Mediator telemetry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection DisableMediatorTelemetry(this IServiceCollection services)
    {
        return services.ConfigureMediatorTelemetry(options =>
        {
            options.Enabled = false;
            options.PacketLevelTelemetryEnabled = false;
        });
    }

    /// <summary>
    /// Configures Blazing.Mediator telemetry for streaming scenarios with customizable packet telemetry.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="enablePacketLevelTelemetry">Whether to enable packet-level telemetry with child spans.</param>
    /// <param name="batchSize">The batch size for packet telemetry events. Set to 1 for individual packet events.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorStreamingTelemetry(
        this IServiceCollection services,
        bool enablePacketLevelTelemetry = true,
        int batchSize = 10)
    {
        return services.ConfigureMediatorTelemetry(options =>
        {
            options.Enabled = true;
            options.PacketLevelTelemetryEnabled = enablePacketLevelTelemetry;
            options.PacketTelemetryBatchSize = Math.Max(1, batchSize);
            options.EnableStreamingMetrics = true;
            options.EnableStreamingPerformanceClassification = true;
        });
    }
}