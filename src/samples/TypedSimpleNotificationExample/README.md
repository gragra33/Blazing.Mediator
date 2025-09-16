# TypedSimpleNotificationExample - Blazing.Mediator Type-Constrained Notifications Demo

A demonstration project showcasing **type-constrained notification middleware** in Blazing.Mediator, highlighting how different notification middleware can be selectively applied based on notification interface types.

## ?? Overview

This example demonstrates how notification middleware can be selectively applied based on notification types using generic type constraints, specifically showing how different middleware components process only specific categories of notifications (Order, Customer, Inventory).

## ?? Key Features Demonstrated

### ??? Notification Type Interfaces
- **IOrderNotification**: Order-related events (OrderCreated, OrderStatusChanged)
- **ICustomerNotification**: Customer-related events (CustomerRegistered)
- **IInventoryNotification**: Inventory-related events (InventoryUpdated)
- **Type Safety**: Compile-time enforcement of notification categorization

### ?? Type-Constrained Notification Middleware
- **OrderNotificationMiddleware**: Constrained to `IOrderNotification` only
- **CustomerNotificationMiddleware**: Constrained to `ICustomerNotification` only
- **InventoryNotificationMiddleware**: Constrained to `IInventoryNotification` only
- **Selective Processing**: Middleware automatically applies only to appropriate notification types

### ?? Clear Distinction Logging
- **Order Processing**: Shows "?? Processing ORDER notification" for order events
- **Customer Processing**: Shows "?? Processing CUSTOMER notification" for customer events
- **Inventory Processing**: Shows "?? Processing INVENTORY notification" for inventory events
- **Visual Differentiation**: Easy identification of different notification types in logs

## ??? Project Structure

```
TypedSimpleNotificationExample/
??? Notifications/           # Notification interfaces and implementations
?   ??? NotificationInterfaces.cs     # IOrderNotification, ICustomerNotification, IInventoryNotification
?   ??? BusinessNotifications.cs      # Concrete notification implementations
??? Middleware/             # Type-constrained notification middleware
?   ??? TypedNotificationMiddleware.cs # All notification middleware implementations
??? Subscribers/            # Notification subscribers
?   ??? NotificationSubscribers.cs    # Email, Inventory, Business, Audit handlers
??? Services/               # Application services
?   ??? Runner.cs                     # Demo orchestration service
?   ??? NotificationPipelineAnalyzer.cs # Pipeline analysis helper
?   ??? NotificationPipelineDisplayer.cs # Dynamic pipeline display
??? GlobalUsings.cs         # Global using statements
??? Program.cs              # Application entry point
??? README.md               # This file
```

## ?? Middleware Analysis

This example demonstrates TYPE-CONSTRAINED notification middleware:

```
NOTIFICATION TYPES:
  - ICustomerNotification
    - CustomerRegisteredNotification

  - IInventoryNotification
    - InventoryUpdatedNotification

  - IOrderNotification
    - OrderCreatedNotification
    - OrderStatusChangedNotification


TYPE-CONSTRAINED MIDDLEWARE:
  - [-2147483648] NotificationErrorHandlingMiddleware
  - [10] GeneralNotificationLoggingMiddleware
  - [50] OrderNotificationMiddleware
  - [60] CustomerNotificationMiddleware
  - [70] InventoryNotificationMiddleware
  - [100] NotificationMetricsMiddleware

NOTIFICATION SUBSCRIBERS:
  - AuditNotificationHandler : CustomerRegisteredNotification, InventoryUpdatedNotification, OrderCreatedNotification, OrderStatusChangedNotification
  - BusinessOperationsHandler : CustomerRegisteredNotification, OrderCreatedNotification
  - EmailNotificationHandler : CustomerRegisteredNotification, OrderCreatedNotification, OrderStatusChangedNotification
  - InventoryNotificationHandler : InventoryUpdatedNotification
```

## ?? Performance Metrics

The example includes comprehensive notification metrics and execution statistics:

```
=== NOTIFICATION METRICS ===
OrderCreatedNotification:
  Total: 2, Success: 2, Failures: 0, Success Rate: 100.0%, Avg Duration: 71.0ms
OrderStatusChangedNotification:
  Total: 2, Success: 2, Failures: 0, Success Rate: 100.0%, Avg Duration: 63.9ms
CustomerRegisteredNotification:
  Total: 2, Success: 2, Failures: 0, Success Rate: 100.0%, Avg Duration: 60.0ms
InventoryUpdatedNotification:
  Total: 5, Success: 5, Failures: 0, Success Rate: 100.0%, Avg Duration: 30.5ms
=============================

=== EXECUTION STATISTICS ===
Mediator Statistics:
Queries: 0
Commands: 0
Notifications: 4
=============================
```

## ?? Notification Middleware Pipeline

The notification middleware pipeline demonstrates selective execution based on notification types:

### For Order Notifications (IOrderNotification):
1. `NotificationErrorHandlingMiddleware` (int.MinValue) - Global error handling
2. `GeneralNotificationLoggingMiddleware` (10) - General logging
3. `OrderNotificationMiddleware` (50) - **Order-specific processing**
4. `NotificationMetricsMiddleware` (100) - Performance tracking
5. **Subscribers** - Email, Business, Audit handlers

### For Customer Notifications (ICustomerNotification):
1. `NotificationErrorHandlingMiddleware` (int.MinValue) - Global error handling
2. `GeneralNotificationLoggingMiddleware` (10) - General logging
3. `CustomerNotificationMiddleware` (60) - **Customer-specific processing**
4. `NotificationMetricsMiddleware` (100) - Performance tracking
5. **Subscribers** - Email, Business, Audit handlers

### For Inventory Notifications (IInventoryNotification):
1. `NotificationErrorHandlingMiddleware` (int.MinValue) - Global error handling
2. `GeneralNotificationLoggingMiddleware` (10) - General logging
3. `InventoryNotificationMiddleware` (70) - **Inventory-specific processing**
4. `NotificationMetricsMiddleware` (100) - Performance tracking
5. **Subscribers** - Inventory, Audit handlers

## ?? Type Constraints in Action

### Order Notification Middleware (Orders Only)
```csharp
public class OrderNotificationMiddleware : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process order notifications
        if (notification is IOrderNotification orderNotification)
        {
            Logger.LogInformation("?? Processing ORDER notification: {NotificationType} for Order {OrderId}",
                typeof(TNotification).Name, orderNotification.OrderId);
        }
        
        await next(notification, cancellationToken);
    }
}
```

### Inventory Notification Middleware (Inventory Only)
```csharp
public class InventoryNotificationMiddleware : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Type constraint check - only process inventory notifications
        if (notification is IInventoryNotification inventoryNotification)
        {
            Logger.LogInformation("?? Processing INVENTORY notification: {NotificationType} for Product {ProductId}",
                typeof(TNotification).Name, inventoryNotification.ProductId);
        }
        
        await next(notification, cancellationToken);
    }
}
```

## ????? Running the Example

```bash
cd samples/TypedSimpleNotificationExample
dotnet run
```

## ?? Expected Output

The application demonstrates the following workflow with clear type distinctions:

### 1. **?? Order Notifications**: 
- OrderCreatedNotification and OrderStatusChangedNotification
- Shows order-specific middleware processing
- Logs: "?? Processing ORDER notification: OrderCreatedNotification for Order 12345"

### 2. **?? Customer Notifications**: 
- CustomerRegisteredNotification
- Shows customer-specific middleware processing
- Logs: "?? Processing CUSTOMER notification: CustomerRegisteredNotification for Jane Smith"

### 3. **?? Inventory Notifications**: 
- InventoryUpdatedNotification with low stock and out-of-stock scenarios
- Shows inventory-specific middleware processing
- Logs: "?? Processing INVENTORY notification: InventoryUpdatedNotification for Product WIDGET-001"

### 4. **?? Complex Workflow**: 
- Demonstrates multiple notification types in sequence
- Shows how different middleware components process their specific notification types
- Includes customer registration ? order creation ? inventory updates ? status changes

### Sample Output:
```
==============================================
Blazing.Mediator - Typed Simple Notification Example
==============================================

This example demonstrates TYPE-CONSTRAINED notification middleware:

NOTIFICATION TYPES:
  - ICustomerNotification
    - CustomerRegisteredNotification

  - IInventoryNotification
    - InventoryUpdatedNotification

  - IOrderNotification
    - OrderCreatedNotification
    - OrderStatusChangedNotification

TYPE-CONSTRAINED MIDDLEWARE:
  - [-2147483648] NotificationErrorHandlingMiddleware
  - [10] GeneralNotificationLoggingMiddleware
  - [50] OrderNotificationMiddleware
  - [60] CustomerNotificationMiddleware
  - [70] InventoryNotificationMiddleware
  - [100] NotificationMetricsMiddleware

-------- ORDER NOTIFICATIONS (IOrderNotification Constraint) --------
?? Processing notification: OrderCreatedNotification
?? Processing ORDER notification: OrderCreatedNotification for Order 12345
?? ORDER CONFIRMATION EMAIL SENT
   To: john.doe@example.com
   Order: #12345
   Total: $299.97
   Items: 2 items
? Notification completed: OrderCreatedNotification in 52.3ms
```

## ?? Key Learning Points

This example teaches:

1. **Type-Constrained Notification Middleware**: How to use interface-based constraints to selectively process notifications
2. **Interface-Based Notification Categorization**: Clear separation between different types of business events
3. **Selective Processing**: How middleware can process only specific notification categories
4. **Multiple Subscribers**: How different handlers can subscribe to the same notification types
5. **Performance Optimization**: Avoiding unnecessary middleware execution for inappropriate notification types
6. **Complex Workflows**: How to orchestrate multiple notification types in business workflows
7. **Dynamic Pipeline Analysis**: Runtime inspection of notification middleware using `INotificationMiddlewarePipelineInspector`
8. **Metrics Tracking**: Performance monitoring and success rate tracking for notifications

## ?? Technologies Used

- **.NET 9.0**: Latest .NET framework with C# 13
- **Blazing.Mediator**: CQRS and mediator pattern implementation with type-constrained notification middleware
- **Microsoft.Extensions.Hosting**: Application hosting and dependency injection
- **FluentValidation**: Declarative validation (for consistency with TypedMiddlewareExample)
- **Custom Logging**: Clean console output formatting

## ?? Comparison with SimpleNotificationExample

| Feature | SimpleNotificationExample | TypedSimpleNotificationExample |
|---------|--------------------------|-------------------------------|
| Notification Types | Generic `INotification` | Categorized interfaces (`IOrderNotification`, etc.) |
| Middleware Targeting | All notifications | Type-specific processing |
| Visual Distinction | General notification processing | Category-specific icons (??, ??, ??) |
| Type Constraints | None | Interface-based constraints |
| Selective Processing | No | Yes, based on notification category |
| Pipeline Analysis | Basic | Dynamic with `INotificationMiddlewarePipelineInspector` |
| Metrics Tracking | Basic | Comprehensive with success rates and timing |

This example builds upon the SimpleNotificationExample to show how type constraints can provide more precise control over notification middleware execution based on business event categories.

## ?? Related Projects

- **SimpleNotificationExample**: Basic notification patterns without type constraints
- **TypedMiddlewareExample**: Type-constrained request middleware for commands and queries
- **MiddlewareExample**: Advanced middleware pipeline with inspection capabilities
- **ECommerce.Api**: Real-world notification usage in a web API

## ?? Advanced Features

### Pipeline Inspection
The example demonstrates advanced pipeline inspection capabilities:
- **Dynamic Middleware Discovery**: Uses `INotificationMiddlewarePipelineInspector` to analyze registered middleware
- **Constraint Analysis**: Shows actual generic constraints on middleware types
- **Order Visualization**: Displays middleware execution order including special values like `int.MinValue`

### Metrics Collection
Comprehensive metrics collection includes:
- **Execution Counts**: Total notifications processed per type
- **Success Rates**: Percentage of successful notifications
- **Performance Timing**: Average execution duration per notification type
- **Failure Tracking**: Count and analysis of failed notifications