using System.Reflection;

namespace OpenTelemetryExample.Application.Services;

/// <summary>
/// Helper class for inspecting and analyzing the logging configuration.
/// Useful for debugging logging provider registration issues.
/// </summary>
public static class LoggerInspectionHelper
{
    private static class Resources
    {
        public const string AnalyzingLoggingConfig = "[LOGGER INSPECTION] === Analyzing Logging Configuration After DI Build ===";
        public const string TestingLoggerCreation = "[LOGGER INSPECTION] === Testing Logger Creation ===";
        public const string EndLoggerInspection = "[LOGGER INSPECTION] === End Logger Inspection ===";
        public const string CreatingTestLogEntries = "[LOGGER INSPECTION] Creating test log entries...";
        public const string TestLogEntriesCreated = "[LOGGER INSPECTION] Test log entries created successfully";
        public const string EndTestLogging = "[LOGGER INSPECTION] === End Test Logging ===";
        public const string EndTestLoggingBlank = "";
        public const string EndLoggerInspectionBlank = "";
        public const string EndLoggerInspectionNewLine = "\n";
        public const string EndTestLoggingNewLine = "\n";
        public const string EndLoggerInspectionNewLine2 = "\n";
        public const string EndLoggerInspectionNewLine3 = "\n";
        public const string CouldNotFindProviders = "[LOGGER INSPECTION] Could not find _providers field via reflection";
    }

    /// <summary>
    /// Inspects the current logging configuration and outputs detailed information
    /// about registered providers and logger factory type.
    /// </summary>
    /// <param name="serviceProvider">The service provider to inspect.</param>
    public static void InspectLoggingConfiguration(IServiceProvider serviceProvider)
    {
        Console.WriteLine(Resources.AnalyzingLoggingConfig);

        try
        {
            // Get the logger factory and inspect its type
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            Console.WriteLine($"[LOGGER INSPECTION] LoggerFactory Type: {loggerFactory.GetType().FullName}");

            // Get all registered ILoggerProvider services
            var loggerProviders = serviceProvider.GetServices<ILoggerProvider>().ToList();
            Console.WriteLine($"[LOGGER INSPECTION] Total ILoggerProvider services registered: {loggerProviders.Count}");

            for (int i = 0; i < loggerProviders.Count; i++)
            {
                var provider = loggerProviders[i];
                Console.WriteLine($"[LOGGER INSPECTION]   Provider {i + 1}: {provider.GetType().FullName}");
            }

            // Try to inspect the internal providers of the logger factory using reflection
            InspectLoggerFactoryInternals(loggerFactory);

            Console.WriteLine();
            Console.WriteLine(Resources.TestingLoggerCreation);

            // Test logger creation and inspect the type
            var testLogger = loggerFactory.CreateLogger("LoggerInspectionTest");
            Console.WriteLine($"[LOGGER INSPECTION] Test Logger Type: {testLogger.GetType().FullName}");

            // Count TelemetryDatabaseLoggingProvider instances specifically
            var telemetryProviders = loggerProviders.OfType<TelemetryDatabaseLoggingProvider>().ToList();
            Console.WriteLine($"[LOGGER INSPECTION] TelemetryDatabaseLoggingProvider instances found: {telemetryProviders.Count}");

            foreach (var provider in telemetryProviders)
            {
                Console.WriteLine($"[LOGGER INSPECTION]   TelemetryProvider: {provider.GetType().FullName}");
            }

            Console.WriteLine(Resources.EndLoggerInspection);
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGGER INSPECTION] Error during inspection: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to inspect the internal structure of the logger factory using reflection.
    /// This is useful for understanding how providers are actually stored and used.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to inspect.</param>
    private static void InspectLoggerFactoryInternals(ILoggerFactory loggerFactory)
    {
        try
        {
            var factoryType = loggerFactory.GetType();

            // Try to find providers field (common in Microsoft's LoggerFactory)
            var providersField = factoryType.GetField("_providers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (providersField != null)
            {
                var providers = providersField.GetValue(loggerFactory);
                if (providers is IEnumerable<ILoggerProvider> providerList)
                {
                    var internalProviders = providerList.ToList();
                    Console.WriteLine($"[LOGGER INSPECTION] Internal providers found via reflection: {internalProviders.Count}");

                    for (int i = 0; i < internalProviders.Count; i++)
                    {
                        Console.WriteLine($"[LOGGER INSPECTION]   Internal Provider {i + 1}: {internalProviders[i].GetType().FullName}");
                    }
                }
            }
            else
            {
                Console.WriteLine(Resources.CouldNotFindProviders);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGGER INSPECTION] Reflection inspection failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates test log entries to verify that logging providers are working correctly.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="categoryName">The category name for the test logger.</param>
    public static void TestLogging(IServiceProvider serviceProvider, string categoryName = "LoggerInspectionTest")
    {
        Console.WriteLine($"[LOGGER INSPECTION] === Testing Logging with Category: {categoryName} ===");

        try
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(categoryName);

            Console.WriteLine(Resources.CreatingTestLogEntries);

            // Generate test logs at different levels
            logger.LogTrace("Trace test message from LoggerInspectionHelper");
            logger.LogDebug("Debug test message from LoggerInspectionHelper");
            logger.LogInformation("Information test message from LoggerInspectionHelper");
            logger.LogWarning("Warning test message from LoggerInspectionHelper");
            logger.LogError("Error test message from LoggerInspectionHelper");
            logger.LogCritical("Critical test message from LoggerInspectionHelper");

            Console.WriteLine(Resources.TestLogEntriesCreated);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGGER INSPECTION] Error during test logging: {ex.Message}");
        }

        Console.WriteLine(Resources.EndTestLogging);
        Console.WriteLine();
    }

    /// <summary>
    /// Performs a comprehensive logging inspection and test.
    /// </summary>
    /// <param name="serviceProvider">The service provider to inspect.</param>
    /// <param name="testCategoryName">Optional category name for test logging.</param>
    public static void PerformFullInspection(IServiceProvider serviceProvider, string testCategoryName = "FullInspectionTest")
    {
        InspectLoggingConfiguration(serviceProvider);
        TestLogging(serviceProvider, testCategoryName);
    }
}
