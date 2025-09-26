namespace TypedNotificationHandlerExample.Services;

/// <summary>
/// Service responsible for running the TypedNotificationHandlerExample demonstration.
/// Shows type-constrained notification middleware with automatic handler discovery and selective notification processing.
/// </summary>
public class Runner
{
    private readonly IMediator _mediator;
    private readonly ILogger<Runner> _logger;
    private readonly INotificationMiddlewarePipelineInspector _notificationPipelineInspector;
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorStatistics _mediatorStatistics;

    public Runner(
        IMediator mediator,
        ILogger<Runner> logger,
        INotificationMiddlewarePipelineInspector notificationPipelineInspector,
        IServiceProvider serviceProvider,
        MediatorStatistics mediatorStatistics)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationPipelineInspector = notificationPipelineInspector;
        _serviceProvider = serviceProvider;
        _mediatorStatistics = mediatorStatistics;
    }

    /// <summary>
    /// Runs the complete demonstration of typed notification handlers with automatic discovery.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogInformation("?? Starting TypedNotificationHandlerExample with automatic handler discovery...");

        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine(" TYPED NOTIFICATION HANDLER EXAMPLE - AUTOMATIC DISCOVERY");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        await DemonstrateHandlerDiscovery();
        await DemonstrateOrderNotifications();
        await DemonstrateCustomerNotifications();
        await DemonstrateInventoryNotifications();
        await DemonstrateComplexWorkflow();
        await DisplayStatistics();

        Console.WriteLine();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine(" DEMONSTRATION COMPLETED");
        Console.WriteLine("=".PadRight(80, '='));
        
        _logger.LogInformation("?? TypedNotificationHandlerExample demonstration completed successfully");
    }

    /// <summary>
    /// Demonstrates automatic handler discovery and registration.
    /// </summary>
    private async Task DemonstrateHandlerDiscovery()
    {
        Console.WriteLine("-------- AUTOMATIC HANDLER DISCOVERY ANALYSIS --------");
        
        _logger.LogInformation("?? Analyzing automatically discovered notification handlers...");
        
        // Get all discovered notification handlers from service provider
        var discoveredHandlers = new List<string>();
        
        // Check for OrderCreatedNotification handlers
        var orderCreatedHandlers = _serviceProvider.GetServices<INotificationHandler<OrderCreatedNotification>>();
        foreach (var handler in orderCreatedHandlers)
        {
            var handlerType = handler.GetType().Name;
            discoveredHandlers.Add($"INotificationHandler<OrderCreatedNotification>: {handlerType}");
            Console.WriteLine($"? Discovered: {handlerType} ? OrderCreatedNotification");
        }
        
        // Check for OrderStatusChangedNotification handlers  
        var orderStatusHandlers = _serviceProvider.GetServices<INotificationHandler<OrderStatusChangedNotification>>();
        foreach (var handler in orderStatusHandlers)
        {
            var handlerType = handler.GetType().Name;
            if (!discoveredHandlers.Any(h => h.Contains(handlerType)))
            {
                discoveredHandlers.Add($"INotificationHandler<OrderStatusChangedNotification>: {handlerType}");
            }
            Console.WriteLine($"? Discovered: {handlerType} ? OrderStatusChangedNotification");
        }

        // Check for CustomerRegisteredNotification handlers
        var customerHandlers = _serviceProvider.GetServices<INotificationHandler<CustomerRegisteredNotification>>();
        foreach (var handler in customerHandlers)
        {
            var handlerType = handler.GetType().Name;
            if (!discoveredHandlers.Any(h => h.Contains(handlerType)))
            {
                discoveredHandlers.Add($"INotificationHandler<CustomerRegisteredNotification>: {handlerType}");
            }
            Console.WriteLine($"? Discovered: {handlerType} ? CustomerRegisteredNotification");
        }

        // Check for InventoryUpdatedNotification handlers
        var inventoryHandlers = _serviceProvider.GetServices<INotificationHandler<InventoryUpdatedNotification>>();
        foreach (var handler in inventoryHandlers)
        {
            var handlerType = handler.GetType().Name;
            if (!discoveredHandlers.Any(h => h.Contains(handlerType)))
            {
                discoveredHandlers.Add($"INotificationHandler<InventoryUpdatedNotification>: {handlerType}");
            }
            Console.WriteLine($"? Discovered: {handlerType} ? InventoryUpdatedNotification");
        }

        Console.WriteLine();
        Console.WriteLine($"?? Total Handlers Discovered: {discoveredHandlers.Count}");
        Console.WriteLine($"?? All handlers are automatically registered - no manual subscription required!");
        Console.WriteLine();

        _logger.LogInformation("?? Discovered {HandlerCount} automatic notification handlers", discoveredHandlers.Count);
        
        await Task.Delay(100); // Brief pause for readability
    }

    /// <summary>
    /// Demonstrates order-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateOrderNotifications()
    {
        Console.WriteLine("-------- ORDER NOTIFICATIONS (Type-Constrained Middleware) --------");

        _logger.LogInformation(">> Publishing OrderCreatedNotification with automatic handler discovery");
        var orderCreated = new OrderCreatedNotification(
            orderId: 12345,
            customerEmail: "customer@example.com",
            customerName: "John Doe", 
            totalAmount: 299.99m,
            items: new List<OrderItem>
            {
                new(1, "Premium Widget", 2, 99.99m),
                new(2, "Standard Gadget", 1, 49.99m),
                new(3, "Deluxe Accessory", 1, 149.99m)
            },
            createdAt: DateTime.UtcNow);

        await _mediator.Publish(orderCreated);
        Console.WriteLine();

        _logger.LogDebug(">> Publishing OrderStatusChangedNotification");
        var statusChanged = new OrderStatusChangedNotification(
            orderId: 12345,
            customerEmail: "customer@example.com",
            oldStatus: "Processing",
            newStatus: "Shipped",
            changedAt: DateTime.UtcNow);

        await _mediator.Publish(statusChanged);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateCustomerNotifications()
    {
        Console.WriteLine("-------- CUSTOMER NOTIFICATIONS (Type-Constrained Middleware) --------");

        _logger.LogInformation(">> Publishing CustomerRegisteredNotification with automatic handler discovery");
        var customerRegistered = new CustomerRegisteredNotification(
            customerEmail: "newcustomer@example.com",
            customerName: "Alice Smith",
            registeredAt: DateTime.UtcNow);

        await _mediator.Publish(customerRegistered);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates inventory-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateInventoryNotifications()
    {
        Console.WriteLine("-------- INVENTORY NOTIFICATIONS (Type-Constrained Middleware) --------");

        // Normal inventory update
        _logger.LogDebug(">> Publishing InventoryUpdatedNotification");
        var inventoryUpdated = new InventoryUpdatedNotification(
            productId: "WIDGET-001",
            productName: "Premium Widget",
            oldQuantity: 25,
            newQuantity: 23,
            changeAmount: -2,
            updatedAt: DateTime.UtcNow);

        await _mediator.Publish(inventoryUpdated);
        Console.WriteLine();

        // Low stock scenario
        _logger.LogDebug(">> Publishing Low Stock InventoryUpdatedNotification");
        var lowStockUpdate = new InventoryUpdatedNotification(
            productId: "GADGET-002",
            productName: "Standard Gadget",
            oldQuantity: 15,
            newQuantity: 8,
            changeAmount: -7,
            updatedAt: DateTime.UtcNow);

        await _mediator.Publish(lowStockUpdate);
        Console.WriteLine();

        // Out of stock scenario
        _logger.LogDebug(">> Publishing Out of Stock InventoryUpdatedNotification");
        var outOfStockUpdate = new InventoryUpdatedNotification(
            productId: "SPECIAL-003",
            productName: "Limited Edition Item",
            oldQuantity: 1,
            newQuantity: 0,
            changeAmount: -1,
            updatedAt: DateTime.UtcNow);

        await _mediator.Publish(outOfStockUpdate);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates a complex workflow with multiple notification types and automatic handler discovery.
    /// </summary>
    private async Task DemonstrateComplexWorkflow()
    {
        Console.WriteLine("-------- COMPLEX WORKFLOW (Multiple Notification Types + Auto Discovery) --------");

        _logger.LogInformation(">> Simulating complex e-commerce workflow with automatic handlers");
        Console.WriteLine();

        // Step 1: Customer registration
        var newCustomer = new CustomerRegisteredNotification(
            customerEmail: "workflow@example.com",
            customerName: "Workflow Customer",
            registeredAt: DateTime.UtcNow);
        await _mediator.Publish(newCustomer);

        await Task.Delay(100); // Small delay for readability

        // Step 2: Order creation  
        var workflowOrder = new OrderCreatedNotification(
            orderId: 99999,
            customerEmail: "workflow@example.com",
            customerName: "Workflow Customer",
            totalAmount: 149.98m,
            items: new List<OrderItem>
            {
                new(1, "Premium Widget", 1, 99.99m),
                new(4, "Accessory Pack", 1, 49.99m)
            },
            createdAt: DateTime.UtcNow);
        await _mediator.Publish(workflowOrder);

        await Task.Delay(100); // Small delay for readability

        // Step 3: Inventory updates for ordered items
        var widgetInventoryUpdate = new InventoryUpdatedNotification(
            productId: "WIDGET-001",
            productName: "Premium Widget",
            oldQuantity: 23,
            newQuantity: 22,
            changeAmount: -1,
            updatedAt: DateTime.UtcNow);
        await _mediator.Publish(widgetInventoryUpdate);

        var accessoryInventoryUpdate = new InventoryUpdatedNotification(
            productId: "ACCESSORY-004",
            productName: "Accessory Pack",
            oldQuantity: 50,
            newQuantity: 49,
            changeAmount: -1,
            updatedAt: DateTime.UtcNow);
        await _mediator.Publish(accessoryInventoryUpdate);

        await Task.Delay(100); // Small delay for readability

        // Step 4: Order status update
        var workflowStatusUpdate = new OrderStatusChangedNotification(
            orderId: 99999,
            customerEmail: "workflow@example.com",
            oldStatus: "Processing",
            newStatus: "Shipped",
            changedAt: DateTime.UtcNow);
        await _mediator.Publish(workflowStatusUpdate);

        Console.WriteLine();
        _logger.LogInformation("<< Complex workflow completed - demonstrated automatic handler discovery with type-constrained processing");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays performance statistics and handler metrics.
    /// </summary>
    private async Task DisplayStatistics()
    {
        Console.WriteLine("-------- PERFORMANCE STATISTICS --------");

        // Display notification metrics from middleware
        var metrics = NotificationMetricsMiddleware.GetMetrics();
        if (metrics.Any())
        {
            Console.WriteLine("?? Notification Performance Metrics:");
            foreach (var (notificationType, stats) in metrics)
            {
                Console.WriteLine($"   {notificationType}:");
                Console.WriteLine($"     - Count: {stats.Count}");
                Console.WriteLine($"     - Avg: {stats.AvgMs:F1}ms");
                Console.WriteLine($"     - Min: {stats.MinMs}ms");
                Console.WriteLine($"     - Max: {stats.MaxMs}ms");
                Console.WriteLine($"     - Total: {stats.TotalMs}ms");
            }
            Console.WriteLine();
        }

        // Display mediator statistics
        var notificationStats = _mediatorStatistics.AnalyzeNotifications(_serviceProvider);
        Console.WriteLine($"?? Mediator Statistics:");
        Console.WriteLine($"   - Notification Types: {notificationStats.Count}");
        
        foreach (var analysis in notificationStats)
        {
            Console.WriteLine($"   - {analysis.ClassName}: {analysis.Handlers.Count} handlers");
        }

        Console.WriteLine();
        _logger.LogInformation("?? Statistics display completed");
        
        await Task.CompletedTask;
    }
}