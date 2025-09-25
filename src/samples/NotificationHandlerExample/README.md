# Blazing.Mediator - Notification Handler Example

This example demonstrates the **NEW automatic notification handler pattern** in Blazing.Mediator, showcasing automatic discovery and execution of notification handlers without manual subscription.

## ?? Key Features

### Automatic Handler Discovery
- **Zero Configuration**: Handlers are automatically discovered and registered
- **No Manual Subscription**: Just implement `INotificationHandler<T>` - that's it!
- **Compile-time Registration**: Better performance and reliability than runtime subscription

### Multiple Handler Pattern
- **Scalable Architecture**: Multiple handlers can process the same notification independently
- **Easy to Extend**: Add new handlers by simply implementing the interface
- **Isolated Error Handling**: Each handler's errors don't affect others

### Complete Middleware Pipeline
- **Automatic Middleware Discovery**: Middleware is found and ordered automatically
- **Validation**: Input validation before handler execution
- **Logging**: Comprehensive pipeline logging
- **Metrics**: Performance tracking and statistics collection

## ?? Handler vs Subscriber Pattern Comparison

| Feature | ?? Notification Handlers (NEW) | ?? Notification Subscribers (Legacy) |
|---------|--------------------------------|--------------------------------------|
| **Registration** | ? Automatic Discovery | ? Manual Subscription Required |
| **Setup Code** | ? Zero - Just implement interface | ? Must call `mediator.Subscribe()` |
| **Performance** | ? Compile-time registration | ?? Runtime subscription overhead |
| **Maintainability** | ? Simple - implement interface | ?? Remember to subscribe each handler |
| **Reliability** | ? Compile-time validation | ?? Runtime subscription management |
| **Scalability** | ? Easy to add new handlers | ?? Must remember configuration |

## ?? What This Example Demonstrates

### Automatic Handler Discovery
Four handlers are automatically discovered and registered:

1. **EmailNotificationHandler** ??
   - Sends order confirmation emails
   - Demonstrates basic handler implementation

2. **InventoryNotificationHandler** ??
   - Updates inventory levels
   - Performs stock level checks and alerts
   - Shows business logic integration

3. **AuditNotificationHandler** ??
   - Logs orders for compliance and auditing
   - Performs compliance checks
   - Demonstrates audit trail creation

4. **ShippingNotificationHandler** ??
   - Handles shipping and fulfillment
   - Calculates delivery estimates
   - Creates shipping labels and tracking numbers

### Middleware Pipeline
Three middleware components are automatically discovered and executed in order:

1. **NotificationLoggingMiddleware** (Order: 100) ??
   - Logs pipeline start/completion
   - Tracks execution duration
   - Handles error logging

2. **NotificationValidationMiddleware** (Order: 200) ?
   - Validates notification data
   - Prevents invalid notifications from processing
   - Shows comprehensive validation logic

3. **NotificationMetricsMiddleware** (Order: 300) ??
   - Collects performance metrics
   - Tracks success/failure rates
   - Provides execution statistics

## ?? How It Works

### 1. Handler Implementation
```csharp
// Just implement the interface - no other setup required!
public class EmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        // Your handler logic here
        await SendEmailAsync(notification, cancellationToken);
    }
}
```

### 2. Automatic Registration
```csharp
// Enable automatic handler discovery
services.AddMediator(config =>
{
    config.WithNotificationHandlerDiscovery()           // Enable handler discovery
          .WithNotificationMiddlewareDiscovery()        // Enable middleware discovery
          .WithStatisticsTracking();                    // Enable metrics collection
}, Assembly.GetExecutingAssembly());
```

### 3. Publishing Notifications
```csharp
// Publish once - all handlers execute automatically
var notification = new OrderCreatedNotification { /* ... */ };
await mediator.Publish(notification);

// All 4 handlers will execute automatically:
// 1. EmailNotificationHandler
// 2. InventoryNotificationHandler  
// 3. AuditNotificationHandler
// 4. ShippingNotificationHandler
```

## ?? Running the Example

### Prerequisites
- .NET 9.0 or later
- Visual Studio 2022 or VS Code

### Running
```bash
cd src/samples/NotificationHandlerExample
dotnet run
```

### Expected Output
You'll see:
1. **Discovery Status** - Confirmation that handlers were automatically found
2. **Sample Orders** - Three orders with different characteristics being processed
3. **Middleware Pipeline** - Logging, validation, and metrics for each notification
4. **All Handlers** - Each handler processing every notification automatically
5. **Validation Demo** - Example of validation failure and error handling
6. **Performance Metrics** - Statistics and performance data collection

## ?? Sample Output
```
==============================================================================
*** Blazing.Mediator - Notification Handler Example (Automatic Discovery) ***
==============================================================================

This example demonstrates the NEW automatic notification handler pattern:

KEY FEATURES:
  [AUTO] AUTOMATIC DISCOVERY - Handlers are discovered and registered automatically
  [EASY] NO MANUAL SUBSCRIPTION - Just implement INotificationHandler<T>
  [MULTI] MULTIPLE HANDLERS - Multiple handlers process the same notification
  [PIPE] MIDDLEWARE PIPELINE - Validation, logging, metrics automatically applied
  [SCALE] SCALABLE ARCHITECTURE - Easy to add new handlers without configuration

AUTOMATIC HANDLER DISCOVERY:
  * EmailNotificationHandler - Sends confirmation emails
  * InventoryNotificationHandler - Updates inventory and stock alerts
  * AuditNotificationHandler - Logs for compliance and auditing
  * ShippingNotificationHandler - Handles shipping and fulfillment

AUTOMATIC MIDDLEWARE DISCOVERY:
  [100] NotificationLoggingMiddleware (Order: 100)
  [200] NotificationValidationMiddleware (Order: 200)
  [300] NotificationMetricsMiddleware (Order: 300)

COMPARED TO NOTIFICATION SUBSCRIBERS:
  [-] Subscribers: Require manual subscription with mediator.Subscribe()
  [+] Handlers: Automatically discovered and invoked - no manual setup!

>> DISCOVERY STATUS:
  >> Handler discovery and registration completed automatically
  >> All handlers registered and ready for automatic invocation

================================================================
>> Publishing Order: ORD-001 ($199.97)
================================================================
[11:30:15.123] [START] Notification Pipeline Started: OrderCreatedNotification
[11:30:15.125] [VALIDATE] Validating notification: OrderCreatedNotification
[11:30:15.127] [+] Notification validation passed
[11:30:15.128] [METRICS] Collecting metrics for: OrderCreatedNotification

[11:30:15.130] [EMAIL] ORDER CONFIRMATION EMAIL SENT
[11:30:15.135] [INVENTORY] INVENTORY UPDATE PROCESSING
[11:30:15.140] [AUDIT] AUDIT LOG: Order Created
[11:30:15.145] [SHIPPING] SHIPPING PROCESSING STARTED

[11:30:15.250] [+] Metrics recorded: OrderCreatedNotification succeeded in 125ms
[11:30:15.251] [OK] Notification Pipeline Completed: OrderCreatedNotification in 128ms

*** DEMONSTRATING VALIDATION FAILURE ***
[11:30:16.123] [START] Notification Pipeline Started: OrderCreatedNotification
[11:30:16.125] [VALIDATE] Validating notification: OrderCreatedNotification
[11:30:16.127] [-] Notification validation failed: CustomerEmail format is invalid, TotalAmount must be greater than zero, Order must contain at least one item
[11:30:16.128] [ERROR] Notification Pipeline Failed: OrderCreatedNotification after 1.46ms
[+] Validation correctly failed: Notification validation failed

*** Demo completed! All handlers were automatically discovered and executed. ***
````````
### Adding Custom Middleware
1. Create a class implementing `INotificationMiddleware`
2. Set the `Order` property to control execution sequence
3. Implement the `InvokeAsync` method with your middleware logic
4. It will be automatically discovered and added to the pipeline

Example:
```csharp
public class CustomMiddleware : INotificationMiddleware
{
    public int Order => 250; // Execute between validation (200) and metrics (300)
    
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Your middleware logic here
        await next(notification, cancellationToken);
    }
}