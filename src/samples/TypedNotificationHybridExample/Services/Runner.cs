using Example.Common.Analysis;
using Blazing.Mediator.Statistics;

namespace TypedNotificationHybridExample.Services;

/// <summary>
/// Runner service that demonstrates the typed notification hybrid pattern.
/// Shows both automatic handlers and manual subscribers working together with type constraints.
/// </summary>
public class Runner(
    IMediator mediator,
    ILogger<Runner> logger,
    ExampleAnalysisService analysisService,
    IServiceProvider serviceProvider)
{
    public async Task RunAsync()
    {
        logger.LogInformation("Starting TYPED Hybrid Notification Pattern Demo");
        logger.LogInformation("This demo shows the typed hybrid approach with type-constrained middleware:");
        logger.LogInformation("  - Automatic discovery of INotificationHandler<T> implementations");
        logger.LogInformation("  - Type-constrained middleware (INotificationMiddleware<T>) for specific categories");
        logger.LogInformation("  - Manual subscription of INotificationSubscriber<T> implementations");
        logger.LogInformation("  - MediatorStatistics for comprehensive analysis of mixed patterns");
        logger.LogInformation("");

        // Display pre-execution analysis
        analysisService.DisplayPreExecutionAnalysis();

        // Inspect notification middleware pipeline with type constraints
        InspectTypedNotificationMiddlewarePipeline();

        // Create sample typed notifications to demonstrate the hybrid pattern
        var customerNotification = new CustomerRegisteredNotification
        {
            CustomerId = "CUST-2024-001",
            CustomerName = "Alice Johnson",
            CustomerEmail = "alice.johnson@example.com",
            RegistrationSource = "Website"
        };

        var orderNotification = new OrderCreatedNotification
        {
            OrderId = "ORD-2024-001",
            CustomerName = "Alice Johnson",
            CustomerEmail = "alice.johnson@example.com",
            Items = new List<OrderItem>
            {
                new() { ProductName = "Wireless Headphones", Quantity = 1, UnitPrice = 89.99m },
                new() { ProductName = "Phone Case", Quantity = 2, UnitPrice = 19.99m }
            },
            TotalAmount = 109.98m
        };

        var inventoryNotification = new InventoryUpdatedNotification
        {
            ProductId = "PROD-2024-001",
            ProductName = "Wireless Headphones",
            PreviousQuantity = 50,
            NewQuantity = 49,
            UpdateReason = "Order fulfillment"
        };

        logger.LogInformation("");
        logger.LogInformation("=== PHASE 1: AUTOMATIC HANDLERS ONLY ===");
        logger.LogInformation("Processing notifications with automatic handlers and type-constrained middleware...");
        logger.LogInformation("");

        // Process notifications with automatic handlers only
        await mediator.Publish(customerNotification);
        await Task.Delay(500); // Small delay for better demo flow

        logger.LogInformation("");
        logger.LogInformation("Adding Manual Subscribers to the mix...");
        
        // Get and subscribe manual subscribers
        var integrationSubscriber = serviceProvider.GetRequiredService<IntegrationNotificationSubscriber>();
        var auditSubscriber = serviceProvider.GetRequiredService<AuditNotificationSubscriber>();

        // Subscribe to specific notification types
        mediator.Subscribe<OrderCreatedNotification>(integrationSubscriber);
        mediator.Subscribe<CustomerRegisteredNotification>(integrationSubscriber);
        
        mediator.Subscribe<OrderCreatedNotification>(auditSubscriber);
        mediator.Subscribe<CustomerRegisteredNotification>(auditSubscriber);
        mediator.Subscribe<InventoryUpdatedNotification>(auditSubscriber);

        logger.LogInformation("Manual subscribers registered:");
        logger.LogInformation("   • IntegrationNotificationSubscriber (manual)");
        logger.LogInformation("   • AuditNotificationSubscriber (manual)");
        logger.LogInformation("");

        logger.LogInformation("UPDATED ANALYSIS (After Manual Subscription - Now Hybrid!):");
        logger.LogInformation("========================================================");
        
        // Get the mediator statistics to show the updated hybrid pattern
        var mediatorStatistics = serviceProvider.GetRequiredService<Blazing.Mediator.Statistics.MediatorStatistics>();
        mediatorStatistics.RenderNotificationAnalysis(serviceProvider, isDetailed: true);
        
        logger.LogInformation("");

        logger.LogInformation("=== PHASE 2: TYPED HYBRID PATTERN (HANDLERS + SUBSCRIBERS) ===");
        logger.LogInformation("Processing different notification types with BOTH automatic handlers AND manual subscribers...");
        logger.LogInformation("Notice how type-constrained middleware executes selectively based on notification interfaces.");
        logger.LogInformation("");

        // Process different types of notifications with both automatic handlers and manual subscribers
        await mediator.Publish(orderNotification);
        await Task.Delay(500);
        await mediator.Publish(inventoryNotification);

        logger.LogInformation("");
        logger.LogInformation("TYPED HYBRID PATTERN DEMONSTRATION:");
        logger.LogInformation("");
        logger.LogInformation("AUTOMATIC HANDLERS (Zero Configuration, Type-Constrained):");
        logger.LogInformation("  EmailNotificationHandler - Handles ICustomerNotification automatically");
        logger.LogInformation("  BusinessOperationsHandler - Handles multiple notification types automatically");
        logger.LogInformation("");
        logger.LogInformation("MANUAL SUBSCRIBERS (Explicit Subscription, Flexible):");
        logger.LogInformation("  IntegrationNotificationSubscriber - Handles specific notification types manually");
        logger.LogInformation("  AuditNotificationSubscriber - Handles all notification types manually");
        logger.LogInformation("");
        logger.LogInformation("TYPE-CONSTRAINED MIDDLEWARE (Selective Processing):");
        logger.LogInformation("  CustomerNotificationMiddleware - Only processes ICustomerNotification");
        logger.LogInformation("  OrderNotificationMiddleware - Only processes IOrderNotification");
        logger.LogInformation("  InventoryNotificationMiddleware - Only processes IInventoryNotification");
        logger.LogInformation("  GeneralNotificationMiddleware - Processes all notification types");
        logger.LogInformation("");

        // Display post-execution analysis with detailed statistics
        analysisService.DisplayPostExecutionAnalysis();

        logger.LogInformation("");
        logger.LogInformation("Demo completed! Type-constrained hybrid pattern processed all notifications efficiently.");
        logger.LogInformation("Notice how middleware executed selectively based on notification interfaces:");
        logger.LogInformation("  - CustomerNotificationMiddleware only ran for CustomerRegisteredNotification");
        logger.LogInformation("  - OrderNotificationMiddleware only ran for OrderCreatedNotification");
        logger.LogInformation("  - InventoryNotificationMiddleware only ran for InventoryUpdatedNotification");
        logger.LogInformation("  - GeneralNotificationMiddleware ran for all notification types");
        logger.LogInformation("");
        logger.LogInformation("KEY BENEFITS DEMONSTRATED:");
        logger.LogInformation("  Type Safety - Automatic handlers constrained to specific interfaces");
        logger.LogInformation("  Performance - Type-specific middleware only executes for relevant notifications");
        logger.LogInformation("  Flexibility - Manual subscribers can handle any notification type");
        logger.LogInformation("  Scalability - Easy to add new handlers or subscribers as needed");
        logger.LogInformation("  Reliability - Type-safe processing with independent error handling");
        logger.LogInformation("  Architecture - Clean separation of concerns with typed interfaces");
    }

    /// <summary>
    /// Inspects and displays the typed notification middleware pipeline configuration.
    /// </summary>
    private void InspectTypedNotificationMiddlewarePipeline()
    {
        logger.LogInformation("=== Typed Notification Middleware Pipeline Inspection ===");

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

                // Show type constraints for type-constrained middleware
                if (!string.IsNullOrEmpty(middleware.GenericConstraints))
                {
                    logger.LogInformation("        - Constraints: {GenericConstraints}", middleware.GenericConstraints);
                }
            }

            // Show additional inspection details
            var registeredTypes = inspector.GetRegisteredMiddleware();
            var configurations = inspector.GetMiddlewareConfiguration();

            logger.LogInformation("");
            logger.LogInformation("Total registered notification middleware: {Count}", registeredTypes.Count);
            logger.LogInformation("Type-constrained middleware: {Count}", 
                middlewareAnalysis.Count(m => !string.IsNullOrEmpty(m.GenericConstraints)));
            logger.LogInformation("General middleware: {Count}", 
                middlewareAnalysis.Count(m => string.IsNullOrEmpty(m.GenericConstraints)));

            var configuredMiddleware = configurations.Where(config => config.Configuration != null).ToList();
            if (configuredMiddleware.Any())
            {
                logger.LogInformation("");
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