var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        // Register Blazing.Mediator with automatic notification middleware discovery and comprehensive statistics
        // This will automatically find and register:
        //   - NotificationLoggingMiddleware
        //   - NotificationValidationMiddleware  
        //   - NotificationMetricsMiddleware
        //   - NotificationAuditMiddleware
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
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());

        // Register notification subscribers as scoped services
        // These are simple classes that handle notifications when subscribed
        services.AddScoped<EmailNotificationHandler>();
        services.AddScoped<InventoryNotificationHandler>();

        // Configure logging and analysis using Example.Common
        services.AddExampleLogging(LogLevel.Information)
                .AddExampleAnalysis();

        // Register the demo runner
        services.AddScoped<Runner>();
    })
    .Build();

const string separator = "==============================================";

Console.WriteLine(separator);
Console.WriteLine("* Blazing.Mediator - Notification Subscriber Example");
Console.WriteLine(separator);
Console.WriteLine();
Console.WriteLine("This example demonstrates the notification system with:");
Console.WriteLine("  - OrderCreatedNotification - published when orders are created");
Console.WriteLine("  - EmailNotificationHandler - simple subscriber class");
Console.WriteLine("  - InventoryNotificationHandler - simple subscriber class");
Console.WriteLine("  - Auto-discovered notification middleware:");
Console.WriteLine("    * NotificationLoggingMiddleware (order: 100)");
Console.WriteLine("    * NotificationValidationMiddleware (order: 200)");
Console.WriteLine("    * NotificationMetricsMiddleware (order: 300)");
Console.WriteLine("    * NotificationAuditMiddleware (order: 400)");
Console.WriteLine();
Console.WriteLine("Key concepts demonstrated:");
Console.WriteLine("  - Multiple subscribers to the same notification");
Console.WriteLine("  - Simple scoped services (no background services)");
Console.WriteLine("  - Auto-discovery of notification middleware (discoverNotificationMiddleware: true)");
Console.WriteLine("  - Notification pipeline inspection for debugging");
Console.WriteLine("  - Manual subscription at runtime");
Console.WriteLine("  - Error handling in subscribers");
Console.WriteLine("  - Uses recommended scoped mediator lifetime");
Console.WriteLine("  - MediatorStatistics Analyzers for comprehensive debugging and analysis");
Console.WriteLine("  - Performance statistics and real-time execution tracking");
Console.WriteLine("  - Enhanced pattern-aware notification analysis");
Console.WriteLine();
Console.WriteLine("MediatorStatistics & Analysis Features:");
Console.WriteLine("  * Real-time performance statistics for debugging and monitoring");
Console.WriteLine("  * Comprehensive type analysis and handler registration verification");
Console.WriteLine("  * Enhanced notification pattern detection (Manual Subscribers vs Automatic Handlers)");
Console.WriteLine("  * Pipeline inspection tools for troubleshooting middleware configuration");
Console.WriteLine("  * Execution tracking and metrics collection for performance optimization");
Console.WriteLine("  * Subscriber tracking with accurate statistics reporting");
Console.WriteLine();
Console.WriteLine(separator);
Console.WriteLine();

// Create a scope to resolve services and demonstrate the notification system
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

// Get the mediator statistics for enhanced analysis
var mediatorStatistics = services.GetRequiredService<MediatorStatistics>();

// Show notification analysis BEFORE subscription to demonstrate "No subscribers" state
Console.WriteLine("* NOTIFICATION ANALYSIS (Before Subscription):");
mediatorStatistics.RenderNotificationAnalysis(services, isDetailed: false);

// Get the mediator and manually subscribe both handlers
var mediator = services.GetRequiredService<IMediator>();
var emailHandler = services.GetRequiredService<EmailNotificationHandler>();
var inventoryHandler = services.GetRequiredService<InventoryNotificationHandler>();

// Subscribe both handlers to OrderCreatedNotification
mediator.Subscribe(emailHandler);
mediator.Subscribe(inventoryHandler);

Console.WriteLine("* SUBSCRIPTION COMPLETE:");
Console.WriteLine("* EmailNotificationHandler subscribed to OrderCreatedNotification");
Console.WriteLine("* InventoryNotificationHandler subscribed to OrderCreatedNotification");
Console.WriteLine();

// Show notification analysis AFTER subscription to demonstrate enhanced tracking
Console.WriteLine("* ENHANCED NOTIFICATION ANALYSIS (After Subscription):");
mediatorStatistics.RenderNotificationAnalysis(services, isDetailed: true);

// Run the demo
var runner = services.GetRequiredService<Runner>();
await runner.RunAsync();

Console.WriteLine();
Console.WriteLine("* FINAL NOTIFICATION STATISTICS:");
mediatorStatistics.RenderNotificationAnalysis(services, isDetailed: false);

Console.WriteLine();
Console.WriteLine("@ Demo finished. Enhanced MediatorStatistics show accurate subscriber tracking!");
Console.WriteLine("@ This demonstrates the fix for issue: NotificationSubscriberExample no longer reports '0 handlers'");
Console.WriteLine("@ Press any key to exit...");
Console.ReadKey();
