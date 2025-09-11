using Blazing.Mediator.Abstractions;
using SimpleNotificationExample.Notifications;
using SimpleNotificationExample.Services;

namespace SimpleNotificationExample;

/// <summary>
/// Runner service that demonstrates the notification system with auto-discovered middleware.
/// </summary>
public class Runner
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public Runner(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Runs the notification demo by inspecting the pipeline and publishing notifications.
    /// </summary>
    public async Task RunAsync()
    {
        // First, let's inspect the notification middleware pipeline to show auto-discovery
        InspectNotificationPipeline();
        
        Console.WriteLine();
        Console.WriteLine("# Publishing Order Created Notifications");
        Console.WriteLine();

        // Publish three different order notifications
        await PublishOrderCreatedNotifications();
        
        Console.WriteLine();
        Console.WriteLine("# Demo completed!");
    }

    /// <summary>
    /// Inspects and displays the auto-discovered notification middleware pipeline.
    /// </summary>
    private void InspectNotificationPipeline()
    {
        Console.WriteLine("* Inspecting Auto-Discovered Notification Middleware Pipeline");
        Console.WriteLine();

        // Get the notification pipeline inspector from the mediator
        var inspector = GetNotificationPipelineInspector(_mediator);
        if (inspector == null)
        {
            Console.WriteLine("  Warning: Could not access notification pipeline inspector");
            return;
        }

        // Use the built-in analysis method from the core library
        var middlewareAnalysis = inspector.AnalyzeMiddleware(_serviceProvider);

        Console.WriteLine("  Auto-discovered notification middleware (in execution order):");
        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"    - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
        }
        
        var count = middlewareAnalysis.Count();
        Console.WriteLine();
        Console.WriteLine($"  Total: {count} notification middleware components auto-discovered");
    }

    /// <summary>
    /// Publishes several order created notifications to demonstrate the pipeline.
    /// </summary>
    private async Task PublishOrderCreatedNotifications()
    {
        // Order 1: Simple electronics order
        var order1 = new OrderCreatedNotification(
            orderId: 1,
            customerEmail: "alice@example.com",
            customerName: "Alice Johnson",
            totalAmount: 1299.99m,
            items: [new OrderItem(101, "Gaming Laptop", 1, 1299.99m)],
            createdAt: DateTime.Now
        );

        Console.WriteLine($"* Publishing: {order1.Items.First().ProductName} order for {order1.CustomerEmail}");
        await _mediator.Publish(order1);
        Console.WriteLine();

        // Order 2: Bulk office supplies
        var order2 = new OrderCreatedNotification(
            orderId: 2, 
            customerEmail: "bob@company.com",
            customerName: "Bob Smith",
            totalAmount: 249.50m,
            items: [new OrderItem(202, "Office Supplies Bundle", 50, 4.99m)],
            createdAt: DateTime.Now
        );

        Console.WriteLine($"* Publishing: {order2.Items.First().ProductName} order for {order2.CustomerEmail}");
        await _mediator.Publish(order2);
        Console.WriteLine();

        // Order 3: Premium service
        var order3 = new OrderCreatedNotification(
            orderId: 3,
            customerEmail: "charlie@startup.io", 
            customerName: "Charlie Wilson",
            totalAmount: 599.00m,
            items: [new OrderItem(303, "Premium Support Plan", 1, 599.00m)],
            createdAt: DateTime.Now
        );

        Console.WriteLine($"* Publishing: {order3.Items.First().ProductName} order for {order3.CustomerEmail}");
        await _mediator.Publish(order3);
        Console.WriteLine();
    }

    /// <summary>
    /// Gets the notification pipeline inspector from the mediator using reflection.
    /// This is needed because the inspector is not directly exposed through the public API.
    /// </summary>
    private static INotificationMiddlewarePipelineInspector? GetNotificationPipelineInspector(IMediator mediator)
    {
        // Use reflection to access the private _notificationPipelineBuilder field
        var notificationPipelineBuilderField = mediator.GetType()
            .GetField("_notificationPipelineBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (notificationPipelineBuilderField?.GetValue(mediator) is INotificationMiddlewarePipelineInspector inspector)
        {
            return inspector;
        }

        return null;
    }
}
