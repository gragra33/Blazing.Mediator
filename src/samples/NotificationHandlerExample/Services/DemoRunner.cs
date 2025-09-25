namespace NotificationHandlerExample.Services;

/// <summary>
/// Demo runner service that demonstrates the automatic notification handler discovery and execution.
/// Shows how multiple handlers are automatically invoked for a single notification.
/// </summary>
public class DemoRunner(IMediator mediator, ILogger<DemoRunner> logger)
{
    private static readonly List<(string ProductName, decimal UnitPrice)> SampleProducts = new()
    {
        ("Laptop Pro 15\"", 1299.99m),
        ("Wireless Mouse", 49.99m),
        ("USB-C Cable", 19.99m),
        ("Bluetooth Headphones", 199.99m),
        ("Mechanical Keyboard", 129.99m),
        ("Monitor 27\" 4K", 599.99m),
        ("Webcam HD", 89.99m),
        ("External SSD 1TB", 149.99m),
        ("Phone Case", 24.99m),
        ("Desk Lamp LED", 39.99m)
    };

    private static readonly List<(string Name, string Email)> SampleCustomers = new()
    {
        ("John Doe", "john.doe@email.com"),
        ("Jane Smith", "jane.smith@email.com"),
        ("Mike Johnson", "mike.johnson@email.com"),
        ("Sarah Wilson", "sarah.wilson@email.com"),
        ("David Brown", "david.brown@email.com"),
        ("Lisa Davis", "lisa.davis@email.com"),
        ("Tom Miller", "tom.miller@email.com"),
        ("Emily Anderson", "emily.anderson@email.com")
    };

    public async Task RunAsync()
    {
        logger.LogInformation(">> Starting NotificationHandler Example Demo");
        logger.LogInformation("This demo shows automatic discovery and execution of INotificationHandler implementations");
        logger.LogInformation("");
        logger.LogInformation("Registered handlers that will be automatically invoked:");
        logger.LogInformation("  * EmailNotificationHandler - Sends order confirmation emails");
        logger.LogInformation("  * InventoryNotificationHandler - Updates inventory and checks stock levels");
        logger.LogInformation("  * AuditNotificationHandler - Logs orders for compliance and auditing");
        logger.LogInformation("  * ShippingNotificationHandler - Processes shipping and fulfillment");
        logger.LogInformation("");
        logger.LogInformation("Middleware pipeline (automatic discovery):");
        logger.LogInformation("  1. NotificationLoggingMiddleware (Order: 100) - Logs pipeline execution");
        logger.LogInformation("  2. NotificationValidationMiddleware (Order: 200) - Validates notification data");
        logger.LogInformation("  3. NotificationMetricsMiddleware (Order: 300) - Collects performance metrics");
        logger.LogInformation("");

        // Create and publish sample orders
        await CreateSampleOrders();

        logger.LogInformation("");
        logger.LogInformation("*** Demo completed! All handlers were automatically discovered and executed. ***");
        logger.LogInformation("Key benefits demonstrated:");
        logger.LogInformation("  [+] No manual subscription required - handlers are auto-discovered");
        logger.LogInformation("  [+] Multiple handlers can process the same notification independently");
        logger.LogInformation("  [+] Middleware pipeline processes all notifications consistently");
        logger.LogInformation("  [+] Error handling is isolated per handler");
        logger.LogInformation("  [+] Scalable architecture - easy to add new handlers");
    }

    private async Task CreateSampleOrders()
    {
        // Create 3 sample orders with different characteristics
        var orders = new[]
        {
            CreateSampleOrder("ORD-001", 1, 2),    // Small order
            CreateSampleOrder("ORD-002", 3, 5),    // Medium order  
            CreateSampleOrder("ORD-003", 6, 8)     // Large order
        };

        foreach (var order in orders)
        {
            logger.LogInformation("================================================================");
            logger.LogInformation(">> Publishing Order: {OrderId} (${TotalAmount:F2})", 
                order.OrderId, order.TotalAmount);
            logger.LogInformation("================================================================");
            
            // Publish the notification - all handlers will be automatically invoked
            await mediator.Publish(order);
            
            logger.LogInformation("================================================================");
            logger.LogInformation("");

            // Small delay between orders for readability
            await Task.Delay(1000);
        }

        // Demonstrate validation failure
        await DemonstrateValidationFailure();
    }

    private OrderCreatedNotification CreateSampleOrder(string orderId, int minItems, int maxItems)
    {
        var customer = SampleCustomers[Random.Shared.Next(SampleCustomers.Count)];
        var itemCount = Random.Shared.Next(minItems, maxItems + 1);
        var items = new List<OrderItem>();

        // Generate random items for the order
        var availableProducts = SampleProducts.ToList();
        for (int i = 0; i < itemCount; i++)
        {
            var productIndex = Random.Shared.Next(availableProducts.Count);
            var product = availableProducts[productIndex];
            availableProducts.RemoveAt(productIndex); // Avoid duplicates

            var quantity = Random.Shared.Next(1, 4);
            items.Add(new OrderItem
            {
                ProductName = product.ProductName,
                Quantity = quantity,
                UnitPrice = product.UnitPrice
            });
        }

        return new OrderCreatedNotification
        {
            OrderId = orderId,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            TotalAmount = items.Sum(i => i.TotalPrice),
            Items = items,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task DemonstrateValidationFailure()
    {
        logger.LogInformation("================================================================");
        logger.LogInformation("*** DEMONSTRATING VALIDATION FAILURE ***");
        logger.LogInformation("================================================================");

        // Create an invalid order (negative total amount)
        var invalidOrder = new OrderCreatedNotification
        {
            OrderId = "ORD-INVALID",
            CustomerName = "Test Customer",
            CustomerEmail = "invalid-email", // Invalid email format
            TotalAmount = -100m, // Invalid negative amount
            Items = new List<OrderItem>(), // Empty items list
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await mediator.Publish(invalidOrder);
            logger.LogError("[-] Expected validation to fail, but it didn't!");
        }
        catch (Exception ex)
        {
            logger.LogInformation("[+] Validation correctly failed: {ErrorMessage}", ex.Message);
            logger.LogInformation("    This demonstrates how middleware can prevent invalid notifications");
            logger.LogInformation("    from being processed by any handlers.");
        }

        logger.LogInformation("================================================================");
        logger.LogInformation("");
    }
}