namespace TypedNotificationHandlerExample.Handlers;

/// <summary>
/// Email notification handler that processes order and customer notifications.
/// This handler demonstrates automatic discovery and selective handling of multiple notification types
/// using the INotificationHandler pattern with type-constrained middleware.
/// </summary>
public class EmailNotificationHandler : 
    INotificationHandler<OrderCreatedNotification>,
    INotificationHandler<OrderStatusChangedNotification>, 
    INotificationHandler<CustomerRegisteredNotification>
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? EMAIL: Sending order confirmation email to {CustomerEmail} for Order #{OrderId}", 
            notification.CustomerEmail, notification.OrderId);
        
        Console.WriteLine($"   ?? Sending order confirmation to: {notification.CustomerEmail}");
        Console.WriteLine($"      - Order ID: {notification.OrderId}");
        Console.WriteLine($"      - Total: ${notification.TotalAmount:F2}");
        Console.WriteLine($"      - Items: {notification.Items.Count}");
        
        // Simulate email sending
        await Task.Delay(50, cancellationToken);
        
        _logger.LogDebug("Email notification sent successfully for order {OrderId}", notification.OrderId);
    }

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? EMAIL: Sending order status update email to {CustomerEmail} for Order #{OrderId}", 
            notification.CustomerEmail, notification.OrderId);
            
        Console.WriteLine($"   ?? Sending status update to: {notification.CustomerEmail}");
        Console.WriteLine($"      - Order ID: {notification.OrderId}");
        Console.WriteLine($"      - Status: {notification.OldStatus} ? {notification.NewStatus}");
        
        // Simulate email sending
        await Task.Delay(30, cancellationToken);
        
        _logger.LogDebug("Status update email sent for order {OrderId}", notification.OrderId);
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? EMAIL: Sending welcome email to new customer {CustomerEmail}", 
            notification.CustomerEmail);
            
        Console.WriteLine($"   ?? Sending welcome email to: {notification.CustomerEmail}");
        Console.WriteLine($"      - Customer: {notification.CustomerName}");
        Console.WriteLine($"      - Registered: {notification.RegisteredAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        // Simulate email sending
        await Task.Delay(40, cancellationToken);
        
        _logger.LogDebug("Welcome email sent to customer {CustomerEmail}", notification.CustomerEmail);
    }
}

/// <summary>
/// Inventory notification handler that manages stock levels and alerts.
/// This handler demonstrates automatic discovery and processing of inventory-specific notifications
/// with type-constrained middleware filtering.
/// </summary>
public class InventoryNotificationHandler : INotificationHandler<InventoryUpdatedNotification>
{
    private readonly ILogger<InventoryNotificationHandler> _logger;

    public InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? INVENTORY: Processing inventory update for {ProductId}", notification.ProductId);
        
        Console.WriteLine($"   ?? Inventory Updated: {notification.ProductName}");
        Console.WriteLine($"      - Product ID: {notification.ProductId}");
        Console.WriteLine($"      - Quantity Change: {notification.OldQuantity} ? {notification.NewQuantity} ({notification.ChangeAmount:+#;-#;0})");
        
        // Check for low stock alerts
        if (notification.NewQuantity <= 10 && notification.NewQuantity > 0)
        {
            Console.WriteLine($"      ??  LOW STOCK ALERT: Only {notification.NewQuantity} units remaining!");
            _logger.LogWarning("Low stock alert for {ProductId}: {NewQuantity} units remaining", 
                notification.ProductId, notification.NewQuantity);
        }
        else if (notification.NewQuantity == 0)
        {
            Console.WriteLine($"      ? OUT OF STOCK: {notification.ProductName} is now out of stock!");
            _logger.LogWarning("OUT OF STOCK: {ProductId} ({ProductName})", 
                notification.ProductId, notification.ProductName);
        }
        
        // Simulate inventory system update
        await Task.Delay(25, cancellationToken);
        
        _logger.LogDebug("Inventory processing completed for {ProductId}", notification.ProductId);
    }
}

/// <summary>
/// Business operations handler that manages order lifecycle and business rules.
/// This handler demonstrates automatic discovery and comprehensive order processing
/// with type-constrained middleware for business logic.
/// </summary>
public class BusinessOperationsHandler : 
    INotificationHandler<OrderCreatedNotification>,
    INotificationHandler<OrderStatusChangedNotification>
{
    private readonly ILogger<BusinessOperationsHandler> _logger;

    public BusinessOperationsHandler(ILogger<BusinessOperationsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? BUSINESS: Processing new order business logic for Order #{OrderId}", notification.OrderId);
        
        Console.WriteLine($"   ?? Business Logic: Order Created");
        Console.WriteLine($"      - Validating order rules for Order #{notification.OrderId}");
        Console.WriteLine($"      - Customer: {notification.CustomerName}");
        Console.WriteLine($"      - Order Total: ${notification.TotalAmount:F2}");
        
        // Simulate business rule validation
        if (notification.TotalAmount > 500)
        {
            Console.WriteLine($"      ?? VIP Order detected - applying premium processing");
            _logger.LogInformation("VIP order processing activated for Order #{OrderId} (${TotalAmount})", 
                notification.OrderId, notification.TotalAmount);
        }
        
        // Simulate business processing
        await Task.Delay(35, cancellationToken);
        
        _logger.LogDebug("Business operations completed for order {OrderId}", notification.OrderId);
    }

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? BUSINESS: Processing status change business logic for Order #{OrderId}", notification.OrderId);
        
        Console.WriteLine($"   ?? Business Logic: Status Changed");
        Console.WriteLine($"      - Order #{notification.OrderId}: {notification.OldStatus} ? {notification.NewStatus}");
        
        // Handle specific status transitions
        switch (notification.NewStatus.ToLowerInvariant())
        {
            case "shipped":
                Console.WriteLine($"      ?? Initiating shipping workflow");
                _logger.LogInformation("Shipping workflow initiated for Order #{OrderId}", notification.OrderId);
                break;
            case "delivered":
                Console.WriteLine($"      ? Delivery confirmed - updating customer records");
                _logger.LogInformation("Delivery confirmed for Order #{OrderId}", notification.OrderId);
                break;
            case "cancelled":
                Console.WriteLine($"      ? Order cancelled - processing refund workflow");
                _logger.LogInformation("Cancellation workflow initiated for Order #{OrderId}", notification.OrderId);
                break;
        }
        
        // Simulate business processing
        await Task.Delay(30, cancellationToken);
        
        _logger.LogDebug("Status change business operations completed for order {OrderId}", notification.OrderId);
    }
}

/// <summary>
/// Audit notification handler that maintains comprehensive audit trails.
/// This handler demonstrates automatic discovery and logging of all notification activities
/// for compliance and monitoring purposes.
/// </summary>
public class AuditNotificationHandler : 
    INotificationHandler<OrderCreatedNotification>,
    INotificationHandler<OrderStatusChangedNotification>,
    INotificationHandler<CustomerRegisteredNotification>,
    INotificationHandler<InventoryUpdatedNotification>
{
    private readonly ILogger<AuditNotificationHandler> _logger;

    public AuditNotificationHandler(ILogger<AuditNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? AUDIT: Recording order creation event for Order #{OrderId}", notification.OrderId);
        
        Console.WriteLine($"   ?? Audit Log: ORDER_CREATED");
        Console.WriteLine($"      - Order ID: {notification.OrderId}");
        Console.WriteLine($"      - Customer: {notification.CustomerEmail}");
        Console.WriteLine($"      - Timestamp: {notification.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        // Simulate audit logging
        await Task.Delay(15, cancellationToken);
        
        _logger.LogDebug("Order creation audit completed for {OrderId}", notification.OrderId);
    }

    public async Task Handle(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? AUDIT: Recording order status change for Order #{OrderId}", notification.OrderId);
        
        Console.WriteLine($"   ?? Audit Log: ORDER_STATUS_CHANGED");
        Console.WriteLine($"      - Order ID: {notification.OrderId}");
        Console.WriteLine($"      - Status Change: {notification.OldStatus} ? {notification.NewStatus}");
        Console.WriteLine($"      - Timestamp: {notification.ChangedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        // Simulate audit logging
        await Task.Delay(15, cancellationToken);
        
        _logger.LogDebug("Status change audit completed for {OrderId}", notification.OrderId);
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? AUDIT: Recording customer registration for {CustomerEmail}", notification.CustomerEmail);
        
        Console.WriteLine($"   ?? Audit Log: CUSTOMER_REGISTERED");
        Console.WriteLine($"      - Email: {notification.CustomerEmail}");
        Console.WriteLine($"      - Name: {notification.CustomerName}");
        Console.WriteLine($"      - Timestamp: {notification.RegisteredAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        // Simulate audit logging
        await Task.Delay(15, cancellationToken);
        
        _logger.LogDebug("Customer registration audit completed for {CustomerEmail}", notification.CustomerEmail);
    }

    public async Task Handle(InventoryUpdatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("?? AUDIT: Recording inventory update for {ProductId}", notification.ProductId);
        
        Console.WriteLine($"   ?? Audit Log: INVENTORY_UPDATED");
        Console.WriteLine($"      - Product ID: {notification.ProductId}");
        Console.WriteLine($"      - Quantity Change: {notification.OldQuantity} ? {notification.NewQuantity}");
        Console.WriteLine($"      - Timestamp: {notification.UpdatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        // Simulate audit logging
        await Task.Delay(15, cancellationToken);
        
        _logger.LogDebug("Inventory update audit completed for {ProductId}", notification.ProductId);
    }
}