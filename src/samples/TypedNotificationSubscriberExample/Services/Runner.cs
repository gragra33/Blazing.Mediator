namespace TypedNotificationSubscriberExample.Services;

/// <summary>
/// Service responsible for running the TypedSimpleNotificationExample demonstration.
/// Shows type-constrained notification middleware and selective notification processing.
/// </summary>
public class Runner
{
    private readonly IMediator _mediator;
    private readonly ILogger<Runner> _logger;
    private readonly INotificationMiddlewarePipelineInspector _notificationPipelineInspector;
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorStatistics _mediatorStatistics;

    // Notification subscribers
    private readonly EmailNotificationHandler _emailHandler;
    private readonly InventoryNotificationHandler _inventoryHandler;
    private readonly BusinessOperationsHandler _businessHandler;
    private readonly AuditNotificationHandler _auditHandler;

    public Runner(
        IMediator mediator,
        ILogger<Runner> logger,
        INotificationMiddlewarePipelineInspector notificationPipelineInspector,
        IServiceProvider serviceProvider,
        MediatorStatistics mediatorStatistics,
        EmailNotificationHandler emailHandler,
        InventoryNotificationHandler inventoryHandler,
        BusinessOperationsHandler businessHandler,
        AuditNotificationHandler auditHandler)
    {
        _mediator = mediator;
        _logger = logger;
        _notificationPipelineInspector = notificationPipelineInspector;
        _serviceProvider = serviceProvider;
        _mediatorStatistics = mediatorStatistics;
        _emailHandler = emailHandler;
        _inventoryHandler = inventoryHandler;
        _businessHandler = businessHandler;
        _auditHandler = auditHandler;
    }

    /// <summary>
    /// Inspects and displays the notification middleware pipeline configuration.
    /// </summary>
    public void InspectNotificationPipeline()
    {
        var middlewareAnalysis = NotificationPipelineAnalyzer.AnalyzeMiddleware(_notificationPipelineInspector, _serviceProvider);

        Console.WriteLine("Registered notification middleware:");
        foreach (var middleware in middlewareAnalysis)
        {
            Console.WriteLine($"  - [{middleware.OrderDisplay}] {middleware.ClassName}{middleware.TypeParameters}");
            if (!string.IsNullOrEmpty(middleware.GenericConstraints))
            {
                Console.WriteLine($"        - Constraints: {middleware.GenericConstraints}");
            }
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Runs the complete demonstration showing type-constrained notification processing.
    /// </summary>
    public async Task RunAsync()
    {
        _logger.LogInformation("Starting TypedSimpleNotificationExample with type-constrained notification middleware...");
        Console.WriteLine();

        // Subscribe all handlers to demonstrate selective processing
        SubscribeNotificationHandlers();

        // Inspect notification pipeline configuration
        InspectNotificationPipeline();

        // Demonstrate each type of notification
        await DemonstrateOrderNotifications();
        await DemonstrateCustomerNotifications();
        await DemonstrateInventoryNotifications();
        await DemonstrateComplexWorkflow();

        // Show notification metrics
        Console.WriteLine("=== NOTIFICATION METRICS ===");
        var metrics = NotificationMetricsMiddleware.GetMetrics();
        foreach (var (notificationType, metric) in metrics)
        {
            Console.WriteLine($"{notificationType}:");
            Console.WriteLine($"  Total: {metric.TotalCount}, Success: {metric.SuccessCount}, " +
                            $"Failures: {metric.FailureCount}, Success Rate: {metric.SuccessRate:F1}%, " +
                            $"Avg Duration: {metric.AverageDuration.TotalMilliseconds:F1}ms");
        }
        Console.WriteLine("=============================");

        // Display detailed mediator statistics analysis using Example.Common
        var analysisService = _serviceProvider.GetRequiredService<ExampleAnalysisService>();
        analysisService.DisplayPostExecutionAnalysis();

        Console.WriteLine();

        _logger.LogInformation("TypedSimpleNotificationExample Demo completed!");
    }

    /// <summary>
    /// Subscribes all notification handlers to their respective notifications.
    /// </summary>
    private void SubscribeNotificationHandlers()
    {
        // Subscribe email handler to order and customer notifications
        _mediator.Subscribe<OrderCreatedNotification>(_emailHandler);
        _mediator.Subscribe<OrderStatusChangedNotification>(_emailHandler);
        _mediator.Subscribe<CustomerRegisteredNotification>(_emailHandler);

        // Subscribe inventory handler to inventory notifications
        _mediator.Subscribe<InventoryUpdatedNotification>(_inventoryHandler);

        // Subscribe business operations handler to order and customer notifications
        _mediator.Subscribe<OrderCreatedNotification>(_businessHandler);
        _mediator.Subscribe<CustomerRegisteredNotification>(_businessHandler);

        // Subscribe audit handler to all notifications
        _mediator.Subscribe<OrderCreatedNotification>(_auditHandler);
        _mediator.Subscribe<OrderStatusChangedNotification>(_auditHandler);
        _mediator.Subscribe<CustomerRegisteredNotification>(_auditHandler);
        _mediator.Subscribe<InventoryUpdatedNotification>(_auditHandler);

        Console.WriteLine("* All notification handlers subscribed");
        Console.WriteLine("   * EmailNotificationHandler ? Order & Customer notifications");
        Console.WriteLine("   * InventoryNotificationHandler ? Inventory notifications");
        Console.WriteLine("   * BusinessOperationsHandler ? Order & Customer notifications");
        Console.WriteLine("   * AuditNotificationHandler ? All notifications");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates order-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateOrderNotifications()
    {
        Console.WriteLine("-------- ORDER NOTIFICATIONS (IOrderNotification Constraint) --------");

        // Publish OrderCreatedNotification
        _logger.LogDebug(">> Publishing OrderCreatedNotification");
        var orderCreated = new OrderCreatedNotification(
            orderId: 12345,
            customerEmail: "john.doe@example.com",
            customerName: "John Doe",
            totalAmount: 299.97m,
            items: new List<OrderItem>
            {
                new(1, "Premium Widget", 2, 99.99m),
                new(2, "Standard Gadget", 1, 99.99m)
            },
            createdAt: DateTime.UtcNow);

        await _mediator.Publish(orderCreated);
        Console.WriteLine();

        // Publish OrderStatusChangedNotification
        _logger.LogDebug(">> Publishing OrderStatusChangedNotification");
        var statusChanged = new OrderStatusChangedNotification(
            orderId: 12345,
            customerEmail: "john.doe@example.com",
            oldStatus: "Pending",
            newStatus: "Processing",
            changedAt: DateTime.UtcNow);

        await _mediator.Publish(statusChanged);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates customer-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateCustomerNotifications()
    {
        Console.WriteLine("-------- CUSTOMER NOTIFICATIONS (ICustomerNotification Constraint) --------");

        // Publish CustomerRegisteredNotification
        _logger.LogDebug(">> Publishing CustomerRegisteredNotification");
        var customerRegistered = new CustomerRegisteredNotification(
            customerEmail: "jane.smith@example.com",
            customerName: "Jane Smith",
            registeredAt: DateTime.UtcNow);

        await _mediator.Publish(customerRegistered);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates inventory-related notifications with type-constrained middleware.
    /// </summary>
    private async Task DemonstrateInventoryNotifications()
    {
        Console.WriteLine("-------- INVENTORY NOTIFICATIONS (IInventoryNotification Constraint) --------");

        // Publish InventoryUpdatedNotification
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

        // Publish low stock scenario
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

        // Publish out of stock scenario
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
    /// Demonstrates a complex workflow with multiple notification types.
    /// </summary>
    private async Task DemonstrateComplexWorkflow()
    {
        Console.WriteLine("-------- COMPLEX WORKFLOW (Multiple Notification Types) --------");

        _logger.LogInformation(">> Simulating complex e-commerce workflow with multiple notifications");
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
        _logger.LogInformation("<< Complex workflow completed - demonstrated type-constrained notification processing");
        Console.WriteLine();
    }
}