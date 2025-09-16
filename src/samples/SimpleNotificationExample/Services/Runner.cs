using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Statistics;

namespace SimpleNotificationExample.Services;

/// <summary>
/// Runner service that demonstrates the notification system in action.
/// This orchestrates the demo by creating sample orders and publishing notifications.
/// </summary>
public class Runner(IMediator mediator, ILogger<Runner> logger, IServiceProvider serviceProvider, MediatorStatistics mediatorStatistics)
{
    /// <summary>
    /// Runs the notification demonstration by creating and publishing sample orders.
    /// </summary>
    public async Task RunAsync()
    {
        logger.LogInformation("* Starting Simple Notification Example Demo with Auto-Discovery");
        logger.LogInformation("This demo shows notification middleware auto-discovery and pipeline inspection:");
        logger.LogInformation("  - Automatic discovery of INotificationMiddleware implementations");
        logger.LogInformation("  - Pipeline inspection using INotificationMiddlewarePipelineInspector");
        logger.LogInformation("  - Multiple notification handlers reacting to the same notification");
        logger.LogInformation("  - NEW: MediatorStatistics for analyzing queries, commands, and notifications");
        logger.LogInformation("");

        // First, analyze the mediator types
        InspectMediatorTypes();

        // Then analyze the notification middleware pipeline
        InspectNotificationMiddlewarePipeline();

        logger.LogInformation("");
        logger.LogInformation("=== Starting Order Processing ===");
        logger.LogInformation("");

        await CreateSampleOrders();

        // Show final statistics
        logger.LogInformation("");
        logger.LogInformation("=== FINAL STATISTICS ===");
        mediatorStatistics.ReportStatistics();
        logger.LogInformation("========================");

        logger.LogInformation("");
        logger.LogInformation("* Demo completed! Both subscribers processed all notifications.");
        logger.LogInformation("Check the logs above to see how each subscriber handled the OrderCreatedNotification.");
        logger.LogInformation("Notice how the middleware executed in order: Validation -> Logging -> Metrics -> Audit");
        logger.LogInformation("");
        logger.LogInformation("Press any key to exit the application.");
    }

    /// <summary>
    /// Demonstrates the new mediator statistics functionality for analyzing queries, commands, and notifications.
    /// </summary>
    private void InspectMediatorTypes()
    {
        logger.LogInformation("=== MEDIATOR TYPE ANALYSIS ===");
        logger.LogInformation("");

        // Analyze all queries in the application
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider);
        logger.LogInformation("* QUERIES DISCOVERED:");
        if (queries.Any())
        {
            var queryGroups = queries.GroupBy(q => q.Assembly);
            foreach (var assemblyGroup in queryGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(q => q.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * {Namespace}", namespaceGroup.Key);
                    foreach (var query in namespaceGroup)
                    {
                        var statusIcon = query.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        var responseType = query.ResponseType?.Name ?? "void";
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} -> {ResponseType} ({HandlerDetails})",
                            statusIcon, query.ClassName, query.TypeParameters, responseType, query.HandlerDetails);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No queries discovered)");
        }

        // Analyze all commands in the application
        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider);
        logger.LogInformation("");
        logger.LogInformation("* COMMANDS DISCOVERED:");
        if (commands.Any())
        {
            var commandGroups = commands.GroupBy(c => c.Assembly);
            foreach (var assemblyGroup in commandGroups)
            {
                logger.LogInformation("  * Assembly: {Assembly}", assemblyGroup.Key);
                var namespaceGroups = assemblyGroup.GroupBy(c => c.Namespace);
                foreach (var namespaceGroup in namespaceGroups)
                {
                    logger.LogInformation("    * {Namespace}", namespaceGroup.Key);
                    foreach (var command in namespaceGroup)
                    {
                        var statusIcon = command.HandlerStatus switch
                        {
                            HandlerStatus.Single => "+",
                            HandlerStatus.Missing => "!",
                            HandlerStatus.Multiple => "#",
                            _ => "?"
                        };
                        var responseType = command.ResponseType?.Name ?? "void";
                        logger.LogInformation("      {StatusIcon} {ClassName}{TypeParameters} -> {ResponseType} ({HandlerDetails})",
                            statusIcon, command.ClassName, command.TypeParameters, responseType, command.HandlerDetails);
                    }
                }
            }
        }
        else
        {
            logger.LogInformation("  (No commands discovered)");
        }

        logger.LogInformation("");
        logger.LogInformation("Legend: + = Handler found, ! = No handler, # = Multiple handlers");
        logger.LogInformation("=========================");
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
    }

    /// <summary>
    /// Creates and publishes several sample orders to demonstrate the notification system.
    /// </summary>
    private async Task CreateSampleOrders()
    {
        var orders = new[]
        {
            new
            {
                OrderId = 1001,
                CustomerName = "Alice Johnson",
                CustomerEmail = "alice.johnson@example.com",
                Items = new List<OrderItem>
                {
                    new(101, "Wireless Mouse", 2, 29.99m),
                    new(102, "USB-C Cable", 1, 15.99m)
                }
            },
            new
            {
                OrderId = 1002,
                CustomerName = "Bob Smith",
                CustomerEmail = "bob.smith@example.com",
                Items = new List<OrderItem>
                {
                    new(103, "Mechanical Keyboard", 1, 149.99m),
                    new(104, "Monitor Stand", 1, 79.99m),
                    new(105, "Desk Lamp", 1, 45.99m)
                }
            },
            new
            {
                OrderId = 1003,
                CustomerName = "Carol Davis",
                CustomerEmail = "carol.davis@example.com",
                Items = new List<OrderItem>
                {
                    new(106, "Ergonomic Chair", 1, 299.99m)
                }
            }
        };

        for (int i = 0; i < orders.Length; i++)
        {
            var order = orders[i];

            logger.LogInformation("📋 Creating Order #{OrderId} for {CustomerName}",
                order.OrderId, order.CustomerName);

            var totalAmount = order.Items.Sum(item => item.Quantity * item.UnitPrice);

            var notification = new OrderCreatedNotification(
                order.OrderId,
                order.CustomerEmail,
                order.CustomerName,
                totalAmount,
                order.Items,
                DateTime.UtcNow
            );

            // Publish the notification - both subscribers will receive it
            await mediator.Publish(notification);

            // Add delay between orders to make the demo easier to follow
            if (i < orders.Length - 1)
            {
                logger.LogInformation("");
                logger.LogInformation("⏳ Waiting before next order...");
                await Task.Delay(2000);
                logger.LogInformation("");
            }
        }
    }
}
