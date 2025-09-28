var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic handler discovery and comprehensive statistics
        // This will automatically discover and register:
        //   - All INotificationHandler<T> implementations in this assembly
        //   - All notification middleware with automatic ordering
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
                  .WithNotificationHandlerDiscovery() // Enable automatic handler discovery
                  .WithNotificationMiddlewareDiscovery(); // Enable automatic middleware discovery
        }, Assembly.GetExecutingAssembly());

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging(LogLevel.Information)
                .AddExampleAnalysis();

        // Register the demo runner service
        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "==============================================================================­";

Console.WriteLine(separator);
Console.WriteLine("*** Blazing.Mediator - Notification Handler Example (Automatic Discovery) ***");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates the NEW automatic notification handler pattern:");
Console.WriteLine();
Console.WriteLine("KEY FEATURES:");
Console.WriteLine("  [AUTO] AUTOMATIC DISCOVERY - Handlers are discovered and registered automatically");
Console.WriteLine("  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>");
Console.WriteLine("  [MULTI] MULTIPLE HANDLERS - Multiple handlers process the same notification");
Console.WriteLine("  [PIPE] MIDDLEWARE PIPELINE - Validation, logging, metrics automatically applied");
Console.WriteLine("  [SCALE] SCALABLE ARCHITECTURE - Easy to add new handlers without configuration");
Console.WriteLine("  [STATS] MediatorStatistics Analyzers - Comprehensive debugging and performance analysis");
Console.WriteLine("  [PERF] Performance Statistics - Real-time execution tracking and metrics collection");
Console.WriteLine();
Console.WriteLine("AUTOMATIC HANDLER DISCOVERY:");
Console.WriteLine("  * EmailNotificationHandler - Sends confirmation emails");
Console.WriteLine("  * InventoryNotificationHandler - Updates inventory and stock alerts");
Console.WriteLine("  * AuditNotificationHandler - Logs for compliance and auditing");
Console.WriteLine("  * ShippingNotificationHandler - Handles shipping and fulfillment");
Console.WriteLine();
Console.WriteLine("AUTOMATIC MIDDLEWARE DISCOVERY:");
Console.WriteLine("  [100] NotificationLoggingMiddleware (Order: 100)");
Console.WriteLine("  [200] NotificationValidationMiddleware (Order: 200)");
Console.WriteLine("  [300] NotificationMetricsMiddleware (Order: 300)");
Console.WriteLine();
Console.WriteLine("MEDIATORSTATISTICS & ANALYSIS:");
Console.WriteLine("  * Real-time execution statistics for debugging and monitoring");
Console.WriteLine("  * Performance counters and detailed analysis for optimization");
Console.WriteLine("  * Pipeline inspection tools for troubleshooting middleware issues");
Console.WriteLine("  * Handler registration verification and comprehensive type analysis");
Console.WriteLine();
Console.WriteLine("COMPARED TO NOTIFICATION SUBSCRIBERS:");
Console.WriteLine("  [-] Subscribers: Require manual subscription with mediator.Subscribe()");
Console.WriteLine("  [+] Handlers: Automatically discovered and invoked - no manual setup!");
Console.WriteLine();
Console.WriteLine("  [-] Subscribers: Need to remember to subscribe each handler");
Console.WriteLine("  [+] Handlers: Just implement the interface - that's it!");
Console.WriteLine();
Console.WriteLine("  [-] Subscribers: Runtime subscription management complexity");
Console.WriteLine("  [+] Handlers: Compile-time registration - better performance and reliability");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope and run the demo
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

Console.WriteLine(">> DISCOVERY STATUS:");
Console.WriteLine("  >> Handler discovery and registration completed automatically");
Console.WriteLine("  >> All handlers registered and ready for automatic invocation");
Console.WriteLine("  >> MediatorStatistics enabled for real-time analysis and debugging");
Console.WriteLine();

// Run the demo
var demoRunner = services.GetRequiredService<Runner>();
await demoRunner.RunAsync();

Console.WriteLine();
Console.WriteLine("*** DEMONSTRATION COMPLETE! ***");
Console.WriteLine();
Console.WriteLine("What you just saw:");
Console.WriteLine("  [OK] Zero configuration - all handlers discovered automatically");
Console.WriteLine("  [OK] Multiple handlers executed for each notification");
Console.WriteLine("  [OK] Complete middleware pipeline with validation, logging, and metrics");
Console.WriteLine("  [OK] High performance - no runtime subscription overhead");
Console.WriteLine("  [OK] Scalable - easy to add more handlers by just implementing INotificationHandler<T>");
Console.WriteLine("  [OK] MediatorStatistics Analyzers - Comprehensive debugging and analysis tools");
Console.WriteLine("  [OK] Performance statistics - Real-time execution tracking for optimization");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  * Try adding your own INotificationHandler<OrderCreatedNotification>");
Console.WriteLine("  * Experiment with different middleware orders and behaviors");
Console.WriteLine("  * Check out the metrics collection and performance tracking");
Console.WriteLine("  * Use MediatorStatistics for debugging and analyzing your application");
Console.WriteLine("  * Compare with the NotificationSubscriberExample for differences");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();