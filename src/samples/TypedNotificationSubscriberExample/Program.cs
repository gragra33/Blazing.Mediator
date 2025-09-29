var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic notification middleware discovery
        // and type constraint support with comprehensive statistics
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
                  .WithNotificationMiddlewareDiscovery(); // Enable auto-discovery with type constraints
        }, Assembly.GetExecutingAssembly());

        // Register notification subscribers as scoped services
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<InventoryNotificationHandler>();
        services.AddScoped<BusinessOperationsHandler>();
        services.AddScoped<AuditNotificationHandler>();

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging(LogLevel.Information)
                .AddExampleAnalysis();

        // Register the demo runner
        services.AddScoped<Runner>();

        // Register the pipeline displayer
        services.AddScoped<NotificationPipelineDisplayer>();
    })
    .Build();

const string separator = "================================================================================";

Console.WriteLine(separator);
Console.WriteLine("*** Blazing.Mediator - Typed Notification Subscriber Example ***");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates type-constrained notification middleware with");
Console.WriteLine("manual subscription patterns and comprehensive analysis capabilities:");
Console.WriteLine();
Console.WriteLine("KEY FEATURES:");
Console.WriteLine("  [TYPE] TYPE-CONSTRAINED MIDDLEWARE - Selective processing based on interfaces");
Console.WriteLine("  [SUBS] MANUAL SUBSCRIPTION PATTERN - Runtime subscription management");
Console.WriteLine("  [CATS] NOTIFICATION CATEGORIZATION - Order, Customer, and Inventory notifications");
Console.WriteLine("  [PERF] PERFORMANCE OPTIMIZATION - Middleware executes only for relevant types");
Console.WriteLine("  [STATS] MediatorStatistics Analyzers - Comprehensive debugging and analysis");
Console.WriteLine("  [DEBUG] Performance Statistics - Real-time execution tracking and metrics");
Console.WriteLine();
Console.WriteLine("NOTIFICATION CATEGORIES:");
Console.WriteLine("  * IOrderNotification - Order-related events and processing");
Console.WriteLine("  * ICustomerNotification - Customer management and lifecycle events");
Console.WriteLine("  * IInventoryNotification - Stock management and warehouse operations");
Console.WriteLine();
Console.WriteLine("TYPE-CONSTRAINED MIDDLEWARE:");
Console.WriteLine("  [100] OrderNotificationMiddleware (IOrderNotification only)");
Console.WriteLine("  [200] CustomerNotificationMiddleware (ICustomerNotification only)");
Console.WriteLine("  [300] InventoryNotificationMiddleware (IInventoryNotification only)");
Console.WriteLine("  [400] GeneralNotificationMiddleware (all notifications)");
Console.WriteLine();
Console.WriteLine("SUBSCRIPTION PATTERN:");
Console.WriteLine("  * Manual subscription using mediator.Subscribe() for precise control");
Console.WriteLine("  * Runtime subscription management for dynamic handler registration");
Console.WriteLine("  * Scoped service lifetime for proper resource management");
Console.WriteLine();
Console.WriteLine("MEDIATORSTATISTICS & ANALYSIS:");
Console.WriteLine("  * Real-time notification statistics for debugging and monitoring");
Console.WriteLine("  * Type constraint analysis and middleware execution verification");
Console.WriteLine("  * Pipeline inspection tools for troubleshooting notification routing");
Console.WriteLine("  * Subscription pattern analysis and handler registration verification");
Console.WriteLine("  * Performance counters and detailed metrics for optimization");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services and demonstrate the notification system
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

// Display pipeline information using displayer
var displayer = services.GetRequiredService<NotificationPipelineDisplayer>();
displayer.DisplayPipelineInfo();

Console.WriteLine(">> SUBSCRIPTION STATUS:");
Console.WriteLine("  >> Manual subscription pattern active for precise control");
Console.WriteLine("  >> MediatorStatistics enabled for comprehensive analysis and debugging");
Console.WriteLine("  >> Performance tracking active for real-time execution monitoring");
Console.WriteLine();

// Run the demo
var runner = services.GetRequiredService<Runner>();
await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("*** DEMONSTRATION COMPLETE! ***");
Console.WriteLine();
Console.WriteLine("What you just saw:");
Console.WriteLine("  [OK] Type-constrained middleware executing selectively based on interfaces");
Console.WriteLine("  [OK] Manual subscription pattern providing precise control over handlers");
Console.WriteLine("  [OK] Performance optimization through targeted middleware execution");
Console.WriteLine("  [OK] Comprehensive notification categorization and routing");
Console.WriteLine("  [OK] MediatorStatistics Analyzers providing debugging and analysis tools");
Console.WriteLine("  [OK] Real-time performance statistics for execution monitoring");
Console.WriteLine();
Console.WriteLine("Next steps:");
Console.WriteLine("  * Compare with TypedNotificationHandlerExample for automatic vs manual patterns");
Console.WriteLine("  * Experiment with different subscription strategies and lifecycle management");
Console.WriteLine("  * Use MediatorStatistics for debugging and analyzing notification patterns");
Console.WriteLine("  * Monitor performance statistics to optimize notification processing");
Console.WriteLine();
Console.WriteLine("Demo finished. Press any key to exit...");
Console.ReadKey();