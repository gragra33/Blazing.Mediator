using Example.Common.Analysis;
using Blazing.Mediator.Statistics;

namespace NotificationHybridExample.Services;

/// <summary>
/// Demo runner service that demonstrates the hybrid notification pattern.
/// Shows both automatic handlers and manual subscribers working together.
/// </summary>
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    ExampleAnalysisService analysisService,
    IServiceProvider serviceProvider)
{
    public async Task RunAsync()
    {
        logger.LogInformation("Starting Hybrid Notification Pattern Demo");
        logger.LogInformation("This demo shows the hybrid approach combining automatic handlers and manual subscribers:");
        logger.LogInformation("  - Automatic discovery of INotificationHandler<T> implementations");
        logger.LogInformation("  - Manual subscription of INotificationSubscriber<T> implementations");
        logger.LogInformation("  - Unified middleware pipeline processing both patterns");
        logger.LogInformation("  - MediatorStatistics for comprehensive analysis and debugging");
        logger.LogInformation("");

        // Display pre-execution analysis
        analysisService.DisplayPreExecutionAnalysis();

        // Inspect notification middleware pipeline
        InspectNotificationMiddlewarePipeline();

        // Create sample orders to demonstrate the hybrid pattern
        var orders = new[]
        {
            new OrderCreatedNotification
            {
                OrderId = "ORD-2024-001",
                CustomerName = "Alice Johnson",
                CustomerEmail = "alice.johnson@example.com",
                TotalAmount = 156.75m,
                Items = 
                [
                    new OrderItem { ProductName = "Wireless Headphones", Quantity = 1, UnitPrice = 89.99m },
                    new OrderItem { ProductName = "Phone Case", Quantity = 2, UnitPrice = 19.99m },
                    new OrderItem { ProductName = "Screen Protector", Quantity = 1, UnitPrice = 26.77m }
                ]
            },
            new OrderCreatedNotification
            {
                OrderId = "ORD-2024-002",
                CustomerName = "Bob Smith",
                CustomerEmail = "bob.smith@example.com",
                TotalAmount = 45.50m,
                Items = 
                [
                    new OrderItem { ProductName = "USB Cable", Quantity = 3, UnitPrice = 12.99m },
                    new OrderItem { ProductName = "Power Adapter", Quantity = 1, UnitPrice = 6.53m }
                ]
            }
        };

        logger.LogInformation("");
        logger.LogInformation("=== PHASE 1: AUTOMATIC HANDLERS ONLY ===");
        logger.LogInformation("Processing orders with automatic handlers (Email & Shipping)...");
        logger.LogInformation("");

        // Process first order with automatic handlers only
        await mediator.Publish(orders[0]);

        logger.LogInformation("");
        logger.LogInformation("Adding Manual Subscribers to the mix...");
        
        // Get and subscribe manual subscribers
        var inventorySubscriber = serviceProvider.GetRequiredService<InventoryNotificationSubscriber>();
        var auditSubscriber = serviceProvider.GetRequiredService<AuditNotificationSubscriber>();

        mediator.Subscribe(inventorySubscriber);
        mediator.Subscribe(auditSubscriber);

        logger.LogInformation("Manual subscribers registered:");
        logger.LogInformation("   • InventoryNotificationSubscriber (manual)");
        logger.LogInformation("   • AuditNotificationSubscriber (manual)");
        logger.LogInformation("");

        logger.LogInformation("UPDATED ANALYSIS (After Manual Subscription - Now Hybrid!):");
        logger.LogInformation("========================================================");
        
        // Get the mediator statistics to show the updated hybrid pattern
        var mediatorStatistics = serviceProvider.GetRequiredService<Blazing.Mediator.Statistics.MediatorStatistics>();
        mediatorStatistics.RenderNotificationAnalysis(serviceProvider, isDetailed: true);
        
        logger.LogInformation("");

        logger.LogInformation("=== PHASE 2: HYBRID PATTERN (HANDLERS + SUBSCRIBERS) ===");
        logger.LogInformation("Processing orders with BOTH automatic handlers AND manual subscribers...");
        logger.LogInformation("");

        // Process second order with both automatic handlers and manual subscribers
        await mediator.Publish(orders[1]);

        logger.LogInformation("");
        logger.LogInformation("HYBRID PATTERN DEMONSTRATION:");
        logger.LogInformation("");
        logger.LogInformation("AUTOMATIC HANDLERS (Zero Configuration):");
        logger.LogInformation("  EmailNotificationHandler - Discovered and executed automatically");
        logger.LogInformation("  ShippingNotificationHandler - Discovered and executed automatically");
        logger.LogInformation("");
        logger.LogInformation("MANUAL SUBSCRIBERS (Explicit Subscription):");
        logger.LogInformation("  InventoryNotificationSubscriber - Subscribed manually, executed on demand");
        logger.LogInformation("  AuditNotificationSubscriber - Subscribed manually, executed on demand");
        logger.LogInformation("");
        logger.LogInformation("MIDDLEWARE PIPELINE:");
        logger.LogInformation("  NotificationLoggingMiddleware (Order: 100)");
        logger.LogInformation("  NotificationMetricsMiddleware (Order: 300)");
        logger.LogInformation("");

        // Display post-execution analysis with detailed statistics
        analysisService.DisplayPostExecutionAnalysis();

        logger.LogInformation("");
        logger.LogInformation("Demo completed! Both automatic handlers and manual subscribers processed all notifications.");
        logger.LogInformation("Notice how the hybrid pattern provides maximum flexibility:");
        logger.LogInformation("  - Automatic handlers for core functionality (always execute)");
        logger.LogInformation("  - Manual subscribers for optional processing (explicit control)");
        logger.LogInformation("  - Unified middleware pipeline for both patterns");
        logger.LogInformation("");
        logger.LogInformation("KEY BENEFITS DEMONSTRATED:");
        logger.LogInformation("  Flexibility - Mix automatic and manual notification handling");
        logger.LogInformation("  Performance - Automatic handlers have zero configuration overhead");
        logger.LogInformation("  Control - Manual subscribers provide explicit control over execution");
        logger.LogInformation("  Scalability - Easy to add new handlers or subscribers as needed");
        logger.LogInformation("  Reliability - Independent error handling for each processing type");
    }

    /// <summary>
    /// Inspects and displays the notification middleware pipeline configuration.
    /// </summary>
    private void InspectNotificationMiddlewarePipeline()
    {
        logger.LogInformation("=== Notification Middleware Pipeline Inspection ===");

        // Get the notification pipeline inspector from the mediator using reflection
        var mediatorType = mediator.GetType();
        var notificationPipelineBuilderField = mediatorType.GetField("_notificationPipelineBuilder", BindingFlags.NonPublic | BindingFlags.Instance);

        if (notificationPipelineBuilderField?.GetValue(mediator) is INotificationMiddlewarePipelineInspector inspector)
        {
            // Use the built-in analysis method from the core library
            var middlewareAnalysis = inspector.AnalyzeMiddleware(serviceProvider);

            logger.LogInformation("Auto-discovered notification middleware (in execution order):");
            foreach (var middleware in middlewareAnalysis)
            {
                logger.LogInformation("  - [{OrderDisplay}] {ClassName}{TypeParameters}",
                    middleware.OrderDisplay,
                    middleware.ClassName,
                    middleware.TypeParameters);
            }

            // Show additional inspection details
            var registeredTypes = inspector.GetRegisteredMiddleware();
            var configurations = inspector.GetMiddlewareConfiguration();

            logger.LogInformation("");
            logger.LogInformation("Total registered notification middleware: {Count}", registeredTypes.Count);

            var configuredMiddleware = configurations.Where(config => config.Configuration != null).ToList();
            if (configuredMiddleware.Any())
            {
                logger.LogInformation("Middleware with configuration:");
                foreach (var config in configuredMiddleware)
                {
                    logger.LogInformation("  - {MiddlewareName}: {Configuration}", config.Type.Name, config.Configuration);
                }
            }
        }
        else
        {
            logger.LogWarning("Could not access notification pipeline inspector");
        }

        logger.LogInformation("");
    }
}