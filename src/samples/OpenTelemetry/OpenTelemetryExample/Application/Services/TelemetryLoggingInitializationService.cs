namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Hosted service to ensure the TelemetryDatabaseLoggingProvider is properly initialized at startup.
/// </summary>
public class TelemetryLoggingInitializationService(
    ILogger<TelemetryLoggingInitializationService> logger,
    IServiceProvider serviceProvider)
    : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Verifying TelemetryDatabaseLoggingProvider registration...");

        try
        {
            // Verify ILoggerProvider services - this is the Microsoft recommended way
            var loggerProviders = serviceProvider.GetServices<ILoggerProvider>();
            var telemetryProviderFound = loggerProviders.Any(p => p is TelemetryDatabaseLoggingProvider);
            logger.LogInformation("TelemetryDatabaseLoggingProvider found in ILoggerProvider services: {Found}", telemetryProviderFound);

            // Manual approach: Add our provider directly to the LoggerFactory
            // This ensures it works even if Serilog is interfering
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            if (!telemetryProviderFound)
            {
                logger.LogWarning("TelemetryDatabaseLoggingProvider not found in DI container - attempting manual registration");

                // Create provider manually and add it to the factory
                var provider = serviceProvider.GetRequiredService<TelemetryDatabaseLoggingProvider>();
                loggerFactory.AddProvider(provider);

                logger.LogInformation("TelemetryDatabaseLoggingProvider manually added to LoggerFactory");
            }
            else
            {
                logger.LogInformation("TelemetryDatabaseLoggingProvider successfully registered following Microsoft's pattern");
            }

            // Test the logging factory to ensure loggers are created properly
            var testLogger = loggerFactory.CreateLogger("StartupTest");
            logger.LogInformation("Test logger created successfully using LoggerFactory");

            // Generate a test log to verify the provider is working
            testLogger.LogInformation("Test log message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogWarning("Test warning message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogError("Test error message for TelemetryDatabaseLoggingProvider verification");

            logger.LogInformation("Database logging initialization completed");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize database logging");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Database logging shutdown initiated");
        return Task.CompletedTask;
    }
}