var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register the request handlers and middleware with comprehensive statistics tracking
        // Use auto-discovery to demonstrate type constraint support
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
                  .WithMiddlewareDiscovery(); // Enable auto-discovery with type constraint support
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
Console.WriteLine("Blazing.Mediator TypedMiddlewareExample");
Console.WriteLine(separator);
Console.WriteLine();

Console.WriteLine("This project demonstrates custom type constraints with domain-specific interfaces");
Console.WriteLine("in Blazing.Mediator middleware with comprehensive analysis capabilities.");
Console.WriteLine();
Console.WriteLine("Key Features:");
Console.WriteLine("* Product requests use IProductRequest<T> interface");
Console.WriteLine("* Customer requests use ICustomerRequest and ICustomerRequest<T> interfaces");
Console.WriteLine("* Inventory requests use IInventoryRequest<T> interface");
Console.WriteLine("* Order requests use IOrderRequest interface");
Console.WriteLine("* Validation middleware is constrained to ICustomerRequest only");
Console.WriteLine("* Monitoring middleware is constrained to IProductRequest only");
Console.WriteLine("* Product queries bypass validation middleware (not customer requests)");
Console.WriteLine("* Clear logging shows the distinction between different domain request processing");
Console.WriteLine("* MediatorStatistics Analyzers provide comprehensive debugging and analysis");
Console.WriteLine("* Performance statistics enable real-time execution tracking and optimization");
Console.WriteLine();
Console.WriteLine("MediatorStatistics & Analysis Features:");
Console.WriteLine("* Real-time performance statistics for debugging and monitoring");
Console.WriteLine("* Type constraint analysis and middleware execution verification");
Console.WriteLine("* Pipeline inspection tools for troubleshooting domain-specific routing");
Console.WriteLine("* Handler registration analysis and comprehensive type mapping");
Console.WriteLine();
Console.WriteLine("The example showcases how custom type constraints can be used to apply");
Console.WriteLine("middleware selectively based on domain-specific request interfaces while");
Console.WriteLine("providing comprehensive debugging and analysis tools for development.");
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
