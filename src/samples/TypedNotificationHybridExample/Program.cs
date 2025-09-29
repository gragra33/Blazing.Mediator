var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with both automatic handler discovery AND support for manual subscribers
        // This typed hybrid configuration demonstrates type-constrained middleware with mixed patterns:
        //   - Automatic discovery for INotificationHandler<T> implementations
        //   - Type-constrained middleware (INotificationMiddleware<T>) for specific notification categories
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
                  .WithNotificationMiddlewareDiscovery(); // Enable automatic middleware discovery (including type-constrained)
        }, Assembly.GetExecutingAssembly());

        // Register manual notification subscribers as scoped services
        // These require explicit subscription but offer more control over execution
        services.AddScoped<AuditNotificationSubscriber>();
        services.AddScoped<IntegrationNotificationSubscriber>();

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging(LogLevel.Information)
                .AddExampleAnalysis();

        // Register the demo runner service
        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "================================================================================";

Console.WriteLine(separator);
Console.WriteLine("*** Blazing.Mediator - TYPED HYBRID Notification Pattern Example ***");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates the TYPED HYBRID notification pattern combining:");
Console.WriteLine();
Console.WriteLine("AUTOMATIC HANDLERS (Zero Configuration):");
Console.WriteLine("  [AUTO] EmailNotificationHandler - Handles OrderCreated + CustomerRegistered");
Console.WriteLine("  [AUTO] BusinessOperationsHandler - Handles all notification types automatically");
Console.WriteLine("  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>");
Console.WriteLine();
Console.WriteLine("MANUAL SUBSCRIBERS (Explicit Control):");
Console.WriteLine("  [MANUAL] AuditNotificationSubscriber - Handles all types (requires subscription)");
Console.WriteLine("  [MANUAL] IntegrationNotificationSubscriber - Handles OrderCreated + CustomerRegistered");
Console.WriteLine("  [CONTROL] EXPLICIT SUBSCRIPTION - Full control over when and how to subscribe");
Console.WriteLine();
Console.WriteLine("TYPE-CONSTRAINED MIDDLEWARE (Selective Processing):");
Console.WriteLine("  [TYPE] OrderNotificationMiddleware - IOrderNotification only");
Console.WriteLine("  [TYPE] CustomerNotificationMiddleware - ICustomerNotification only");
Console.WriteLine("  [TYPE] InventoryNotificationMiddleware - IInventoryNotification only");
Console.WriteLine("  [ALL] GeneralNotificationMiddleware - All notification types");
Console.WriteLine();
Console.WriteLine("NOTIFICATION CATEGORIES:");
Console.WriteLine("  * IOrderNotification - Order-related events (OrderCreatedNotification)");
Console.WriteLine("  * ICustomerNotification - Customer events (CustomerRegisteredNotification)");
Console.WriteLine("  * IInventoryNotification - Inventory events (InventoryUpdatedNotification)");
Console.WriteLine();
Console.WriteLine("TYPED HYBRID BENEFITS:");
Console.WriteLine("  [PERF] OPTIMAL PERFORMANCE - Middleware executes only for relevant types");
Console.WriteLine("  [FLEX] MAXIMUM FLEXIBILITY - Use the right pattern for each scenario");
Console.WriteLine("  [SAFE] TYPE SAFETY - Compile-time guarantees for middleware constraints");
Console.WriteLine("  [CTRL] FINE-GRAINED CONTROL - Manual subscribers for complex scenarios");
Console.WriteLine("  [SCALE] EASY SCALING - Mix and match patterns as your application grows");
Console.WriteLine();
Console.WriteLine("MEDIATORSTATISTICS & ANALYSIS:");
Console.WriteLine("  * Real-time execution statistics for handlers, subscribers, and middleware");
Console.WriteLine("  * Type constraint analysis and middleware execution verification");
Console.WriteLine("  * Pipeline inspection tools for troubleshooting mixed patterns");
Console.WriteLine("  * Registration verification for both automatic and manual components");
Console.WriteLine();
Console.WriteLine("USE CASES FOR TYPED HYBRID PATTERN:");
Console.WriteLine("  [AUTO] Core business logic - Use automatic handlers for consistency");
Console.WriteLine("  [MANUAL] Optional features - Use manual subscribers for flexibility");
Console.WriteLine("  [TYPE] Category-specific processing - Use type-constrained middleware");
Console.WriteLine("  [ALL] Cross-cutting concerns - Use general middleware for all types");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope and run the demo
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

Console.WriteLine(">> TYPED HYBRID CONFIGURATION STATUS:");
Console.WriteLine("  >> Automatic handler discovery enabled - handlers will be discovered automatically");
Console.WriteLine("  >> Type-constrained middleware discovery enabled - selective processing by type");
Console.WriteLine("  >> Manual subscriber services registered - ready for explicit subscription");
Console.WriteLine("  >> MediatorStatistics enabled for comprehensive analysis of all patterns");
Console.WriteLine("  >> Unified pipeline configured for processing multiple notification categories");
Console.WriteLine();

// Run the demo
var runner = services.GetRequiredService<Runner>();

await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("*** TYPED HYBRID PATTERN DEMONSTRATION COMPLETE! ***");
Console.WriteLine();
Console.WriteLine("What you just experienced:");
Console.WriteLine("  [OK] Automatic handlers executed with zero configuration");
Console.WriteLine("  [OK] Type-constrained middleware executed selectively by notification category");
Console.WriteLine("  [OK] Manual subscribers executed with explicit control");
Console.WriteLine("  [OK] Unified pipeline processing multiple notification types efficiently");
Console.WriteLine("  [OK] Maximum flexibility with optimal performance");
Console.WriteLine();
Console.WriteLine("Pattern Decision Guide:");
Console.WriteLine("  *Use AUTOMATIC HANDLERS when:");
Console.WriteLine("     • Core business logic that should always execute");
Console.WriteLine("     • Simple, stateless processing");
Console.WriteLine("     • You want zero configuration overhead");
Console.WriteLine("     • The logic is tightly coupled to the notification");
Console.WriteLine();
Console.WriteLine("  *Use MANUAL SUBSCRIBERS when:");
Console.WriteLine("     • Optional or conditional processing");
Console.WriteLine("     • Complex initialization or setup required");
Console.WriteLine("     • Dynamic subscription/unsubscription needed");
Console.WriteLine("     • Integration with external systems or legacy code");
Console.WriteLine();
Console.WriteLine("  *Use TYPE-CONSTRAINED MIDDLEWARE when:");
Console.WriteLine("     • Category-specific validation or processing");
Console.WriteLine("     • Performance optimization (avoid unnecessary middleware execution)");
Console.WriteLine("     • Type-safe processing with compile-time guarantees");
Console.WriteLine("     • Different processing logic for different notification categories");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  * Experiment with creating new notification categories and interfaces");
Console.WriteLine("  * Try adding your own type-constrained middleware");
Console.WriteLine("  * Monitor the performance benefits using MediatorStatistics");
Console.WriteLine("  * Use this pattern in production for maximum flexibility and performance");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();