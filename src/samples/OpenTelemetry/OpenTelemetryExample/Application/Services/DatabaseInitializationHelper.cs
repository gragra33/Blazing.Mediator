using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Helper class for database initialization, seeding, and verification.
/// Handles database setup and telemetry provider verification during startup.
/// </summary>
public static class DatabaseInitializationHelper
{
    /// <summary>
    /// Initializes the database, seeds initial data, and verifies telemetry logging setup.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitializeAndSeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        
        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var logger = scopedServices.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Starting database initialization and seeding process");
        
        await InitializeDatabaseSchemaAsync(context, logger);
        await VerifyDatabaseTablesAsync(context, logger);
        await VerifyTelemetryLoggingProviderAsync(scopedServices, logger);
        
        LogCompletionStatus();
        
        logger.LogInformation("Database initialization and seeding process completed successfully");
    }
    
    /// <summary>
    /// Initializes the database schema by ensuring it's recreated with the latest structure.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger for this operation.</param>
    private static async Task InitializeDatabaseSchemaAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Initializing in-memory database schema");
        
        try
        {
            // Ensure database is created with latest schema including TelemetryLogs
            await context.Database.EnsureDeletedAsync(); // Delete existing database to ensure fresh schema
            await context.Database.EnsureCreatedAsync();
            
            logger.LogInformation("Database schema created successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize database schema");
            throw;
        }
    }
    
    /// <summary>
    /// Verifies that all expected database tables exist and are accessible.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger for this operation.</param>
    private static async Task VerifyDatabaseTablesAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            // Check Users table
            var userCount = await context.Users.CountAsync();
            logger.LogInformation("Database initialized with {UserCount} users", userCount);
            
            // Check TelemetryLogs table exists and is accessible
            var logCount = await context.TelemetryLogs.CountAsync();
            logger.LogInformation("TelemetryLogs table initialized with {LogCount} logs", logCount);
            
            // Verify other telemetry tables
            var activityCount = await context.TelemetryActivities.CountAsync();
            var metricCount = await context.TelemetryMetrics.CountAsync();
            var traceCount = await context.TelemetryTraces.CountAsync();
            
            logger.LogInformation("Telemetry tables verified - Activities: {ActivityCount}, Metrics: {MetricCount}, Traces: {TraceCount}", 
                activityCount, metricCount, traceCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying database tables");
            throw;
        }
    }
    
    /// <summary>
    /// Verifies that the TelemetryDatabaseLoggingProvider is properly registered and functional.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="logger">The logger for this operation.</param>
    private static async Task VerifyTelemetryLoggingProviderAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            // Verify that our provider is registered using the correct pattern
            var providers = serviceProvider.GetServices<ILoggerProvider>();
            var telemetryProviderFound = providers.Any(p => p is TelemetryDatabaseLoggingProvider);
            logger.LogInformation("TelemetryDatabaseLoggingProvider found in DI providers: {Found}", telemetryProviderFound);
            
            if (telemetryProviderFound)
            {
                logger.LogInformation("TelemetryDatabaseLoggingProvider successfully registered following Microsoft's pattern");
                
                // Test the logging factory to ensure loggers are created properly
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var testLogger = loggerFactory.CreateLogger("DatabaseInitializationTest");
                logger.LogInformation("Test logger created successfully using LoggerFactory");
                
                // Generate test logs to verify the provider is working
                await GenerateTestLogsAsync(testLogger, logger);
            }
            else
            {
                logger.LogWarning("TelemetryDatabaseLoggingProvider not found in DI container - logging may not be captured");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying TelemetryDatabaseLoggingProvider");
            throw;
        }
    }
    
    /// <summary>
    /// Generates test log entries at various levels to verify the telemetry provider is working.
    /// </summary>
    /// <param name="testLogger">The test logger to use.</param>
    /// <param name="logger">The main logger for this operation.</param>
    private static async Task GenerateTestLogsAsync(ILogger testLogger, ILogger logger)
    {
        logger.LogInformation("Generating test logs for TelemetryDatabaseLoggingProvider verification");
        
        try
        {
            // Generate test logs at different levels
            testLogger.LogTrace("Test trace message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogDebug("Test debug message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogInformation("Test information message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogWarning("Test warning message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogError("Test error message for TelemetryDatabaseLoggingProvider verification");
            testLogger.LogCritical("Test critical message for TelemetryDatabaseLoggingProvider verification");
            
            // Wait a brief moment for async processing
            await Task.Delay(100);
            
            logger.LogInformation("Test logs generated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating test logs");
        }
    }
    
    /// <summary>
    /// Logs completion status to the console for visibility.
    /// </summary>
    private static void LogCompletionStatus()
    {
        Console.WriteLine("[*] In-memory database created and seeded with initial user data");
        Console.WriteLine("[*] TelemetryLogs table is ready for log capture");
        Console.WriteLine("[*] Database initialization completed successfully");
    }
    
    /// <summary>
    /// Generates startup test logs to verify logging functionality across different categories.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving loggers.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GenerateStartupTestLogsAsync(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("OpenTelemetry API Server is starting up");
        logger.LogInformation("Blazing.Mediator Telemetry: {TelemetryStatus}", 
            Blazing.Mediator.Mediator.TelemetryEnabled ? "ENABLED" : "DISABLED");
        logger.LogInformation("Environment: {Environment}", 
            serviceProvider.GetRequiredService<IWebHostEnvironment>().EnvironmentName);
        logger.LogInformation("Application URLs will be displayed after startup");

        // Test various log levels to generate sample data
        logger.LogDebug("Debug message: Application debugging information");
        logger.LogInformation("Information message: Application started successfully");
        logger.LogWarning("Warning message: This is a test warning for telemetry demonstration");

        // Test application-specific logging from different categories
        var appLogger = serviceProvider.GetRequiredService<ILogger<OpenTelemetryExample.Controllers.UsersController>>();
        appLogger.LogInformation("Test log from UsersController category for telemetry capture");
        
        var mediatorLogger = serviceProvider.GetRequiredService<ILogger<Blazing.Mediator.Mediator>>();
        mediatorLogger.LogInformation("Test log from Blazing.Mediator category for telemetry capture");

        // Wait a moment for logs to be processed
        await Task.Delay(3000);

        LogStartupCompletion();
    }
    
    /// <summary>
    /// Logs startup completion status to the console.
    /// </summary>
    private static void LogStartupCompletion()
    {
        Console.WriteLine("[*] OpenTelemetry API Server is ready!");
        Console.WriteLine($"[*] Blazing.Mediator Telemetry: {(Blazing.Mediator.Mediator.TelemetryEnabled ? "ENABLED" : "DISABLED")}");
        Console.WriteLine("[*] Test logs have been generated for telemetry demonstration");
    }
}