# Blazing.Mediator - Hybrid Notification Pattern Example

This example demonstrates the **HYBRID notification pattern** in Blazing.Mediator, showcasing how to combine **automatic handler discovery** with **manual subscriber registration** for maximum flexibility. This approach allows you to use the best pattern for each specific scenario in your application.

## ?? Overview

The hybrid pattern provides **maximum flexibility** by combining two notification approaches:

- **Automatic Handlers** (`INotificationHandler<T>`) - Zero configuration, automatically discovered and registered
- **Manual Subscribers** (`INotificationSubscriber<T>`) - Explicit control, manually registered and subscribed

This gives you the **best of both worlds**: zero-configuration simplicity for core logic and fine-grained control for complex scenarios.

## ? Key Features Demonstrated

### Automatic Handler Discovery (Zero Configuration)
- **EmailNotificationHandler** - Automatically discovered and registered
- **ShippingNotificationHandler** - Automatically discovered and registered
- **No Manual Setup Required** - Just implement `INotificationHandler<T>` and it works

### Manual Subscriber Registration (Explicit Control)
- **InventoryNotificationSubscriber** - Requires manual subscription with `mediator.Subscribe()`
- **AuditNotificationSubscriber** - Requires manual subscription with `mediator.Subscribe()`
- **Full Control** - You decide when, where, and how to subscribe

### Unified Middleware Pipeline
- **NotificationLoggingMiddleware** (Order: 100) - Logs all notification processing
- **NotificationMetricsMiddleware** (Order: 300) - Collects performance metrics
- **Shared Pipeline** - Both handlers and subscribers use the same middleware

### Comprehensive Statistics
- **MediatorStatistics** - Real-time performance tracking for both patterns
- **Pipeline Analysis** - Inspection tools for troubleshooting
- **Mixed Pattern Support** - Statistics for handlers, subscribers, and middleware

## ?? Hybrid Pattern Comparison

| Feature | ?? Automatic Handlers | ?? Manual Subscribers | ?? When to Use |
|---------|----------------------|----------------------|----------------|
| **Registration** | ? Automatic Discovery | ? Manual Subscription Required | Auto: Core logic / Manual: Optional features |
| **Setup Code** | ? Zero - Just implement interface | ? Must call `mediator.Subscribe()` | Auto: Always-on / Manual: Conditional |
| **Performance** | ? Compile-time registration | ?? Runtime subscription overhead | Auto: High performance / Manual: Complex scenarios |
| **Control** | ?? Always active | ? Dynamic subscription/unsubscription | Auto: Fixed behavior / Manual: Dynamic behavior |
| **Maintainability** | ? Simple - implement interface | ?? Remember to subscribe | Auto: Set and forget / Manual: Explicit management |
| **Flexibility** | ?? Limited runtime control | ? Full lifecycle control | Auto: Consistency / Manual: Flexibility |

## ??? Architecture Overview

### Automatic Handlers (Core Business Logic)
```csharp
// Just implement INotificationHandler<T> - automatically discovered and registered
public class EmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Always executes - zero configuration required
        await SendEmailAsync(notification);
    }
}
```

### Manual Subscribers (Optional/Conditional Logic)
```csharp
// Implement INotificationSubscriber<T> - requires manual subscription
public class InventoryNotificationSubscriber : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Only executes when manually subscribed
        await UpdateInventoryAsync(notification);
    }
}

// Manual subscription required
mediator.Subscribe(inventorySubscriber);
```

## ?? What This Example Demonstrates

### Automatic Handlers (Always Active)
1. **EmailNotificationHandler** ??
   - Sends order confirmation emails
   - Always executes for every order
   - Zero configuration - just implement the interface
   - Core business functionality that should never be disabled

2. **ShippingNotificationHandler** ??
   - Handles shipping and fulfillment processing
   - Creates shipping labels and tracking numbers
   - Automatically processes every order
   - Essential business logic with zero setup

### Manual Subscribers (Explicit Control)
1. **InventoryNotificationSubscriber** ??
   - Updates inventory levels and stock counts
   - Only active when explicitly subscribed
   - Can be dynamically enabled/disabled
   - Optional feature that may not be needed in all scenarios

2. **AuditNotificationSubscriber** ??
   - Logs detailed audit trails for compliance
   - Requires explicit subscription for activation
   - Can be conditionally enabled based on configuration
   - Complex setup with external system integration

### Unified Middleware Pipeline
1. **NotificationLoggingMiddleware** (Order: 100) ??
   - Logs start/completion for all notifications
   - Works with both handlers and subscribers
   - Provides unified logging across patterns

2. **NotificationMetricsMiddleware** (Order: 300) ??
   - Collects performance metrics for all notifications
   - Tracks both automatic and manual processors
   - Enables comprehensive performance analysis

## ?? Configuration and Setup

### Service Registration
```csharp
services.AddMediator(config =>
{
    config.WithStatisticsTracking(options =>
        {
            options.EnableRequestMetrics = true;
            options.EnableNotificationMetrics = true;
            options.EnableMiddlewareMetrics = true;
            options.EnablePerformanceCounters = true;
            options.EnableDetailedAnalysis = true;
        })
          .WithNotificationHandlerDiscovery()    // Enable automatic handler discovery
          .WithNotificationMiddlewareDiscovery(); // Enable automatic middleware discovery
}, Assembly.GetExecutingAssembly());

// Manual subscribers must be registered in DI container
services.AddScoped<InventoryNotificationSubscriber>();
services.AddScoped<AuditNotificationSubscriber>();
```

### Runtime Subscription
```csharp
// Create scope and get services
using var scope = host.Services.CreateScope();
var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

// Manual subscription required for subscribers
var inventorySubscriber = scope.ServiceProvider.GetRequiredService<InventoryNotificationSubscriber>();
var auditSubscriber = scope.ServiceProvider.GetRequiredService<AuditNotificationSubscriber>();

mediator.Subscribe(inventorySubscriber);
mediator.Subscribe(auditSubscriber);

// Automatic handlers require no subscription - they're already active!
```

## ?? Running the Example

### Prerequisites
- .NET 9.0 or later
- Visual Studio 2022 or VS Code

### Running
```bash
cd src/samples/NotificationHybridExample
dotnet run
```

### Expected Output
```
================================================================================
*** Blazing.Mediator - HYBRID Notification Pattern Example ***
================================================================================

This example demonstrates the HYBRID notification pattern combining:

AUTOMATIC HANDLERS (Zero Configuration):
  [AUTO] EmailNotificationHandler - Discovered and registered automatically
  [AUTO] ShippingNotificationHandler - Discovered and registered automatically
  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>

MANUAL SUBSCRIBERS (Explicit Control):
  [MANUAL] InventoryNotificationSubscriber - Requires manual subscription
  [MANUAL] AuditNotificationSubscriber - Requires manual subscription
  [CONTROL] EXPLICIT SUBSCRIPTION - Full control over when and how to subscribe

HYBRID BENEFITS:
  [FLEX] MAXIMUM FLEXIBILITY - Use the right pattern for each scenario
  [PERF] OPTIMAL PERFORMANCE - Automatic handlers have zero overhead
  [CTRL] FINE-GRAINED CONTROL - Manual subscribers for complex scenarios
  [SCALE] EASY SCALING - Mix and match as your application grows

>> HYBRID CONFIGURATION STATUS:
  >> Automatic handler discovery enabled - handlers will be discovered automatically
  >> Manual subscriber services registered - ready for explicit subscription
  >> MediatorStatistics enabled for comprehensive analysis of both patterns
  >> Middleware pipeline configured for unified processing

================================================================
>> Processing Order: ORD-001 ($299.99) for Alice Johnson
================================================================

[12:34:56.789] [START] Notification Pipeline Started: OrderCreatedNotification
[12:34:56.790] [EMAIL] ORDER CONFIRMATION EMAIL SENT (Automatic Handler)
[12:34:56.792] [SHIPPING] SHIPPING PROCESSING STARTED (Automatic Handler)
[12:34:56.795] [INVENTORY] INVENTORY UPDATE PROCESSING (Manual Subscriber)
[12:34:56.798] [AUDIT] DETAILED AUDIT LOG CREATED (Manual Subscriber)
[12:34:56.801] [METRICS] Pipeline completed in 12ms

*** HYBRID PATTERN DEMONSTRATION COMPLETE! ***

What you just experienced:
  [OK] Automatic handlers executed with zero configuration
  [OK] Manual subscribers executed with explicit control
  [OK] Unified middleware pipeline processing both patterns
  [OK] Comprehensive statistics tracking for both approaches
  [OK] Maximum flexibility - use the right tool for each job
```

## ?? Use Case Decision Guide

### Use **Automatic Handlers** When:
- ? **Core business logic** that should always execute
- ? **Simple, stateless processing** without complex setup
- ? **Zero configuration overhead** is desired
- ? **Tightly coupled** to the notification type
- ? **Always-on services** like email, logging, metrics

**Examples:**
- Order confirmation emails
- Basic logging and metrics
- Core business rule validation
- Standard workflow processing

### Use **Manual Subscribers** When:
- ? **Optional or conditional processing** based on configuration
- ? **Complex initialization** or external system integration
- ? **Dynamic subscription/unsubscription** is needed
- ? **Legacy system integration** requires specific setup
- ? **Feature flags** control functionality

**Examples:**
- Optional inventory management
- Conditional audit logging
- Integration with external systems
- Feature-flagged functionality
- A/B testing scenarios

## ?? Adding Your Own Components

### Adding an Automatic Handler
```csharp
// Just implement INotificationHandler<T> - it will be discovered automatically
public class PaymentNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Process payment automatically for every order
        await ProcessPaymentAsync(notification);
    }
}
// No additional registration needed - it's automatic!
```

### Adding a Manual Subscriber
```csharp
// Implement INotificationSubscriber<T> - requires manual setup
public class LoyaltyNotificationSubscriber : INotificationSubscriber<OrderCreatedNotification>
{
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Update loyalty points only when subscribed
        await UpdateLoyaltyPointsAsync(notification);
    }
}

// Register in DI container
services.AddScoped<LoyaltyNotificationSubscriber>();

// Manual subscription required
var loyaltySubscriber = scope.ServiceProvider.GetRequiredService<LoyaltyNotificationSubscriber>();
mediator.Subscribe(loyaltySubscriber);
```

### Adding Middleware
```csharp
public class SecurityNotificationMiddleware : INotificationMiddleware
{
    public int Order => 50; // Execute early in pipeline
    
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Security checks for both handlers and subscribers
        await ValidateSecurityAsync(notification);
        await next(notification, cancellationToken);
    }
}
// Automatically discovered and applied to both patterns!
```

## ?? Performance Considerations

### Automatic Handlers
- ? **Compile-time registration** - No runtime overhead
- ? **Direct invocation** - Minimal indirection
- ? **Memory efficient** - No dynamic subscriptions
- ?? **Always active** - Cannot be disabled at runtime

### Manual Subscribers
- ?? **Runtime subscription** - Small overhead during subscription
- ? **Dynamic control** - Can be enabled/disabled
- ? **Flexible lifecycle** - Full control over activation
- ?? **Memory management** - Must manage subscriptions properly

### Best Practices
1. **Use automatic handlers for core logic** - Maximum performance
2. **Use manual subscribers for optional features** - Maximum flexibility
3. **Share middleware pipeline** - Unified processing for both patterns
4. **Monitor with statistics** - Track performance of both approaches
5. **Consider memory management** - Dispose manual subscriptions when appropriate

## ?? Related Examples

- **`NotificationHandlerExample`** - Pure automatic handler discovery pattern
- **`NotificationSubscriberExample`** - Pure manual subscription pattern
- **`TypedNotificationHybridExample`** - Hybrid pattern with type-constrained middleware
- **`TypedNotificationHandlerExample`** - Automatic handlers with type constraints
- **`TypedNotificationSubscriberExample`** - Manual subscribers with type constraints

## ?? Learning Outcomes

This example teaches:

1. **Hybrid Architecture Design** - Combining different patterns effectively
2. **Pattern Selection Criteria** - When to use automatic vs manual approaches
3. **Unified Processing Pipeline** - How middleware works with both patterns
4. **Performance Trade-offs** - Understanding the costs and benefits
5. **Flexibility vs Simplicity** - Balancing ease of use with control
6. **Real-world Application** - Practical scenarios for each pattern
7. **Statistics and Monitoring** - Tracking performance across patterns

## ?? Best Practices Demonstrated

1. **Use the Right Tool** - Automatic for core logic, manual for optional features
2. **Unified Middleware** - Single pipeline for both patterns
3. **Comprehensive Statistics** - Monitor performance across all patterns
4. **Clear Separation** - Distinct use cases for each approach
5. **Resource Management** - Proper cleanup for manual subscriptions
6. **Documentation** - Clear guidelines for when to use each pattern
7. **Maintainability** - Balance between simplicity and flexibility

---

This hybrid example showcases the ultimate flexibility of Blazing.Mediator, allowing you to choose the perfect notification pattern for each specific scenario while maintaining a unified processing pipeline and comprehensive monitoring across all approaches.