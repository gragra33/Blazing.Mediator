var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register the request handlers and middleware with comprehensive statistics tracking
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
                {
                    options.EnableRequestMetrics = true;
                    options.EnableNotificationMetrics = true;
                    options.EnableMiddlewareMetrics = true;
                    options.EnablePerformanceCounters = true;  // Enable performance counters!
                    options.EnableDetailedAnalysis = true;     // Enable detailed analysis!
                    options.MetricsRetentionPeriod = TimeSpan.FromHours(1);
                    options.CleanupInterval = TimeSpan.FromMinutes(15);
                })
                  .WithMiddlewareDiscovery()
                  .WithoutLogging()
                  .WithoutTelemetry();

        }, Assembly.GetExecutingAssembly());

        // Register FluentValidation services
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging()
                .AddExampleAnalysis();

        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("Blazing.Mediator Middleware Pipeline Demo");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This project demonstrates the core features of Blazing.Mediator");
Console.WriteLine("with comprehensive middleware pipeline and analysis capabilities.");
Console.WriteLine();
Console.WriteLine("It includes a simple e-commerce scenario with product lookup,");
Console.WriteLine("inventory management, order confirmation, and customer registration.");
Console.WriteLine();
Console.WriteLine("Key Features Demonstrated:");
Console.WriteLine("* MediatorStatistics Analyzers for debugging and performance analysis");
Console.WriteLine("* Real-time performance statistics and execution tracking");
Console.WriteLine("* Middleware pipeline inspection and analysis");
Console.WriteLine("* CQRS pattern implementation with type-safe operations");
Console.WriteLine("* Advanced error handling, validation, and business operation auditing");
Console.WriteLine();
Console.WriteLine("The example showcases how to use Blazing.Mediator with middleware");
Console.WriteLine("for error handling, validation, and logging while providing");
Console.WriteLine("comprehensive debugging and analysis tools for development.");
Console.WriteLine();

Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var runner = services.GetRequiredService<Runner>();

// Inspect middleware pipeline before running the demo
runner.InspectMiddlewarePipeline();

await runner.Run();
