var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with both automatic handler discovery AND support for manual subscribers
        // This hybrid configuration demonstrates the best of both worlds:
        //   - Automatic discovery for INotificationHandler<T> implementations
        //   - Manual subscription support for INotificationSubscriber<T> implementations
        services.AddMediator(config =>
        {
            config.WithStatisticsTracking(options =>
                {
                    options.EnableRequestMetrics = true;
                    options.EnableNotificationMetrics = true;
                    options.EnableMiddlewareMetrics = true;
                    options.EnablePerformanceCounters = true;
                    options.EnableDetailedAnalysis = true;
                    options.MetricsRetentionPeriod = TimeSpan.FromHours(1);
                    options.CleanupInterval = TimeSpan.FromMinutes(15);
                })
                  .WithNotificationHandlerDiscovery()    // Enable automatic handler discovery
                  .WithNotificationMiddlewareDiscovery(); // Enable automatic middleware discovery
        }, Assembly.GetExecutingAssembly());

        // Register manual notification subscribers as scoped services
        // These require explicit subscription but offer more control
        services.AddScoped<InventoryNotificationSubscriber>();
        services.AddScoped<AuditNotificationSubscriber>();

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging(LogLevel.Information)
                .AddExampleAnalysis();

        // Register the demo runner service
        services.AddScoped<DemoRunner>();
    })
    .Build();

const string separator = "================================================================================";

Console.WriteLine(separator);
Console.WriteLine("*** Blazing.Mediator - HYBRID Notification Pattern Example ***");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates the HYBRID notification pattern combining:");
Console.WriteLine();
Console.WriteLine("AUTOMATIC HANDLERS (Zero Configuration):");
Console.WriteLine("  [AUTO] EmailNotificationHandler - Discovered and registered automatically");
Console.WriteLine("  [AUTO] ShippingNotificationHandler - Discovered and registered automatically");
Console.WriteLine("  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>");
Console.WriteLine();
Console.WriteLine("MANUAL SUBSCRIBERS (Explicit Control):");
Console.WriteLine("  [MANUAL] InventoryNotificationSubscriber - Requires manual subscription");
Console.WriteLine("  [MANUAL] AuditNotificationSubscriber - Requires manual subscription");
Console.WriteLine("  [CONTROL] EXPLICIT SUBSCRIPTION - Full control over when and how to subscribe");
Console.WriteLine();
Console.WriteLine("HYBRID BENEFITS:");
Console.WriteLine("  [FLEX] MAXIMUM FLEXIBILITY - Use the right pattern for each scenario");
Console.WriteLine("  [PERF] OPTIMAL PERFORMANCE - Automatic handlers have zero overhead");
Console.WriteLine("  [CTRL] FINE-GRAINED CONTROL - Manual subscribers for complex scenarios");
Console.WriteLine("  [SCALE] EASY SCALING - Mix and match as your application grows");
Console.WriteLine();
Console.WriteLine("AUTOMATIC MIDDLEWARE DISCOVERY:");
Console.WriteLine("  [100] NotificationLoggingMiddleware (Order: 100)");
Console.WriteLine("  [300] NotificationMetricsMiddleware (Order: 300)");
Console.WriteLine();
Console.WriteLine("MEDIATORSTATISTICS & ANALYSIS:");
Console.WriteLine("  * Real-time execution statistics for both handlers and subscribers");
Console.WriteLine("  * Performance counters and detailed analysis for optimization");
Console.WriteLine("  * Pipeline inspection tools for troubleshooting mixed patterns");
Console.WriteLine("  * Registration verification for both automatic and manual components");
Console.WriteLine();
Console.WriteLine("USE CASES FOR HYBRID PATTERN:");
Console.WriteLine("  [AUTO] Core business logic - Use automatic handlers for consistency");
Console.WriteLine("  [MANUAL] Optional features - Use manual subscribers for flexibility");
Console.WriteLine("  [AUTO] Always-on services - Email, logging, metrics");
Console.WriteLine("  [MANUAL] Conditional services - Audit, special processing, integrations");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope and run the demo
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

Console.WriteLine(">> HYBRID CONFIGURATION STATUS:");
Console.WriteLine("  >> Automatic handler discovery enabled - handlers will be discovered automatically");
Console.WriteLine("  >> Manual subscriber services registered - ready for explicit subscription");
Console.WriteLine("  >> MediatorStatistics enabled for comprehensive analysis of both patterns");
Console.WriteLine("  >> Middleware pipeline configured for unified processing");
Console.WriteLine();

// Run the demo
var runner = services.GetRequiredService<DemoRunner>();

await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("*** HYBRID PATTERN DEMONSTRATION COMPLETE! ***");
Console.WriteLine();
Console.WriteLine("What you just experienced:");
Console.WriteLine("  [OK] Automatic handlers executed with zero configuration");
Console.WriteLine("  [OK] Manual subscribers executed with explicit control");
Console.WriteLine("  [OK] Unified middleware pipeline processing both patterns");
Console.WriteLine("  [OK] Comprehensive statistics tracking for both approaches");
Console.WriteLine("  [OK] Maximum flexibility - use the right tool for each job");
Console.WriteLine();
Console.WriteLine("Pattern Decision Guide:");
Console.WriteLine("  * Use AUTOMATIC HANDLERS when:");
Console.WriteLine("     • Core business logic that should always execute");
Console.WriteLine("     • Simple, stateless processing");
Console.WriteLine("     • You want zero configuration overhead");
Console.WriteLine("     • The logic is tightly coupled to the notification");
Console.WriteLine();
Console.WriteLine("  * Use MANUAL SUBSCRIBERS when:");
Console.WriteLine("     • Optional or conditional processing");
Console.WriteLine("     • Complex initialization or setup required");
Console.WriteLine("     • Dynamic subscription/unsubscription needed");
Console.WriteLine("     • Integration with external systems or legacy code");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  * Experiment with adding your own handlers and subscribers");
Console.WriteLine("  * Try dynamic subscription/unsubscription scenarios");
Console.WriteLine("  * Monitor the performance differences using MediatorStatistics");
Console.WriteLine("  * Use this pattern in production for maximum flexibility");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();