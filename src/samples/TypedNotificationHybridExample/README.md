# Blazing.Mediator - Typed Hybrid Notification Pattern Example

This example demonstrates the **TYPED HYBRID notification pattern** in Blazing.Mediator, showcasing how to combine **automatic handler discovery** with **manual subscriber registration** while using **type-constrained middleware** for optimal performance and type safety. This represents the most advanced and flexible notification pattern available.

## ?? Overview

The typed hybrid pattern provides **maximum flexibility with optimal performance** by combining three powerful concepts:

- **Automatic Handlers** (`INotificationHandler<T>`) - Zero configuration, automatically discovered and registered
- **Manual Subscribers** (`INotificationSubscriber<T>`) - Explicit control, manually registered and subscribed  
- **Type-Constrained Middleware** - Selective processing based on notification type interfaces

This gives you **ultimate control**: zero-configuration for core logic, fine-grained control for complex scenarios, and performance-optimized middleware that only processes relevant notification types.

## ? Key Features Demonstrated

### Automatic Handler Discovery (Zero Configuration)
- **EmailNotificationHandler** - Handles multiple notification types automatically
- **BusinessOperationsHandler** - Processes all business notifications automatically
- **Multi-Type Support** - Single handler can process multiple notification types
- **No Manual Setup Required** - Just implement `INotificationHandler<T>` and it works

### Manual Subscriber Registration (Explicit Control)
- **AuditNotificationSubscriber** - Comprehensive audit logging with manual subscription
- **IntegrationNotificationSubscriber** - External system integration with explicit control
- **Dynamic Control** - Subscribe/unsubscribe at runtime as needed
- **Full Lifecycle Management** - You decide when, where, and how to activate

### Type-Constrained Middleware (Performance Optimized)
- **OrderNotificationMiddleware** - Only processes `IOrderNotification` types
- **CustomerNotificationMiddleware** - Only processes `ICustomerNotification` types
- **InventoryNotificationMiddleware** - Only processes `IInventoryNotification` types
- **GeneralNotificationMiddleware** - Processes all notification types
- **Performance Benefits** - Middleware only runs when notification types match

### Notification Categories with Type Safety
- **`IOrderNotification`** - Order-related events with compile-time safety
- **`ICustomerNotification`** - Customer-related events with type guarantees
- **`IInventoryNotification`** - Inventory-related events with constraint validation
- **Multiple Interface Support** - Single notifications can implement multiple interfaces

## ??? Type System Architecture

### Marker Interfaces (Type Constraints)
```csharp
// Define clear contracts for type-constrained processing
public interface IOrderNotification : INotification
{
    string OrderId { get; }
    string CustomerEmail { get; }
}

public interface ICustomerNotification : INotification
{
    string CustomerEmail { get; }
    string CustomerName { get; }
}

public interface IInventoryNotification : INotification
{
    string ProductId { get; }
    int Quantity { get; }
}
```

### Concrete Notifications
```csharp
// Notifications can implement multiple interfaces for flexible processing
public class OrderCreatedNotification : IOrderNotification, ICustomerNotification
{
    public string OrderId { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    // Will trigger both IOrderNotification and ICustomerNotification middleware
}

public class CustomerRegisteredNotification : ICustomerNotification
{
    public string CustomerEmail { get; set; }
    public string CustomerName { get; set; }
    // Will only trigger ICustomerNotification middleware
}
```

## ?? Typed Hybrid Pattern Comparison

| Feature | ?? Automatic Handlers | ?? Manual Subscribers | ?? Type-Constrained Middleware | ?? Best Use Case |
|---------|----------------------|----------------------|--------------------------------|------------------|
| **Registration** | ? Automatic Discovery | ? Manual Subscription | ? Automatic Discovery | Auto: Core / Manual: Optional / Type: Performance |
| **Type Safety** | ? Compile-time | ? Compile-time | ? Compile-time + Runtime | All: Maximum type safety |
| **Performance** | ? Zero overhead | ?? Subscription cost | ? Selective execution | Auto+Type: High perf / Manual: Flexibility |
| **Flexibility** | ?? Always active | ? Dynamic control | ?? Type-based | Manual: Max flexibility / Others: Structured |
| **Maintainability** | ? Zero config | ?? Explicit setup | ? Type-driven | Auto+Type: Low maintenance / Manual: Explicit |
| **Scalability** | ? Easy to add | ?? Must remember to subscribe | ? Automatic categorization | All: Excellent scalability |

## ?? What This Example Demonstrates

### Automatic Handlers (Core Business Logic)
1. **EmailNotificationHandler** ??
   - **Multi-Type Handler**: Processes `OrderCreatedNotification` and `CustomerRegisteredNotification`
   - **Automatic Discovery**: Zero configuration required
   - **Type-Safe Processing**: Handles different notification types appropriately
   - **Always Active**: Core communication that should never be disabled

2. **BusinessOperationsHandler** ??
   - **Universal Handler**: Processes all business notification types
   - **Business Logic Hub**: Centralized business rule processing
   - **Zero Setup**: Automatically discovered and registered
   - **Consistent Processing**: Ensures business rules always apply

### Manual Subscribers (Advanced Control)
1. **AuditNotificationSubscriber** ??
   - **Comprehensive Auditing**: Handles all notification types when subscribed
   - **Conditional Activation**: Only active when compliance is required
   - **External Integration**: Complex setup with audit systems
   - **Manual Control**: Subscribe/unsubscribe based on regulatory needs

2. **IntegrationNotificationSubscriber** ??
   - **External System Integration**: Handles order and customer notifications
   - **Complex Setup**: Requires configuration and connection management
   - **Dynamic Activation**: Enable/disable based on system availability
   - **Explicit Control**: Full lifecycle management for integration scenarios

### Type-Constrained Middleware (Performance Optimized)
1. **OrderNotificationMiddleware** [200] ???
   - **Type Constraint**: `TNotification : IOrderNotification`
   - **Order-Specific Logic**: Validation, processing, and business rules
   - **Performance**: Only executes for order-related notifications
   - **Type Safety**: Compile-time guarantee of order notification properties

2. **CustomerNotificationMiddleware** [250] ??
   - **Type Constraint**: `TNotification : ICustomerNotification`
   - **Customer-Specific Logic**: Email validation, customer processing
   - **Selective Execution**: Only runs for customer-related events
   - **Type Guarantee**: Compile-time access to customer properties

3. **InventoryNotificationMiddleware** [300] ??
   - **Type Constraint**: `TNotification : IInventoryNotification`
   - **Inventory-Specific Logic**: Stock validation, quantity checks
   - **Optimized Processing**: Only executes for inventory notifications
   - **Type Safety**: Guaranteed access to inventory properties

4. **GeneralNotificationMiddleware** [100, 400] ??
   - **No Type Constraint**: Processes all notifications
   - **Cross-Cutting Concerns**: Logging, metrics, audit trails
   - **Universal Pipeline**: Always executes regardless of notification type
   - **Comprehensive Coverage**: Handles general processing for all types

## ?? Middleware Pipeline Execution

### Pipeline Flow by Notification Type

**OrderCreatedNotification** (implements `IOrderNotification`, `ICustomerNotification`):
```
GeneralNotificationMiddleware [100] (logging)
    ?
OrderNotificationMiddleware [200] (order validation)
    ?
CustomerNotificationMiddleware [250] (customer validation)
    ?
GeneralNotificationMiddleware [400] (metrics)
    ?
Handlers: EmailNotificationHandler + BusinessOperationsHandler (automatic)
Subscribers: AuditNotificationSubscriber + IntegrationNotificationSubscriber (if subscribed)
```

**CustomerRegisteredNotification** (implements `ICustomerNotification`):
```
GeneralNotificationMiddleware [100] (logging)
    ?
CustomerNotificationMiddleware [250] (customer validation)
    ?
GeneralNotificationMiddleware [400] (metrics)
    ?
Handlers: EmailNotificationHandler + BusinessOperationsHandler (automatic)
Subscribers: AuditNotificationSubscriber (if subscribed)
```

**InventoryUpdatedNotification** (implements `IInventoryNotification`):
```
GeneralNotificationMiddleware [100] (logging)
    ?
InventoryNotificationMiddleware [300] (inventory validation)
    ?
GeneralNotificationMiddleware [400] (metrics)
    ?
Handlers: BusinessOperationsHandler (automatic)
Subscribers: AuditNotificationSubscriber (if subscribed)
```

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
          .WithNotificationMiddlewareDiscovery(); // Enable automatic middleware discovery (including type-constrained)
}, Assembly.GetExecutingAssembly());

// Manual subscribers must be registered in DI container
services.AddScoped<AuditNotificationSubscriber>();
services.AddScoped<IntegrationNotificationSubscriber>();
```

### Type-Constrained Middleware Implementation
```csharp
// Middleware that only processes order notifications
public class OrderNotificationMiddleware : INotificationMiddleware
{
    public int Order => 200;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // This middleware only executes if TNotification implements IOrderNotification
        if (notification is IOrderNotification orderNotification)
        {
            // Type-safe access to order properties
            await ValidateOrderAsync(orderNotification);
            await LogOrderProcessingAsync(orderNotification);
        }

        await next(notification, cancellationToken);
    }
}
```

### Multi-Type Handler Implementation
```csharp
// Handler that processes multiple notification types automatically
public class EmailNotificationHandler : 
    INotificationHandler<OrderCreatedNotification>,
    INotificationHandler<CustomerRegisteredNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Order-specific email logic
        await SendOrderConfirmationEmailAsync(notification);
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        // Customer-specific email logic
        await SendWelcomeEmailAsync(notification);
    }
}
```

## ?? Running the Example

### Prerequisites
- .NET 9.0 or later
- Visual Studio 2022 or VS Code

### Running
```bash
cd src/samples/TypedNotificationHybridExample
dotnet run
```

### Expected Output
```
================================================================================
*** Blazing.Mediator - TYPED HYBRID Notification Pattern Example ***
================================================================================

This example demonstrates the TYPED HYBRID notification pattern combining:

AUTOMATIC HANDLERS (Zero Configuration):
  [AUTO] EmailNotificationHandler - Handles OrderCreated + CustomerRegistered
  [AUTO] BusinessOperationsHandler - Handles all notification types automatically
  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>

MANUAL SUBSCRIBERS (Explicit Control):
  [MANUAL] AuditNotificationSubscriber - Handles all types (requires subscription)
  [MANUAL] IntegrationNotificationSubscriber - Handles OrderCreated + CustomerRegistered
  [CONTROL] EXPLICIT SUBSCRIPTION - Full control over when and how to subscribe

TYPE-CONSTRAINED MIDDLEWARE (Selective Processing):
  [TYPE] OrderNotificationMiddleware - IOrderNotification only
  [TYPE] CustomerNotificationMiddleware - ICustomerNotification only
  [TYPE] InventoryNotificationMiddleware - IInventoryNotification only
  [ALL] GeneralNotificationMiddleware - All notification types

================================================================
>> Processing OrderCreatedNotification (IOrderNotification + ICustomerNotification)
================================================================

[12:34:56.789] [GENERAL] Processing notification: OrderCreatedNotification
[12:34:56.790] [ORDER] Order validation started for Order #ORD-001
[12:34:56.791] [ORDER] Order validation passed: $299.99, 3 items
[12:34:56.792] [CUSTOMER] Customer validation started for alice@example.com
[12:34:56.793] [CUSTOMER] Customer validation passed: Alice Johnson
[12:34:56.794] [EMAIL] Order confirmation email sent (Automatic Handler)
[12:34:56.795] [BUSINESS] Business rules processed (Automatic Handler)
[12:34:56.796] [AUDIT] Comprehensive audit logged (Manual Subscriber)
[12:34:56.797] [INTEGRATION] External system notified (Manual Subscriber)
[12:34:56.798] [GENERAL] Metrics recorded: 9ms

================================================================
>> Processing CustomerRegisteredNotification (ICustomerNotification only)
================================================================

[12:34:57.123] [GENERAL] Processing notification: CustomerRegisteredNotification
[12:34:57.124] [CUSTOMER] Customer validation started for bob@example.com
[12:34:57.125] [CUSTOMER] New customer validation passed: Bob Smith
[12:34:57.126] [EMAIL] Welcome email sent (Automatic Handler)
[12:34:57.127] [BUSINESS] Customer onboarding processed (Automatic Handler)
[12:34:57.128] [AUDIT] Customer registration audited (Manual Subscriber)
[12:34:57.129] [GENERAL] Metrics recorded: 6ms
(Note: Order and Inventory middleware did not execute - performance optimized!)

*** TYPED HYBRID PATTERN DEMONSTRATION COMPLETE! ***

What you just experienced:
  [OK] Automatic handlers executed with zero configuration
  [OK] Type-constrained middleware executed selectively by notification category
  [OK] Manual subscribers executed with explicit control
  [OK] Unified pipeline processing multiple notification types efficiently
  [OK] Maximum flexibility with optimal performance
```

## ?? Advanced Use Case Decision Guide

### Use **Automatic Handlers** When:
- ? **Core business logic** that should always execute
- ? **Multi-type processing** with single handler class
- ? **Zero configuration overhead** is desired
- ? **Consistent behavior** across notification types

**Examples:**
- Email notifications for multiple events
- Core business rule validation
- Universal logging and metrics
- Cross-cutting concerns

### Use **Manual Subscribers** When:
- ? **Optional or conditional processing** based on configuration
- ? **Complex external integrations** requiring setup
- ? **Dynamic subscription control** is needed
- ? **Compliance or regulatory** features that may be toggled

**Examples:**
- Optional audit logging
- External system integrations
- Regulatory compliance features
- A/B testing scenarios

### Use **Type-Constrained Middleware** When:
- ? **Category-specific processing** is needed
- ? **Performance optimization** is important
- ? **Type safety** with compile-time guarantees
- ? **Different logic** for different notification categories

**Examples:**
- Order-specific validation
- Customer-specific processing
- Inventory-specific business rules
- Category-specific logging/metrics

## ?? Adding Your Own Components

### Adding a New Notification Category
```csharp
// Define the marker interface
public interface IPaymentNotification : INotification
{
    string PaymentId { get; }
    decimal Amount { get; }
}

// Create concrete notification
public class PaymentProcessedNotification : IPaymentNotification, IOrderNotification
{
    public string PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string OrderId { get; set; }
    public string CustomerEmail { get; set; }
    // Will trigger IPaymentNotification and IOrderNotification middleware
}

// Add type-constrained middleware
public class PaymentNotificationMiddleware : INotificationMiddleware
{
    public int Order => 350;

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        if (notification is IPaymentNotification paymentNotification)
        {
            await ValidatePaymentAsync(paymentNotification);
        }

        await next(notification, cancellationToken);
    }
}
```

### Adding a Multi-Type Automatic Handler
```csharp
public class SecurityNotificationHandler : 
    INotificationHandler<OrderCreatedNotification>,
    INotificationHandler<CustomerRegisteredNotification>,
    INotificationHandler<PaymentProcessedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await PerformSecurityChecksAsync("order", notification.OrderId);
    }

    public async Task Handle(CustomerRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await PerformSecurityChecksAsync("customer", notification.CustomerEmail);
    }

    public async Task Handle(PaymentProcessedNotification notification, CancellationToken cancellationToken = default)
    {
        await PerformSecurityChecksAsync("payment", notification.PaymentId);
    }
}
// Automatically discovered - no additional registration needed!
```

## ?? Performance Analysis

### Type-Constrained Middleware Benefits
```
Traditional Approach (All middleware runs for all notifications):
OrderCreatedNotification: 5 middleware × 100ms = 500ms
CustomerRegisteredNotification: 5 middleware × 100ms = 500ms
InventoryUpdatedNotification: 5 middleware × 100ms = 500ms
Total: 1,500ms

Type-Constrained Approach (Only relevant middleware runs):
OrderCreatedNotification: 3 relevant middleware × 100ms = 300ms
CustomerRegisteredNotification: 2 relevant middleware × 100ms = 200ms  
InventoryUpdatedNotification: 2 relevant middleware × 100ms = 200ms
Total: 700ms (53% improvement!)
```

### Memory and CPU Benefits
- ? **Reduced CPU Usage** - Middleware only executes when relevant
- ? **Lower Memory Allocation** - No unnecessary object creation
- ? **Better Cache Performance** - More predictable execution patterns
- ? **Improved Scalability** - Performance scales with notification diversity

## ?? Related Examples

- **`NotificationHybridExample`** - Basic hybrid pattern without type constraints
- **`TypedNotificationHandlerExample`** - Automatic handlers with type constraints
- **`TypedNotificationSubscriberExample`** - Manual subscribers with type constraints
- **`NotificationHandlerExample`** - Pure automatic handler discovery
- **`NotificationSubscriberExample`** - Pure manual subscription pattern

## ?? Learning Outcomes

This advanced example teaches:

1. **Advanced Architecture Design** - Combining multiple patterns effectively
2. **Type System Utilization** - Leveraging interfaces for compile-time safety
3. **Performance Optimization** - Selective middleware execution strategies
4. **Pattern Selection Mastery** - When and how to use each approach
5. **Scalability Planning** - Designing systems that scale with complexity
6. **Type Safety Implementation** - Compile-time guarantees with runtime efficiency
7. **Real-World Application** - Production-ready patterns for complex systems

## ?? Advanced Best Practices

1. **Design Clear Type Hierarchies** - Use marker interfaces for logical grouping
2. **Optimize Middleware Execution** - Leverage type constraints for performance
3. **Balance Flexibility and Performance** - Choose the right pattern for each scenario
4. **Implement Comprehensive Monitoring** - Track all patterns with statistics
5. **Plan for Evolution** - Design extensible notification categories
6. **Maintain Type Safety** - Use compile-time checking wherever possible
7. **Document Decision Rationale** - Explain when and why to use each pattern

## ?? Production Considerations

### Scaling Strategy
- **Start with automatic handlers** for core functionality
- **Add type-constrained middleware** for performance-critical paths
- **Use manual subscribers** for optional or feature-flagged functionality
- **Monitor performance** with comprehensive statistics
- **Evolve incrementally** as requirements change

### Monitoring and Observability
- **Track execution times** by notification type and pattern
- **Monitor middleware performance** across different categories
- **Analyze subscription patterns** for manual subscribers
- **Set up alerts** for performance degradation
- **Use detailed statistics** for optimization decisions

---

This typed hybrid example represents the pinnacle of notification pattern flexibility in Blazing.Mediator, providing maximum performance, type safety, and architectural flexibility for production applications of any complexity.