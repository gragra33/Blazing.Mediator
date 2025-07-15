# Blazing.Mediator - Notification System Guide

## Overview

The Notification System in `Blazing.Mediator` implements the **Observer Pattern** to enable loosely coupled, event-driven communication between components. This powerful system allows multiple services to react to domain events without tight coupling, promoting scalability and maintainability.

The notification system is built on top of the mediator pattern and provides a clean way to publish domain events that can be handled by multiple subscribers. This is particularly useful for implementing cross-cutting concerns like logging, caching, email notifications, and business rule enforcement.

### Key Features

-   **Event-Driven Architecture**: Publish domain events and have multiple subscribers react to them
-   **Observer Pattern**: Multiple services can subscribe to the same notification without coupling
-   **Asynchronous Processing**: All notifications are processed asynchronously for better performance
-   **Middleware Support**: Add cross-cutting concerns like logging and metrics to notification processing
-   **Background Services**: Integrate with hosted services for long-running notification processing
-   **Built-in Metrics**: Track notification performance and success rates
-   **Type Safety**: Strongly typed notifications with compile-time checking
-   **Testable Design**: Easy to test notification publishers and subscribers

## Table of Contents

1. [Quick Start](#quick-start)
2. [Core Concepts](#core-concepts)
3. [Creating Notifications](#creating-notifications)
4. [Implementing Subscribers](#implementing-subscribers)
5. [Setup and Registration](#setup-and-registration)
6. [Notification Middleware](#notification-middleware)
7. [Background Services](#background-services)
8. [Testing Notifications](#testing-notifications)
9. [ECommerce.Api Implementation](#ecommerceapi-implementation)
10. [Swagger Testing Guide](#swagger-testing-guide)
11. [Automated Testing with PowerShell Script](#automated-testing-with-powershell-script)
12. [Best Practices](#best-practices)
13. [Troubleshooting](#troubleshooting)

## Quick Start

Get started with notifications in under 5 minutes:

### 1. Install the Package

```bash
dotnet add package Blazing.Mediator
```

### 2. Create Your First Notification

```csharp
// Define a notification
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedNotification(int orderId, string customerEmail, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
    }
}
```

### 3. Create a Notification Subscriber

```csharp
// Handle the notification
public class EmailNotificationHandler : INotificationSubscriber<OrderCreatedNotification>
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Send email confirmation
        _logger.LogInformation("📧 Sending order confirmation email to {Email} for order {OrderId}",
            notification.CustomerEmail, notification.OrderId);

        // Your email sending logic here
        await Task.CompletedTask;
    }
}
```

### 4. Register Services

```csharp
// Program.cs
builder.Services.AddMediator(typeof(Program).Assembly);
```

### 5. Publish Notifications

```csharp
// In your service or handler
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IMediator _mediator;

    public CreateOrderHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Create order logic...
        var orderId = 123;

        // Publish notification
        await _mediator.Publish(new OrderCreatedNotification(orderId, request.CustomerEmail, request.TotalAmount), cancellationToken);

        return orderId;
    }
}
```

## Core Concepts

### Notifications

**Notifications** are messages that represent something interesting that happened in your domain. They implement the `INotification` interface:

```csharp
public interface INotification
{
    // Marker interface - no methods required
}
```

### Notification Subscribers

**Subscribers** handle notifications by implementing `INotificationSubscriber<TNotification>`:

```csharp
public interface INotificationSubscriber<in TNotification> : INotificationSubscriber
    where TNotification : INotification
{
    Task OnNotification(TNotification notification, CancellationToken cancellationToken = default);
}
```

### How Notifications Work

The notification system follows the Observer pattern:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                          Notification System Flow                              │
└─────────────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────┐
                    │   Publisher     │
                    │  (Command/Query │
                    │   Handler)      │
                    └─────────┬───────┘
                              │
                              │ 1. Publish Notification
                              ▼
                    ┌─────────────────┐
                    │   IMediator     │
                    │  (Dispatcher)   │
                    └─────────┬───────┘
                              │
                              │ 2. Route to Subscribers
                              ▼
         ┌────────────────────┴────────────────────┐
         │                                         │
         ▼                                         ▼
┌─────────────────┐                       ┌─────────────────┐
│  Subscriber 1   │                       │  Subscriber 2   │
│  (Email         │                       │  (Inventory     │
│   Service)      │                       │   Service)      │
│                 │                       │                 │
│ OnNotification  │                       │ OnNotification  │
│ (Async)         │                       │ (Async)         │
└─────────┬───────┘                       └─────────┬───────┘
          │                                         │
          │ 3. Process Notification                 │ 3. Process Notification
          ▼                                         ▼
┌─────────────────┐                       ┌─────────────────┐
│  Send Email     │                       │  Update Stock   │
│  Confirmation   │                       │  Levels         │
└─────────────────┘                       └─────────────────┘
```

## Creating Notifications

### Basic Notification

```csharp
public class UserRegisteredNotification : INotification
{
    public int UserId { get; }
    public string Email { get; }
    public DateTime RegisteredAt { get; }

    public UserRegisteredNotification(int userId, string email, DateTime registeredAt)
    {
        UserId = userId;
        Email = email;
        RegisteredAt = registeredAt;
    }
}
```

### Rich Notification with Complex Data

```csharp
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }
    public List<OrderItem> Items { get; }
    public DateTime CreatedAt { get; }

    public OrderCreatedNotification(int orderId, int customerId, string customerEmail,
        decimal totalAmount, List<OrderItem> items, DateTime createdAt)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Items = items;
        CreatedAt = createdAt;
    }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
```

### Notification Naming Conventions

Follow these naming conventions for clarity:

-   Use **past tense** for events that have already happened: `OrderCreated`, `UserRegistered`, `PaymentProcessed`
-   Use **present tense** for states: `StockLow`, `SystemBusy`
-   Always suffix with `Notification`: `OrderCreatedNotification`, `StockLowNotification`

## Implementing Subscribers

### Simple Subscriber

```csharp
public class OrderEmailHandler : INotificationSubscriber<OrderCreatedNotification>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderEmailHandler> _logger;

    public OrderEmailHandler(IEmailService emailService, ILogger<OrderEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _emailService.SendOrderConfirmationAsync(
                notification.CustomerEmail,
                notification.OrderId,
                notification.TotalAmount,
                cancellationToken);

            _logger.LogInformation("Order confirmation email sent for order {OrderId}", notification.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
            // Don't rethrow - we don't want to fail the entire notification pipeline
        }
    }
}
```

### Multiple Notification Subscriber

```csharp
public class AuditService :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<OrderStatusChangedNotification>,
    INotificationSubscriber<UserRegisteredNotification>
{
    private readonly IAuditRepository _auditRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditRepository auditRepository, ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await _auditRepository.LogEventAsync("OrderCreated", notification.OrderId, notification, cancellationToken);
        _logger.LogInformation("Audit logged for order creation {OrderId}", notification.OrderId);
    }

    public async Task OnNotification(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        await _auditRepository.LogEventAsync("OrderStatusChanged", notification.OrderId, notification, cancellationToken);
        _logger.LogInformation("Audit logged for order status change {OrderId}", notification.OrderId);
    }

    public async Task OnNotification(UserRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await _auditRepository.LogEventAsync("UserRegistered", notification.UserId, notification, cancellationToken);
        _logger.LogInformation("Audit logged for user registration {UserId}", notification.UserId);
    }
}
```

## Setup and Registration

### Basic Registration

```csharp
// Program.cs
builder.Services.AddMediator(typeof(Program).Assembly);
```

### Advanced Registration with Configuration

```csharp
// Program.cs
builder.Services.AddMediator(config =>
{
    // Add notification middleware
    config.AddNotificationMiddleware<NotificationLoggingMiddleware>();
    config.AddNotificationMiddleware<NotificationMetricsMiddleware>();

    // Configure assemblies to scan
    config.AddFromAssembly(typeof(Program).Assembly);
    config.AddFromAssembly(typeof(OrderCreatedNotification).Assembly);

}, typeof(Program).Assembly);
```

### Manual Subscriber Registration

```csharp
// If you need manual control over subscriber registration
builder.Services.AddScoped<INotificationSubscriber<OrderCreatedNotification>, OrderEmailHandler>();
builder.Services.AddScoped<INotificationSubscriber<OrderCreatedNotification>, OrderAuditHandler>();
```

## Notification Middleware

Middleware allows you to add cross-cutting concerns to notification processing:

### Logging Middleware

```csharp
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    private readonly ILogger<NotificationLoggingMiddleware> _logger;

    public NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("🔔 Publishing notification: {NotificationName}", notificationName);

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("✅ Notification completed: {NotificationName} in {Duration}ms",
                notificationName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "❌ Notification failed: {NotificationName} after {Duration}ms",
                notificationName, duration.TotalMilliseconds);
            throw;
        }
    }
}
```

### Metrics Middleware

```csharp
public class NotificationMetricsMiddleware : INotificationMiddleware
{
    private readonly ILogger<NotificationMetricsMiddleware> _logger;
    private static readonly Dictionary<string, NotificationMetrics> _metrics = new();
    private static readonly object _lock = new();

    public NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            await next(notification, cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: true);
        }
        catch (Exception)
        {
            var duration = DateTime.UtcNow - startTime;
            UpdateMetrics(notificationName, duration, success: false);
            throw;
        }
    }

    private void UpdateMetrics(string notificationName, TimeSpan duration, bool success)
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(notificationName))
            {
                _metrics[notificationName] = new NotificationMetrics();
            }

            var metrics = _metrics[notificationName];
            metrics.TotalCount++;
            metrics.TotalDuration += duration;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            // Log metrics periodically
            if (metrics.TotalCount % 10 == 0)
            {
                _logger.LogInformation("📊 Notification metrics for {NotificationName}: " +
                    "Total: {Total}, Success: {Success}, Failures: {Failures}, Avg Duration: {AvgDuration}ms",
                    notificationName, metrics.TotalCount, metrics.SuccessCount, metrics.FailureCount,
                    metrics.AverageDuration.TotalMilliseconds);
            }
        }
    }
}

public class NotificationMetrics
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration => TotalCount > 0 ? TimeSpan.FromTicks(TotalDuration.Ticks / TotalCount) : TimeSpan.Zero;
}
```

## Background Services

Integrate notifications with background services for long-running processing:

### Email Notification Service

```csharp
public class EmailNotificationService : BackgroundService,
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<OrderStatusChangedNotification>
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public EmailNotificationService(ILogger<EmailNotificationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to notifications
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        mediator.Subscribe<OrderCreatedNotification>(this);
        mediator.Subscribe<OrderStatusChangedNotification>(this);

        // Keep the service running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(100, cancellationToken); // Simulate processing

            _logger.LogInformation("📧 ORDER CONFIRMATION EMAIL SENT");
            _logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            _logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            _logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
        }
    }

    public async Task OnNotification(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(100, cancellationToken); // Simulate processing

            _logger.LogInformation("📧 ORDER STATUS UPDATE EMAIL SENT");
            _logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            _logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            _logger.LogInformation("   Status: {Status}", notification.NewStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order status email for order {OrderId}", notification.OrderId);
        }
    }
}
```

### Inventory Management Service

```csharp
public class InventoryManagementService : BackgroundService,
    INotificationSubscriber<ProductStockLowNotification>,
    INotificationSubscriber<ProductOutOfStockNotification>
{
    private readonly ILogger<InventoryManagementService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public InventoryManagementService(ILogger<InventoryManagementService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        mediator.Subscribe<ProductStockLowNotification>(this);
        mediator.Subscribe<ProductOutOfStockNotification>(this);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    public async Task OnNotification(ProductStockLowNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("⚠️  LOW STOCK ALERT");
        _logger.LogWarning("   Product: {ProductName} (ID: {ProductId})", notification.ProductName, notification.ProductId);
        _logger.LogWarning("   Current Stock: {CurrentStock}", notification.CurrentStock);
        _logger.LogWarning("   Minimum Threshold: {MinimumThreshold}", notification.MinimumThreshold);

        // Trigger reorder process
        await TriggerReorderProcess(notification, cancellationToken);
    }

    public async Task OnNotification(ProductOutOfStockNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogError("🚨 OUT OF STOCK ALERT - URGENT");
        _logger.LogError("   Product: {ProductName} (ID: {ProductId})", notification.ProductName, notification.ProductId);
        _logger.LogError("   Last Stock Update: {LastStockUpdate}", notification.LastStockUpdate);

        // Trigger urgent reorder
        await TriggerUrgentReorder(notification, cancellationToken);
    }

    private async Task TriggerReorderProcess(ProductStockLowNotification notification, CancellationToken cancellationToken)
    {
        // Simulate reorder logic
        await Task.Delay(50, cancellationToken);
        _logger.LogInformation("📋 REORDER NOTIFICATION SENT TO PURCHASING");
        _logger.LogInformation("   Product: {ProductName}", notification.ProductName);
        _logger.LogInformation("   Recommended Quantity: {ReorderQuantity}", notification.ReorderQuantity);
    }

    private async Task TriggerUrgentReorder(ProductOutOfStockNotification notification, CancellationToken cancellationToken)
    {
        // Simulate urgent reorder logic
        await Task.Delay(50, cancellationToken);
        _logger.LogError("🚨 URGENT REORDER INITIATED");
        _logger.LogError("   Product: {ProductName}", notification.ProductName);
        _logger.LogError("   Priority: URGENT");
    }
}
```

## Testing Notifications

### Unit Testing Notification Subscribers

```csharp
[Test]
public async Task OnNotification_OrderCreated_SendsEmail()
{
    // Arrange
    var mockEmailService = new Mock<IEmailService>();
    var mockLogger = new Mock<ILogger<OrderEmailHandler>>();
    var handler = new OrderEmailHandler(mockEmailService.Object, mockLogger.Object);

    var notification = new OrderCreatedNotification(
        orderId: 123,
        customerEmail: "test@example.com",
        totalAmount: 99.99m,
        items: new List<OrderItem>(),
        createdAt: DateTime.UtcNow
    );

    // Act
    await handler.OnNotification(notification);

    // Assert
    mockEmailService.Verify(x => x.SendOrderConfirmationAsync(
        "test@example.com",
        123,
        99.99m,
        It.IsAny<CancellationToken>()
    ), Times.Once);
}
```

### Integration Testing with Test Mediator

```csharp
[Test]
public async Task CreateOrder_PublishesNotification()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddMediator(typeof(CreateOrderHandler).Assembly);
    services.AddScoped<INotificationSubscriber<OrderCreatedNotification>, TestOrderNotificationHandler>();

    var serviceProvider = services.BuildServiceProvider();
    var mediator = serviceProvider.GetRequiredService<IMediator>();

    var command = new CreateOrderCommand
    {
        CustomerId = 1,
        CustomerEmail = "test@example.com",
        Items = new List<OrderItem>()
    };

    // Act
    var orderId = await mediator.Send(command);

    // Assert
    Assert.That(orderId, Is.GreaterThan(0));
    Assert.That(TestOrderNotificationHandler.NotificationsReceived, Is.EqualTo(1));
}

public class TestOrderNotificationHandler : INotificationSubscriber<OrderCreatedNotification>
{
    public static int NotificationsReceived { get; private set; }

    public Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        NotificationsReceived++;
        return Task.CompletedTask;
    }
}
```

## ECommerce.Api Implementation

The ECommerce.Api sample project provides a comprehensive demonstration of the notification system in action. It includes:

### Notification Types

1. **OrderCreatedNotification** - Published when a new order is created
2. **OrderStatusChangedNotification** - Published when order status changes
3. **ProductCreatedNotification** - Published when a new product is created
4. **ProductUpdatedNotification** - Published when a product is updated
5. **ProductStockLowNotification** - Published when product stock falls below threshold
6. **ProductOutOfStockNotification** - Published when product is out of stock

### Background Services

1. **EmailNotificationService** - Handles email notifications
2. **InventoryManagementService** - Manages inventory alerts and reordering

### Middleware

1. **NotificationLoggingMiddleware** - Logs all notifications
2. **NotificationMetricsMiddleware** - Tracks notification performance

### Example Notification Flow

When an order is created, the following notifications are triggered:

```csharp
// In CreateOrderHandler
public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
{
    // Create order logic...
    var order = new Order(request.CustomerId, request.CustomerEmail);

    // Add items and calculate total
    foreach (var item in request.Items)
    {
        order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);

        // Check stock levels and publish stock notifications if needed
        var product = await _context.Products.FindAsync(item.ProductId);
        if (product != null)
        {
            product.ReduceStock(item.Quantity);

            if (product.StockQuantity <= 10 && product.StockQuantity > 0)
            {
                await _mediator.Publish(new ProductStockLowNotification(
                    product.Id, product.Name, product.StockQuantity, 10, 50
                ), cancellationToken);
            }
            else if (product.StockQuantity <= 0)
            {
                await _mediator.Publish(new ProductOutOfStockNotification(
                    product.Id, product.Name, DateTime.UtcNow
                ), cancellationToken);
            }
        }
    }

    await _context.SaveChangesAsync(cancellationToken);

    // Publish order created notification
    await _mediator.Publish(new OrderCreatedNotification(
        order.Id,
        order.CustomerId,
        order.CustomerEmail,
        order.TotalAmount,
        order.Items.Select(i => new OrderItem
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList(),
        order.CreatedAt
    ), cancellationToken);

    return order.Id;
}
```

This single operation can trigger multiple notifications:

-   OrderCreatedNotification → EmailNotificationService sends confirmation email
-   OrderCreatedNotification → InventoryManagementService tracks inventory
-   ProductStockLowNotification → InventoryManagementService triggers reorder
-   ProductOutOfStockNotification → InventoryManagementService triggers urgent reorder

## Swagger Testing Guide

The ECommerce.Api includes comprehensive Swagger documentation for testing the notification system. Here's how to use it:

### 1. Start the Application

```bash
cd src/samples/ECommerce.Api
dotnet run
```

### 2. Open Swagger UI

Navigate to: `https://localhost:54336/swagger/index.html`

### 3. Watch Console Output

Keep the console window visible to see notification logs in real-time. Look for:

-   📧 Email notifications
-   📦 Inventory notifications
-   🔔 Notification middleware logs

### 4. Recommended Testing Sequence

#### Step 1: Create Test Product

```http
POST /api/products
{
  "name": "Notification Test Product",
  "description": "Product for testing notifications",
  "price": 99.99,
  "stockQuantity": 20
}
```

**Watch for:** ProductCreatedNotification

#### Step 2: Create Order

```http
POST /api/orders
{
  "customerId": 1,
  "customerEmail": "test@notifications.com",
  "orderItems": [
    {
      "productId": 1,
      "quantity": 15,
      "unitPrice": 99.99
    }
  ]
}
```

**Watch for:** OrderCreatedNotification, inventory tracking

#### Step 3: Process Order Workflow

```http
POST /api/orders/1/process-workflow
```

**Watch for:** Multiple OrderStatusChangedNotifications (Confirmed → Processing → Shipped → Delivered)

#### Step 4: Trigger Low Stock Alert

```http
POST /api/products/1/reduce-stock?quantity=8
```

**Watch for:** ProductStockLowNotification

#### Step 5: Trigger Out of Stock Alert

```http
POST /api/products/1/simulate-bulk-order?orderQuantity=10
```

**Watch for:** OrderCreatedNotification, ProductOutOfStockNotification

### 5. Available Notification Endpoints

#### Email Notification Endpoints

-   `POST /api/orders` - Create Order (triggers OrderCreatedNotification)
-   `POST /api/orders/process` - Process Order (triggers OrderCreatedNotification + status changes)
-   `PUT /api/orders/{id}/status` - Update Order Status (triggers OrderStatusChangedNotification)
-   `POST /api/orders/{id}/cancel` - Cancel Order (triggers OrderStatusChangedNotification)
-   `POST /api/orders/{id}/complete` - Complete Order Workflow (triggers multiple OrderStatusChangedNotifications)
-   `POST /api/orders/{id}/process-workflow` - Full Order Workflow (triggers multiple OrderStatusChangedNotifications)

#### Inventory Management Endpoints

-   `POST /api/products` - Create Product (triggers ProductCreatedNotification)
-   `PUT /api/products/{id}` - Update Product (triggers ProductUpdatedNotification)
-   `PUT /api/products/{id}/stock` - Update Stock (may trigger stock notifications)
-   `POST /api/products/{id}/deactivate` - Deactivate Product (triggers ProductUpdatedNotification)
-   `POST /api/products/{id}/reduce-stock?quantity={amount}` - Reduce Stock (triggers stock notifications)
-   `POST /api/products/{id}/simulate-bulk-order?orderQuantity={amount}` - Simulate Bulk Order (triggers multiple notifications)

#### Monitoring Endpoints

-   `GET /api/products/{id}` - Get Product by ID
-   `GET /api/products` - Get Products (with filtering)
-   `GET /api/products/low-stock?threshold={number}` - Get Low Stock Products
-   `GET /api/orders/{id}` - Get Order by ID
-   `GET /api/orders` - Get Orders (with status filtering)
-   `GET /api/orders/customer/{customerId}` - Get Customer Orders
-   `GET /api/orders/statistics` - Get Order Statistics

### 6. Console Output Examples

When testing, you'll see output like:

```
🔔 NOTIFICATION PUBLISHING: OrderCreatedNotification
📧 ORDER CONFIRMATION EMAIL SENT
   To: test@notifications.com
   Order: #123
   Total: $99.99
📦 INVENTORY TRACKING - Order #123
   Product: Notification Test Product (ID: 1)
   Quantity Ordered: 15
   Remaining Stock: 5
✅ NOTIFICATION COMPLETED: OrderCreatedNotification

⚠️ LOW STOCK ALERT
   Product: Notification Test Product (ID: 1)
   Current Stock: 5
   Minimum Threshold: 10
   Recommended Reorder: 50
📋 REORDER NOTIFICATION SENT TO PURCHASING
```

### 7. Testing Error Scenarios

Try these scenarios to test error handling:

```http
# Try to create order with invalid product
POST /api/orders
{
  "customerId": 999,
  "orderItems": [
    {
      "productId": 9999,
      "quantity": 1,
      "unitPrice": 99.99
    }
  ]
}

# Try to update non-existent order status
PUT /api/orders/9999/status
{
  "orderId": 9999,
  "status": 2,
  "notes": "This should fail"
}
```

## Automated Testing with PowerShell Script

The `test-notifications-endpoints.ps1` script provides comprehensive automated testing of all notification endpoints and BackgroundService warnings in the ECommerce.Api.

### What is test-notifications-endpoints.ps1?

The `test-notifications-endpoints.ps1` script is a PowerShell automation tool that:

-   Tests all notification endpoints in a systematic workflow
-   Triggers every type of notification in the ECommerce.Api
-   Generates comprehensive BackgroundService activity
-   Provides detailed console output with emojis for easy monitoring
-   Performs stress testing with concurrent operations
-   Validates system state after each test phase

### How to Use the Script

#### Prerequisites:

1. Start the ECommerce.Api project first:
    ```bash
    dotnet run --project src/samples/ECommerce.Api
    ```
2. Ensure the API is running on https://localhost:54336
3. Open a PowerShell terminal in the Blazing.Mediator root directory

#### Running the script:

```powershell
.\test-notifications-endpoints.ps1
```

The script will:

-   ✅ Automatically check if the API is running
-   ✅ Execute all test scenarios in sequence
-   ✅ Display colored progress messages
-   ✅ Show detailed results for each test
-   ✅ Provide final system status summary

### Comprehensive Test Scenarios Performed

#### PHASE 1: NOTIFICATION WORKFLOW DEMONSTRATION

**🆕 Step 1: Product Creation**

-   Creates "Notification Demo Product" with 20 units stock
-   Triggers: `ProductCreatedNotification`
-   Expected: `🔔 NOTIFICATION PUBLISHING: ProductCreatedNotification`

**📦 Step 2: Order Creation**

-   Creates order for 15 units of the demo product
-   Triggers: `OrderCreatedNotification` + `ProductStockLowNotification`
-   Expected: `🔔 NOTIFICATION PUBLISHING: OrderCreatedNotification`
-   Expected: `🔔 NOTIFICATION PUBLISHING: ProductStockLowNotification`

**⚙️ Step 3: Complete Order Workflow**

-   Processes order through: Confirmed → Processing → Shipped → Delivered
-   Triggers: Multiple `OrderStatusChangedNotifications` (4 notifications)
-   Expected: `🔔 NOTIFICATION PUBLISHING: OrderStatusChangedNotification` (x4)

**⚠️ Step 4: Low Stock Alert**

-   Creates additional order for 3 units
-   Reduces stock to critical levels (2 units remaining)
-   Triggers: `OrderCreatedNotification` + `ProductStockLowNotification`
-   Expected: `⚠️ LOW STOCK ALERT` messages in console

**🚨 Step 5: Out of Stock Alert**

-   Reduces stock by 5 units using reduce-stock endpoint
-   Triggers stock to reach 0 (out of stock)
-   Triggers: `ProductOutOfStockNotification`
-   Expected: `🚨 OUT OF STOCK ALERT - URGENT` messages

#### PHASE 2: ADVANCED NOTIFICATION TESTING SCENARIOS

**🎯 Test 1: Bulk Order Simulation**

-   Creates dedicated product with 25 units stock
-   Simulates bulk order for 10 units
-   Triggers: `OrderCreatedNotification` + inventory tracking
-   Expected: Bulk order processing notifications

**🆕 Test 2: Multiple Product Management**

-   Creates "Test Product 2" with 8 units stock
-   Sets up for additional testing scenarios
-   Triggers: `ProductCreatedNotification`

**📈 Test 3: Manual Order Status Progression**

-   Creates order and manually progresses through each status
-   Tests: Processing → Shipped → Delivered transitions
-   Triggers: `OrderStatusChangedNotification` for each transition
-   Expected: Status-specific email notifications

**🏁 Test 4: Quick Order Completion**

-   Uses the `/complete` endpoint for rapid status changes
-   Triggers: Multiple quick `OrderStatusChangedNotifications`
-   Expected: Rapid-fire notification processing

**❌ Test 5: Order Cancellation**

-   Creates order and immediately cancels it
-   Tests cancellation notification workflow
-   Triggers: `OrderStatusChangedNotification` for cancellation
-   Expected: `📧 ORDER CANCELLATION EMAIL SENT`

**🚀 Test 6: Concurrent Operations (Stress Test)**

-   Runs multiple bulk orders simultaneously
-   Tests notification system under concurrent load
-   Triggers: Multiple simultaneous notifications
-   Expected: Proper handling of concurrent notification processing

#### PHASE 3: FINAL SYSTEM STATUS CHECK

**📦 Low Stock Products Query**

-   Retrieves all products with stock below threshold (10 units)
-   Validates that previous tests properly affected inventory
-   Expected: List of products with low stock warnings

**📈 Order Statistics Summary**

-   Retrieves comprehensive order statistics
-   Shows total orders, delivered orders, cancelled orders
-   Validates that all test orders were processed correctly

### Expected Console Output Patterns

When running the script, watch for these notification patterns in the ECommerce.Api console:

#### 🔔 NOTIFICATION MIDDLEWARE LOGS:

```
info: ECommerce.Api.Application.Middleware.NotificationLoggingMiddleware[0]
      🔔 NOTIFICATION PUBLISHING: ProductCreatedNotification
info: ECommerce.Api.Application.Middleware.NotificationLoggingMiddleware[0]
      ✅ NOTIFICATION COMPLETED: ProductCreatedNotification
```

#### 📧 EMAIL NOTIFICATION SERVICE LOGS:

```
info: ECommerce.Api.Application.Services.EmailNotificationService[0]
      📧 ORDER CONFIRMATION EMAIL SENT - Customer: demo@notifications.com
info: ECommerce.Api.Application.Services.EmailNotificationService[0]
      📧 ORDER STATUS UPDATE EMAIL SENT - Order #X changed to Processing
```

#### 📦 INVENTORY MANAGEMENT SERVICE LOGS:

```
info: ECommerce.Api.Application.Services.InventoryManagementService[0]
      📦 INVENTORY TRACKING - Order #X processed, stock updated
info: ECommerce.Api.Application.Services.InventoryManagementService[0]
      ⚠️ LOW STOCK ALERT - Product: Notification Demo Product (Stock: 2)
info: ECommerce.Api.Application.Services.InventoryManagementService[0]
      🚨 OUT OF STOCK ALERT - URGENT - Product: Notification Demo Product
```

#### 📊 NOTIFICATION METRICS (every 10th notification):

```
info: ECommerce.Api.Application.Middleware.NotificationMetricsMiddleware[0]
      📊 NOTIFICATION METRICS: OrderCreatedNotification
info: ECommerce.Api.Application.Middleware.NotificationMetricsMiddleware[0]
         Total Count: 10
info: ECommerce.Api.Application.Middleware.NotificationMetricsMiddleware[0]
         Success Count: 10
info: ECommerce.Api.Application.Middleware.NotificationMetricsMiddleware[0]
         Success Rate: 100.0%
```

### Script Success Indicators

The script will display these success messages:

-   ✅ ECommerce.Api is running
-   ✅ Created product with ID: X
-   ✅ Created order with ID: X
-   ✅ Order workflow completed: Order workflow completed successfully
-   ✅ Stock reduced: Stock reduced by 5 units
-   ✅ Bulk order simulated: Bulk order simulation completed
-   ✅ Order status updated to Processing/Shipped/Delivered
-   ✅ Order completed: Order completed successfully
-   ✅ Order cancelled: Order cancelled successfully
-   ✅ Concurrent bulk orders completed
-   ✅ Found X low stock products
-   ✅ Total orders: X

#### Final success message:

```
🎉 NOTIFICATION TESTING COMPLETED SUCCESSFULLY!
All notification endpoints have been tested and should have triggered:
📧 Email notifications (OrderCreatedNotification, OrderStatusChangedNotification)
📦 Inventory notifications (ProductStockLowNotification, ProductOutOfStockNotification)
🆕 Product notifications (ProductCreatedNotification)
```

### Troubleshooting

#### Common issues and solutions:

**Issue: "ECommerce.Api is not running"**

-   Solution: Start the API first with: `dotnet run --project src/samples/ECommerce.Api`

**Issue: "Insufficient stock" errors**

-   Solution: This is expected behavior - the script tests stock depletion scenarios

**Issue: PowerShell execution policy errors**

-   Solution: Run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

**Issue: SSL certificate errors**

-   Solution: The script uses `-SkipCertificateCheck`, but ensure HTTPS is working

**Issue: Script stops with validation errors**

-   Solution: Check that all required fields are properly formatted in the script

### Customization Options

You can modify the script for different testing scenarios:

#### Change base URL:

```powershell
$baseUrl = "https://localhost:YOUR_PORT"
```

#### Modify stock quantities:

```powershell
stockQuantity = 50  # Increase for more extensive testing
```

#### Adjust order quantities:

```powershell
quantity = 20  # Test with larger orders
```

#### Add more concurrent tests:

Increase the number of `Start-Job` blocks in Test 6

#### Change notification thresholds:

```powershell
/api/products/low-stock?threshold=5  # Lower threshold for testing
```

### Continuous Testing

For continuous testing during development:

1. Keep the ECommerce.Api running
2. Run the script multiple times to generate ongoing notification activity
3. Each run creates new products/orders, so data accumulates
4. Monitor console output for notification patterns
5. Use the script to validate notification system changes

The script is designed to be run repeatedly without conflicts, making it perfect for ongoing development and testing workflows.

```

```

## Best Practices

### 1. Keep Notifications Immutable

```csharp
// ✅ Good - Immutable notification
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedNotification(int orderId, string customerEmail, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
    }
}

// ❌ Bad - Mutable notification
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; set; }
    public string CustomerEmail { get; set; }
    public decimal TotalAmount { get; set; }
}
```

### 2. Use Descriptive Notification Names

```csharp
// ✅ Good - Clear, descriptive names
public class OrderShippedNotification : INotification { }
public class PaymentProcessedNotification : INotification { }
public class InventoryStockLowNotification : INotification { }

// ❌ Bad - Vague names
public class OrderNotification : INotification { }
public class UpdateNotification : INotification { }
public class AlertNotification : INotification { }
```

### 3. Handle Exceptions Gracefully

```csharp
// ✅ Good - Graceful error handling
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    try
    {
        await _emailService.SendOrderConfirmationAsync(notification.CustomerEmail, notification.OrderId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
        // Don't rethrow - we don't want to fail the entire notification pipeline
    }
}

// ❌ Bad - Unhandled exceptions
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    // This can crash the entire notification pipeline
    await _emailService.SendOrderConfirmationAsync(notification.CustomerEmail, notification.OrderId);
}
```

### 4. Use Structured Logging

```csharp
// ✅ Good - Structured logging
_logger.LogInformation("Order confirmation email sent for order {OrderId} to {CustomerEmail}",
    notification.OrderId, notification.CustomerEmail);

// ❌ Bad - String concatenation
_logger.LogInformation($"Order confirmation email sent for order {notification.OrderId} to {notification.CustomerEmail}");
```

### 5. Implement Idempotency

```csharp
// ✅ Good - Idempotent notification handler
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    // Check if we've already processed this notification
    var existingEmail = await _emailRepository.GetByOrderIdAsync(notification.OrderId);
    if (existingEmail != null)
    {
        _logger.LogInformation("Order confirmation email already sent for order {OrderId}", notification.OrderId);
        return;
    }

    // Process the notification
    await _emailService.SendOrderConfirmationAsync(notification.CustomerEmail, notification.OrderId);

    // Record that we've processed this notification
    await _emailRepository.SaveAsync(new EmailRecord(notification.OrderId, notification.CustomerEmail));
}
```

### 6. Use Cancellation Tokens

```csharp
// ✅ Good - Respects cancellation
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    await _emailService.SendOrderConfirmationAsync(
        notification.CustomerEmail,
        notification.OrderId,
        cancellationToken);
}

// ❌ Bad - Ignores cancellation
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    await _emailService.SendOrderConfirmationAsync(notification.CustomerEmail, notification.OrderId);
}
```

### 7. Keep Notifications Lightweight

```csharp
// ✅ Good - Contains essential data only
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }

    // Constructor...
}

// ❌ Bad - Contains too much data
public class OrderCreatedNotification : INotification
{
    public Order CompleteOrder { get; }
    public Customer CompleteCustomer { get; }
    public List<Product> AllProducts { get; }
    public PaymentDetails PaymentDetails { get; }

    // Constructor...
}
```

## Troubleshooting

### Common Issues

#### 1. Notifications Not Being Received

**Problem:** Notification subscribers are not being called.

**Solutions:**

-   Ensure subscribers are registered in DI container
-   Check that notification is being published with correct type
-   Verify assembly scanning is configured correctly

```csharp
// Make sure this is called
builder.Services.AddMediator(typeof(Program).Assembly);

// And that your subscriber is properly registered
builder.Services.AddScoped<INotificationSubscriber<OrderCreatedNotification>, OrderEmailHandler>();
```

#### 2. Notifications Failing Silently

**Problem:** Exceptions in notification handlers are not being logged.

**Solutions:**

-   Add try-catch blocks in notification handlers
-   Use notification middleware for centralized error handling
-   Check your logging configuration

```csharp
public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
{
    try
    {
        // Your logic here
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process notification {NotificationType}", typeof(OrderCreatedNotification).Name);
        // Don't rethrow unless you want to fail the entire pipeline
    }
}
```

#### 3. Performance Issues

**Problem:** Notification processing is slow.

**Solutions:**

-   Make notification handlers asynchronous
-   Use background services for heavy processing
-   Implement proper cancellation token usage
-   Consider using a message queue for high-volume notifications

```csharp
// Use background services for heavy processing
public class HeavyProcessingService : BackgroundService, INotificationSubscriber<OrderCreatedNotification>
{
    private readonly Channel<OrderCreatedNotification> _channel;

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Queue the notification for background processing
        await _channel.Writer.WriteAsync(notification, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var notification in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            // Process the notification in the background
            await ProcessNotificationAsync(notification, stoppingToken);
        }
    }
}
```

#### 4. Memory Leaks

**Problem:** Memory usage increases over time.

**Solutions:**

-   Properly dispose of resources in notification handlers
-   Use scoped services where appropriate
-   Unsubscribe from notifications when no longer needed

```csharp
public class NotificationService : IDisposable
{
    private readonly IMediator _mediator;

    public NotificationService(IMediator mediator)
    {
        _mediator = mediator;
        _mediator.Subscribe<OrderCreatedNotification>(this);
    }

    public void Dispose()
    {
        _mediator.Unsubscribe<OrderCreatedNotification>(this);
    }
}
```

### Debugging Tips

1. **Enable Debug Logging** - Set logging level to Debug to see detailed notification flow
2. **Use Notification Middleware** - Add logging middleware to trace notification execution
3. **Monitor Performance** - Use metrics middleware to track notification performance
4. **Test in Isolation** - Unit test notification handlers individually
5. **Check Registration** - Verify all subscribers are properly registered in DI container

### Getting Help

If you're still having issues:

1. Check the [sample projects](../src/samples/) for working examples
2. Review the [main documentation](MEDIATOR_PATTERN_GUIDE.md) for general guidance
3. Look at the [tests](../tests/) for usage patterns
4. Create an issue on the GitHub repository with a minimal reproduction

## Conclusion

The Blazing.Mediator notification system provides a powerful, flexible way to implement event-driven architecture in your .NET applications. By following the patterns and best practices outlined in this guide, you can build scalable, maintainable applications that respond to domain events in a loosely coupled manner.

The ECommerce.Api sample project demonstrates all these concepts in action, providing a comprehensive reference implementation that you can study and adapt for your own projects.

Remember to:

-   Keep notifications immutable and lightweight
-   Handle exceptions gracefully
-   Use structured logging
-   Implement proper cancellation token support
-   Test your notification handlers thoroughly

With these practices, you'll be able to build robust, event-driven applications that scale with your business needs.
