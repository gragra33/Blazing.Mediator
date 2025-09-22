using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using OpenTelemetryExample.Application.Services;

namespace OpenTelemetryExample.Extensions;

/// <summary>
/// Extension methods for ILoggingBuilder to add database logging.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds the database logging provider to the logging builder.
    /// Follows Microsoft's recommended pattern for custom logging providers.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <returns>The logging builder for method chaining.</returns>
    public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder)
    {
        // Add configuration support (Microsoft recommended pattern)
        builder.AddConfiguration();
        
        // Register the provider using Microsoft's recommended enumerable pattern
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, TelemetryDatabaseLoggingProvider>());

        // Register provider options using Microsoft's recommended pattern
        LoggerProviderOptions.RegisterProviderOptions
            <TelemetryDatabaseLoggingConfiguration, TelemetryDatabaseLoggingProvider>(builder.Services);

        return builder;
    }

    /// <summary>
    /// Adds the database logging provider to the logging builder with custom configuration.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">Configuration action for the database logging provider.</param>
    /// <returns>The logging builder for method chaining.</returns>
    public static ILoggingBuilder AddDatabaseLogging(
        this ILoggingBuilder builder,
        Action<TelemetryDatabaseLoggingConfiguration> configure)
    {
        builder.AddDatabaseLogging();
        builder.Services.Configure(configure);
        
        return builder;
    }
}