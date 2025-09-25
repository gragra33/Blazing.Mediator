# Blazing.Mediator - Notification Subscriber Example

This console application demonstrates the notification system in Blazing.Mediator using **manual subscription with simple scoped classes**. This approach is **required for client applications** where automatic handler discovery is not available.

## ✅ When This Approach is Required

**This example demonstrates manual subscription, which is REQUIRED for client applications.**

This approach is necessary for:

-   🎯 **Blazor WebAssembly** - Client-side applications without server-side handler discovery
-   📱 **MAUI Applications** - Cross-platform mobile and desktop apps
-   🖥️ **WinForms Applications** - Traditional Windows desktop applications
-   🎨 **WPF Applications** - Windows Presentation Foundation applications
-   🖥️ **Console Applications** - Command-line applications and background services

### Why Manual Subscription?

In client applications, you typically need manual subscription because:

-   ❌ **No automatic handler discovery** - Client apps don't scan for `INotificationHandler` implementations
-   ❌ **Different DI container behavior** - Limited reflection capabilities in some client frameworks
-   ❌ **AOT compatibility** - Ahead-of-time compilation restrictions
-   ✅ **Explicit control needed** - Client apps often need precise control over when handlers are active
-   ✅ **Performance considerations** - Manual registration can be more efficient in resource-constrained environments

This example demonstrates:

-   ✅ **Manual subscription management** - Explicit control over handler registration
-   ✅ **Simple scoped classes** - Clean, testable notification handlers
-   ✅ **Proper resource management** - Automatic cleanup by DI container
-   ✅ **Testable and predictable** - Standard dependency injection patterns
-   ✅ **No memory leak risks** - Proper scoped lifecycle management
-   ✅ **Auto-discovery of middleware** - Middleware can still be auto-discovered
-   ✅ **Statistics tracking** - Built-in analysis of mediator usage

```csharp
// This example uses manual subscription (REQUIRED for client apps):
services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithNotificationMiddlewareDiscovery();
}, Assembly.GetExecutingAssembly());
// IMediator is automatically registered as scoped (default)

// Manual subscription required for client applications
mediator.Subscribe(emailHandler);
mediator.Subscribe(inventoryHandler);
```

## 🎯 What This Example Demonstrates

### Core Concepts

-   **Manual Subscription**: Required for client applications like Blazor WASM, MAUI, WinForms, WPF
-   **Multiple Simple Subscribers**: Two different scoped classes subscribing to the same `OrderCreatedNotification`
-   **Standard Classes Only**: Simple, scoped notification subscribers
-   **Runtime Subscription Management**: Explicit control with `IMediator.Subscribe()`
-   **Auto-discovered Middleware**: Automatic discovery and registration of notification middleware
-   **Pipeline Inspection**: Analysis of the middleware pipeline for debugging
-   **Statistics Tracking**: Built-in analysis of queries, commands, and notifications
-   **Proper Scoped Lifecycle**: Services created and disposed per operation scope

### Key Features Demonstrated

-   **EmailNotificationHandler** - Sends order confirmation emails
-   **InventoryNotificationHandler** - Updates inventory and checks stock levels
-   **Four Auto-discovered Middleware**:
    - `NotificationValidationMiddleware` (Order: 5)
    - `NotificationLoggingMiddleware` (Order: 10) 
    - `NotificationMetricsMiddleware` (Order: 300)
    - `NotificationAuditMiddleware` (Order: 400)

## 🔑 Key Features: Manual Subscription for Client Apps

### 📧 Manual Subscriber Registration

**Why Manual Subscription:**

-   🎯 **Client App Requirement** - Blazor WASM, MAUI, WinForms, WPF need explicit registration
-   🎯 **AOT Compatibility** - Works with ahead-of-time compilation
-   🎯 **Performance** - No runtime reflection overhead
-   🎯 **Explicit Control** - You control exactly when handlers are active
-   🎯 **Resource Management** - Precise control over memory usage

**Characteristics:**

-   ✅ **Simple setup** - Register in DI container as scoped, then manually subscribe
-   ✅ **Scoped lifecycle** - Created per operation/request
-   ✅ **Manual management** - Subscription handled by application code
-   ✅ **Automatic cleanup** - Disposed by DI container
-   ✅ **Testable** - Easy to unit test with standard mocking
-   ✅ **Client-app friendly** - Works in all client application types

**Best for:**

-   Blazor WebAssembly applications
-   MAUI mobile and desktop apps
-   WinForms desktop applications
-   WPF desktop applications
-   Console applications (as demonstrated here)
-   Any scenario requiring explicit handler control

## 📋 Implementation Guide

### Prerequisites: Default Scoped Mediator Registration

This example uses the default and recommended scoped lifetime for the mediator with auto-discovery:

```csharp
// In Program.cs - Uses default scoped registration with auto-discovery (RECOMMENDED)
services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithNotificationMiddlewareDiscovery();
}, Assembly.GetExecutingAssembly());
// IMediator is automatically registered as scoped
```

**This scoped registration is recommended because:**

-   ✅ Follows dependency injection best practices
-   ✅ Proper resource management and cleanup
-   ✅ Predictable lifecycle management
-   ✅ Works well with request/operation scopes
-   ✅ No memory leak concerns
-   ✅ Compatible with client applications

### Step 1: Email Notification Handler

```csharp
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email processing delay
            await Task.Delay(100, cancellationToken);

            logger.LogInformation("# ORDER CONFIRMATION EMAIL SENT");
            logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Total: ${TotalAmount:F2}", notification.TotalAmount);
            logger.LogInformation("   Items Count: {ItemCount}", notification.Items.Count);

            foreach (var item in notification.Items)
            {
                logger.LogInformation("   - {ProductName} x{Quantity} @ ${UnitPrice:F2}",
                    item.ProductName, item.Quantity, item.UnitPrice);
            }

            logger.LogInformation("   Created: {CreatedAt:yyyy-MM-dd HH:mm:ss}", notification.CreatedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
            // Don't rethrow - prevents affecting other subscribers
        }
    }
}
```

### Step 2: Inventory Notification Handler

```csharp
public class InventoryNotificationHandler(ILogger<InventoryNotificationHandler> logger)
    : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("- INVENTORY UPDATE PROCESSING");
            logger.LogInformation("   Order: #{OrderId}", notification.OrderId);
            logger.LogInformation("   Customer: {CustomerName}", notification.CustomerName);

            // Process each item in the order
            foreach (var item in notification.Items)
            {
                await ProcessInventoryUpdate(item, cancellationToken);
            }

            logger.LogInformation("- INVENTORY UPDATE COMPLETED for Order #{OrderId}", notification.OrderId);

            // Check for low stock alerts
            await CheckForStockAlerts(notification.Items, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process inventory update for order {OrderId}", notification.OrderId);
        }
    }

    private async Task ProcessInventoryUpdate(OrderItem item, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
        
        logger.LogInformation("   - Updating inventory for {ProductName} (ID: {ProductId})",
            item.ProductName, item.ProductId);
        logger.LogInformation("      Quantity sold: {Quantity}", item.Quantity);

        // Simulate stock calculation
        var currentStock = Random.Shared.Next(0, 100);
        var newStock = Math.Max(0, currentStock - item.Quantity);

        logger.LogInformation("      Stock before: {CurrentStock}, after: {NewStock}", currentStock, newStock);
    }

    private async Task CheckForStockAlerts(List<OrderItem> items, CancellationToken cancellationToken)
    {
        await Task.Delay(25, cancellationToken);

        foreach (var item in items)
        {
            var stockLevel = Random.Shared.Next(0, 50);
            
            if (stockLevel <= 10)
            {
                if (stockLevel == 0)
                {
                    logger.LogError("! OUT OF STOCK ALERT - URGENT");
                    logger.LogError("   Product: {ProductName} (ID: {ProductId})", item.ProductName, item.ProductId);
                }
                else
                {
                    logger.LogWarning("*  LOW STOCK ALERT");
                    logger.LogWarning("   Product: {ProductName} (ID: {ProductId})", item.ProductName, item.ProductId);
                }
            }
        }
    }
}
```

### Step 3: Registration and Manual Subscription

```csharp
// In Program.cs - Service registration
services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithNotificationMiddlewareDiscovery();
}, Assembly.GetExecutingAssembly());

services.AddScoped<EmailNotificationHandler>();
services.AddScoped<InventoryNotificationHandler>();
services.AddScoped<Runner>();

// Later, in application code - MANUAL SUBSCRIPTION (Required for client apps)
using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var mediator = services.GetRequiredService<IMediator>();
var emailHandler = services.GetRequiredService<EmailNotificationHandler>();
var inventoryHandler = services.GetRequiredService<InventoryNotificationHandler>();

// Manual subscription - REQUIRED for client applications
mediator.Subscribe(emailHandler);
mediator.Subscribe(inventoryHandler);
```

**Key Points for Client Applications:**

1. **Register as Scoped**: Use `AddScoped<T>()` for notification handlers
2. **Manual Subscription Required**: Use `mediator.Subscribe(handler)` - no automatic discovery
3. **Simple Constructor**: Use primary constructor with dependency injection
4. **Explicit Control**: You decide exactly when handlers are active
5. **No Cleanup Needed**: DI container handles disposal automatically

## 🚀 Running the Example

### Prerequisites

-   .NET 9.0 or later
-   Terminal/Command Prompt

### Steps

1. Navigate to the example directory:

    ```bash
    cd src/samples/NotificationSubscriberExample
    ```

2. Run the application:

    ```bash
    dotnet run
    ```

3. Observe the comprehensive output showing:

```
==============================================
* Blazing.Mediator - Notification Subscriber Example
==============================================

=== MEDIATOR TYPE ANALYSIS ===
* QUERIES DISCOVERED:
  (No queries discovered)

* COMMANDS DISCOVERED:
  (No commands discovered)

=== Notification Middleware Pipeline Inspection ===
Auto-discovered notification middleware (in execution order):
  - [5] NotificationValidationMiddleware
  - [10] NotificationLoggingMiddleware  
  - [300] NotificationMetricsMiddleware
  - [400] NotificationAuditMiddleware

=== Starting Order Processing ===

📋 Creating Order #1001 for Alice Johnson
* Validating notification: OrderCreatedNotification
# Validation passed for: OrderCreatedNotification
* Publishing notification: OrderCreatedNotification
& Audit: Started processing OrderCreatedNotification
- INVENTORY UPDATE PROCESSING                   ← Inventory handler
# ORDER CONFIRMATION EMAIL SENT               ← Email handler
& Audit: Completed OrderCreatedNotification in XXXms
$ Metrics updated for OrderCreatedNotification: XXXms
# Notification completed: OrderCreatedNotification in XXXms

=== FINAL STATISTICS ===
Mediator Statistics:
Queries: 0
Commands: 0
Notifications: 3
```

## 🔧 Auto-Discovered Middleware

The application automatically discovers and registers these middleware in order:

### 1. NotificationValidationMiddleware (Order: 5)
```csharp
public class NotificationValidationMiddleware : INotificationMiddleware
{
    public int Order => 5; // Execute first
    
    public async Task InvokeAsync<TNotification>(TNotification? notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        // Validates OrderId, TotalAmount, CustomerEmail
        if (notification is OrderCreatedNotification order)
        {
            if (order.OrderId <= 0) throw new InvalidOperationException("OrderId must be positive");
            if (order.TotalAmount <= 0) throw new InvalidOperationException("TotalAmount must be positive");
            if (string.IsNullOrWhiteSpace(order.CustomerEmail)) 
                throw new InvalidOperationException("CustomerEmail is required");
        }
        
        await next(notification, cancellationToken);
    }
}
```

### 2. NotificationLoggingMiddleware (Order: 10)
```csharp
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    public int Order => 10;
    
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("* Publishing notification: {NotificationName}", typeof(TNotification).Name);
        
        try
        {
            await next(notification, cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("# Notification completed: {NotificationName} in {Duration}ms",
                typeof(TNotification).Name, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "! Notification failed");
            throw;
        }
    }
}
```

## ⚡ Troubleshooting Common Issues

### Issue 1: Handlers Not Receiving Notifications (Most Common in Client Apps)

**Problem:** Handlers are registered but don't receive notifications.

**Cause:** Forgot to manually subscribe the instances (automatic discovery doesn't work in client apps).

**Solution:** Always use manual subscription in client applications:

```csharp
// ❌ WRONG - just registering in DI isn't enough for client apps
services.AddScoped<EmailNotificationHandler>();

// ✅ CORRECT - must manually subscribe in client applications
var handler = scope.ServiceProvider.GetRequiredService<EmailNotificationHandler>();
mediator.Subscribe(handler);
```

### Issue 2: Middleware Not Auto-Discovered

**Problem:** Custom middleware not executing.

**Cause:** Auto-discovery not enabled or middleware doesn't implement `INotificationMiddleware`.

**Solution:** Enable auto-discovery and implement correctly:

```csharp
// ✅ CORRECT - enable auto-discovery
services.AddMediator(config =>
{
    config.WithNotificationMiddlewareDiscovery();
}, Assembly.GetExecutingAssembly());

// ✅ CORRECT - implement INotificationMiddleware
public class CustomMiddleware : INotificationMiddleware
{
    public int Order => 100; // Define execution order
    
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        // Your logic here
        await next(notification, cancellationToken);
    }
}
```

### Issue 3: Statistics Not Available

**Problem:** `MediatorStatistics` showing zero counts.

**Cause:** Statistics tracking not enabled.

**Solution:** Enable statistics tracking:

```csharp
// ✅ CORRECT - enable statistics
services.AddMediator(config =>
{
    config.WithStatisticsTracking()
          .WithNotificationMiddlewareDiscovery();
}, Assembly.GetExecutingAssembly());
```

### Issue 4: Client App Performance Issues

**Problem:** Slow notification handling in client applications.

**Cause:** Too many active subscriptions or inefficient handlers.

**Solution:** Manage subscriptions lifecycle appropriately:

```csharp
// ✅ CORRECT - manage subscription lifecycle in client apps
public class NotificationManager : IDisposable
{
    private readonly List<IDisposable> _subscriptions = new();
    
    public void Subscribe<T>(INotificationSubscriber<T> handler) where T : INotification
    {
        var subscription = _mediator.Subscribe(handler);
        _subscriptions.Add(subscription);
    }
    
    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
        _subscriptions.Clear();
    }
}
```

## 🎛️ Architecture Overview

```
Client Application Startup (Blazor WASM, MAUI, WinForms, WPF)
        │
        ├─► Register scoped IMediator (default/recommended)
        │
        ├─► Auto-discover notification middleware
        │   ├─► NotificationValidationMiddleware (Order: 5)
        │   ├─► NotificationLoggingMiddleware (Order: 10)
        │   ├─► NotificationMetricsMiddleware (Order: 300)
        │   └─► NotificationAuditMiddleware (Order: 400)
        │
        ├─► Register EmailNotificationHandler (scoped)
        │
        ├─► Register InventoryNotificationHandler (scoped)
        │
        └─► Register Runner (scoped)

Client Application Execution
        │
        ├─► Create scope
        │
        ├─► Analyze mediator types (queries, commands, notifications)
        │
        ├─► Inspect middleware pipeline
        │
        ├─► Resolve handlers from scope
        │
        ├─► MANUAL SUBSCRIPTION (Required for client apps)
        │   ├─► mediator.Subscribe(emailHandler)
        │   └─► mediator.Subscribe(inventoryHandler)
        │
        └─► Publish notifications

When OrderCreatedNotification is published:
        │
        ├─► NotificationValidationMiddleware (validates data)
        │
        ├─► NotificationLoggingMiddleware (logs start/end)
        │
        ├─► NotificationAuditMiddleware (audit trail)
        │
        ├─► EmailNotificationHandler.OnNotification()
        │   └─► Sends confirmation email with order details
        │
        ├─► InventoryNotificationHandler.OnNotification()
        │   ├─► Updates inventory for each item
        │   └─► Checks for stock alerts
        │
        ├─► NotificationMetricsMiddleware (records metrics)
        │
        └─► Statistics updated
```

## 🏆 Best Practices

### For Client Applications (Blazor WASM, MAUI, WinForms, WPF):

1. **Use scoped registration** - `services.AddScoped<THandler>()`
2. **Always use manual subscription** - `mediator.Subscribe(handler)` (automatic discovery not available)
3. **Enable auto-discovery for middleware** - `config.WithNotificationMiddlewareDiscovery()`
4. **Use primary constructors** - Clean dependency injection syntax
5. **Manage subscription lifecycle** - Consider implementing `IDisposable` for cleanup
6. **Use try-catch** in notification handlers for resilience
7. **Don't rethrow exceptions** in handlers (breaks other subscribers)
8. **Enable statistics tracking** - `config.WithStatisticsTracking()`

### For Middleware:

1. **Implement `INotificationMiddleware`** for auto-discovery
2. **Set appropriate `Order`** values for execution sequence
3. **Always call `next()`** unless intentionally stopping the pipeline
4. **Handle exceptions appropriately** - log and decide whether to rethrow
5. **Keep middleware lightweight** - avoid heavy processing

### General:

1. **Use structured logging** with meaningful context
2. **Test notification handling** with unit tests
3. **Consider using cancellation tokens** for long-running operations
4. **Subscribe once per handler instance** to avoid duplicates
5. **Inspect middleware pipeline** during development for debugging
6. **Consider memory management** in long-running client applications

## 🔗 When to Use This Approach

### Use Manual Subscription When:

-   🎯 **Building client applications** - Blazor WASM, MAUI, WinForms, WPF
-   🎯 **AOT compilation required** - Ahead-of-time compilation scenarios
-   🎯 **Explicit control needed** - You need precise control over handler lifecycle
-   🎯 **Performance critical** - Avoiding runtime reflection overhead
-   🎯 **Resource-constrained environments** - Mobile or embedded applications
-   🎯 **Security requirements** - When reflection-based discovery is restricted

### Don't Use Manual Subscription When:

-   ❌ **Building server applications** - ASP.NET Core, Blazor Server (use automatic discovery instead)
-   ❌ **Reflection is acceptable** - When runtime reflection overhead is not a concern
-   ❌ **You want zero configuration** - Automatic discovery provides less setup code

### Client Application Examples:

```csharp
// Blazor WebAssembly
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        
        builder.Services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());
        
        builder.Services.AddScoped<OrderNotificationHandler>();
        
        var host = builder.Build();
        
        // Manual subscription required
        var mediator = host.Services.GetRequiredService<IMediator>();
        var handler = host.Services.GetRequiredService<OrderNotificationHandler>();
        mediator.Subscribe(handler);
        
        await host.RunAsync();
    }
}

// MAUI Application
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMediator(config =>
        {
            config.WithStatisticsTracking()
                  .WithNotificationMiddlewareDiscovery();
        }, Assembly.GetExecutingAssembly());
        
        builder.Services.AddScoped<NotificationManager>();
        builder.Services.AddScoped<OrderNotificationHandler>();

        return builder.Build();
    }
}
