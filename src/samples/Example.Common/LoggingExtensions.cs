namespace Example.Common;

/// <summary>
/// Extension methods for configuring logging in sample applications with consistent formatting.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures standard logging for example applications with the SimpleConsoleFormatter.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="minimumLevel">The minimum log level (default: Debug).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddExampleLogging(this IServiceCollection services, LogLevel minimumLevel = LogLevel.Debug)
    {
        // Configure console encoding for proper character display
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch
        {
            // Ignore if unable to set encoding (some environments don't support it)
        }

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole(options =>
            {
                options.FormatterName = "SimpleClassName";
            });
            logging.AddConsoleFormatter<SimpleConsoleFormatter, Logging.SimpleConsoleFormatterOptions>();
            logging.SetMinimumLevel(minimumLevel);
        });

        // Configure the host to use the console output
        services.AddSingleton(Console.Out);

        return services;
    }

    /// <summary>
    /// Adds the example analysis service for common middleware and mediator analysis.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddExampleAnalysis(this IServiceCollection services)
    {
        services.AddScoped<ExampleAnalysisService>();
        return services;
    }
}