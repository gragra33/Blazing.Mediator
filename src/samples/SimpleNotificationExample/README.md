# Blazing.Mediator - Simple Notification Example

This console application demonstrates the notification system in Blazing.Mediator using **only simple scoped classes** instead of background services. This is the **recommended approach** for most notification scenarios.

## ✅ Why This Approach is Recommended

**This example uses the default scoped lifetime for `IMediator`, which is the RECOMMENDED approach.**

Unlike the complex NotificationExample that uses background services with singleton mediator registration, this example:

-   ✅ **Uses the recommended scoped mediator** - follows best practices
-   ✅ **Simple and straightforward** - easy to understand and maintain
-   ✅ **Proper resource management** - automatic cleanup by DI container
-   ✅ **Testable and predictable** - standard dependency injection patterns
-   ✅ **No memory leak risks** - proper scoped lifecycle management
-   ✅ **External subscription management** - clear and explicit control

```csharp
// This example uses the recommended scoped registration:
services.AddMediator(config => { ... }, Assembly.GetExecutingAssembly());
// IMediator is automatically registered as scoped (default)
```

## 🎯 What This Example Demonstrates

### Core Concepts

-   **Multiple Simple Subscribers**: Two different scoped classes subscribing to the same `OrderCreatedNotification`
-   **Standard Classes Only**: Simple, scoped notification subscribers (no background services)
-   **Manual Subscription**: Runtime subscription management with `IMediator.Subscribe()`
-   **Notification Middleware**: Cross-cutting concerns like logging
-   **Proper Scoped Lifecycle**: Services created and disposed per operation scope

## 🔑 Key Features: Simple Scoped Services

### 📧 Standard Scoped Classes

**Characteristics:**

-   ✅ **Simple setup** - just register in DI container as scoped
-   ✅ **Scoped lifecycle** - created per operation/request
-   ✅ **External management** - subscription handled by calling code
-   ✅ **Automatic cleanup** - disposed by DI container
-   ✅ **Testable** - easy to unit test with standard mocking
-   ✅ **Predictable** - follows standard DI patterns

**Best for:**

-   Most notification scenarios
-   Request-scoped operations
-   Simple event handlers
-   Services that process events on-demand
-   Production applications

## 📋 Implementation Guide

### Prerequisites: Default Scoped Mediator Registration

This example uses the default and recommended scoped lifetime for the mediator:

```csharp
// In Program.cs - Uses default scoped registration (RECOMMENDED)
services.AddMediator(config =>
{
    config.AddNotificationMiddleware<NotificationLoggingMiddleware>();
}, Assembly.GetExecutingAssembly());
// IMediator is automatically registered as scoped
```

**This scoped registration is recommended because:**

-   ✅ Follows dependency injection best practices
-   ✅ Proper resource management and cleanup
-   ✅ Predictable lifecycle management
-   ✅ Works well with request/operation scopes
-   ✅ No memory leak concerns

### Step 1: Simple Email Notification Handler

```csharp
public class EmailNotificationHandler : INotificationSubscriber<OrderCreatedNotification>
{
    private readonly ILogger<EmailNotificationHandler> _logger;

    public EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📧 ORDER CONFIRMATION EMAIL SENT");
            _logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            _logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            _logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            _logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
            
            // Simulate email processing
            await Task.Delay(100, cancellationToken);
            
            // ... email sending logic
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for order {OrderId}", notification.OrderId);
            // Don't rethrow - prevents affecting other subscribers
        }
    }
}
```

### Step 2: Simple Inventory Notification Handler

```csharp
public class InventoryNotificationHandler : INotificationSubscriber<OrderCreatedNotification>
{
    private readonly ILogger<InventoryNotificationHandler> _logger;

    public InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📦 INVENTORY UPDATE PROCESSING");
            _logger.LogInformation("   Order: #{OrderId}", notification.OrderId);

            // Process each item in the order
            foreach (var item in notification.Items)
            {
                await ProcessInventoryUpdate(item, cancellationToken);
            }

            _logger.LogInformation("✅ INVENTORY UPDATE COMPLETED for Order #{OrderId}", notification.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process inventory update for order {OrderId}", notification.OrderId);
        }
    }

    private async Task ProcessInventoryUpdate(OrderItem item, CancellationToken cancellationToken)
    {
        // Simulate inventory processing
        await Task.Delay(50, cancellationToken);
        
        _logger.LogInformation("   📦 Updating inventory for {ProductName}", item.ProductName);
        // ... inventory logic
    }
}
```

### Step 3: Registration and Subscription

```csharp
// In Program.cs ConfigureServices
services.AddScoped<EmailNotificationHandler>();
services.AddScoped<InventoryNotificationHandler>();

// Later, in application code
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var mediator = services.GetRequiredService<IMediator>();
var emailHandler = services.GetRequiredService<EmailNotificationHandler>();
var inventoryHandler = services.GetRequiredService<InventoryNotificationHandler>();

// Subscribe both handlers
mediator.Subscribe<OrderCreatedNotification>(emailHandler);
mediator.Subscribe<OrderCreatedNotification>(inventoryHandler);
```

**Key Points for Simple Classes:**

1. **Register as Scoped**: Use `AddScoped<T>()` for notification handlers
2. **External Subscription**: Application code manages subscription
3. **Simple Constructor**: Just inject dependencies normally
4. **No Cleanup Needed**: DI container handles disposal automatically

## 🚀 Running the Example

### Prerequisites

-   .NET 9.0 or later
-   Terminal/Command Prompt

### Steps

1. Navigate to the example directory:

    ```bash
    cd src/samples/SimpleNotificationExample
    ```

2. Run the application:

    ```bash
    dotnet run
    ```

3. Observe the output to see both subscribers working:

```
🔔 Blazing.Mediator - Simple Notification Example
===============================================

✅ EmailNotificationHandler subscribed
✅ InventoryNotificationHandler subscribed

🔔 Publishing notification: OrderCreatedNotification
📧 ORDER CONFIRMATION EMAIL SENT              ← Email handler processes
📦 INVENTORY UPDATE PROCESSING                 ← Inventory handler processes
✅ Notification completed in 180ms
```

## ⚡ Troubleshooting Common Issues

### Issue 1: Handlers Not Receiving Notifications

**Problem:** Handlers are registered but don't receive notifications.

**Cause:** Forgot to subscribe the instances.

**Solution:** Ensure external subscription:

```csharp
// ❌ WRONG - just registering in DI isn't enough
services.AddScoped<EmailNotificationHandler>();

// ✅ CORRECT - must also subscribe
var handler = scope.ServiceProvider.GetRequiredService<EmailNotificationHandler>();
mediator.Subscribe<OrderCreatedNotification>(handler);
```

### Issue 2: Handlers Called Multiple Times

**Problem:** Notification handlers seem to be called multiple times.

**Cause:** Multiple subscriptions of the same handler.

**Solution:** Subscribe each handler instance only once:

```csharp
// ✅ CORRECT - subscribe once per scope
mediator.Subscribe<OrderCreatedNotification>(emailHandler);

// ❌ WRONG - don't subscribe the same instance multiple times
mediator.Subscribe<OrderCreatedNotification>(emailHandler);
mediator.Subscribe<OrderCreatedNotification>(emailHandler); // Duplicate!
```

### Issue 3: Scope Disposal Issues

**Problem:** Handlers stop working after scope disposal.

**Cause:** The scope containing the handlers was disposed.

**Solution:** Manage scope lifetime appropriately:

```csharp
// ✅ CORRECT - keep scope alive while needed
using var scope = host.Services.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
// ... subscribe handlers
// ... publish notifications
// Scope disposed here automatically
```

## 🎛️ Architecture Overview

```
Application Startup
        │
        ├─► Register scoped IMediator (default/recommended)
        │
        ├─► Register EmailNotificationHandler (scoped)
        │
        └─► Register InventoryNotificationHandler (scoped)

Application Execution
        │
        ├─► Create scope
        │
        ├─► Resolve handlers from scope
        │
        ├─► Subscribe handlers to mediator
        │
        └─► Publish notifications

When OrderCreatedNotification is published:
        │
        ├─► NotificationLoggingMiddleware (logs)
        │
        ├─► EmailNotificationHandler.OnNotification()
        │   └─► Sends confirmation email
        │
        └─► InventoryNotificationHandler.OnNotification()
            └─► Updates inventory, checks stock
```

## 🏆 Best Practices

### For Simple Scoped Services:

1. **Use scoped registration** - `services.AddScoped<THandler>()`
2. **Keep constructors simple** - just inject what you need
3. **Let calling code manage subscription** - don't self-subscribe
4. **Use try-catch** in notification handlers for resilience
5. **Keep handlers lightweight** - offload heavy work if needed
6. **Consider scoped lifetime** when designing handler logic

### General:

1. **Don't rethrow exceptions** in notification handlers (breaks other subscribers)
2. **Use structured logging** with meaningful context
3. **Test notification handling** with unit tests
4. **Consider using cancellation tokens** for long-running operations
5. **Subscribe once per handler instance** to avoid duplicates

## 🔗 When to Use This Approach

### Use Simple Scoped Services When:

-   ✅ **Most notification scenarios** - this is the default choice
-   ✅ Simple, stateless notification handling
-   ✅ Request-scoped or operation-scoped processing
-   ✅ Easy testability is important
-   ✅ Minimal setup and configuration needed
-   ✅ Following dependency injection best practices
-   ✅ Building production applications

### Consider Background Services Only When:

-   ⚠️ **Very specific scenarios** with continuous monitoring needs
-   ⚠️ Service must run for the entire application lifetime
-   ⚠️ Complex initialization or persistent state management required
-   ⚠️ Independent processing that doesn't fit request scopes
-   ⚠️ **Be aware of the complexity and risks involved**

## 🆚 Comparison with Background Service Approach

| Aspect | Simple Scoped Services (This Example) | Background Services (NotificationExample) |
|--------|----------------------------------------|-------------------------------------------|
| **Mediator Lifetime** | ✅ Scoped (recommended) | ❌ Singleton (not recommended) |
| **Complexity** | ✅ Simple | ❌ Complex |
| **Resource Management** | ✅ Automatic | ⚠️ Manual scope management |
| **Memory Leaks** | ✅ No risk | ⚠️ Risk if not handled properly |
| **Testability** | ✅ Easy | ⚠️ More difficult |
| **Production Ready** | ✅ Yes | ⚠️ Limited scenarios only |
| **Subscription Management** | ✅ External/explicit | ⚠️ Self-managing |

This example demonstrates the recommended approach for most notification scenarios in Blazing.Mediator!
