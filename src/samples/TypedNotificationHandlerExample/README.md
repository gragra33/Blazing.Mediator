# TypedNotificationHandlerExample - Blazing.Mediator Type-Constrained Handlers with Automatic Discovery

A comprehensive demonstration project showcasing **type-constrained notification middleware** with **automatic handler discovery** in Blazing.Mediator, highlighting how different notification middleware can be selectively applied based on notification interface types while automatically discovering and registering handlers.

## ?? Overview

This example demonstrates the **automatic notification handler pattern** using `INotificationHandler<T>` where handlers are automatically discovered and registered without manual subscription. It showcases how notification middleware can be selectively applied based on notification types using generic type constraints, demonstrating the power of combining automatic discovery with type-safe middleware filtering.

> **Key Difference**: This example uses the modern **automatic notification handler pattern** where handlers are discovered and registered automatically. For manual subscription patterns, see the `TypedNotificationSubscriberExample` project.

## ?? Key Features Demonstrated

### Automatic Handler Discovery
- **Zero Configuration**: Handlers implementing `INotificationHandler<T>` are automatically discovered and registered
- **Multi-Type Support**: Single handler classes can process multiple notification types
- **Scoped Lifetime**: All handlers are registered as scoped services automatically
- **Type Safety**: Compile-time type checking ensures correct handler-notification matching

### Type-Constrained Middleware
- **Selective Processing**: Middleware only processes notifications matching their generic constraints
- **Interface-Based Constraints**: Uses marker interfaces (`IOrderNotification`, `ICustomerNotification`, `IInventoryNotification`)
- **Ordered Execution**: Middleware executes in defined order (100, 200, 250, 300, 400)
- **Type-Safe Filtering**: Only applicable middleware runs for each notification type

### Multiple Handler Support
- **Single Notification ? Multiple Handlers**: Each notification automatically triggers all relevant handlers
- **Independent Processing**: Handlers process notifications independently
- **Parallel-Safe**: Handlers can run concurrently without conflicts
- **Exception Isolation**: Handler failures don't affect other handlers

### Comprehensive Middleware Pipeline
- **General Logging**: Tracks all notification processing with timing
- **Type-Constrained Processing**: Order, Customer, and Inventory specific middleware
- **Performance Monitoring**: Automatic metrics collection and reporting
- **Error Handling**: Graceful exception handling with detailed logging

## ?? Project Structure

```
TypedNotificationHandlerExample/
??? Notifications/
?   ??? NotificationInterfaces.cs      # Marker interfaces for type constraints
?   ??? BusinessNotifications.cs       # Concrete notification implementations
??? Handlers/
?   ??? NotificationHandlers.cs        # Automatic notification handlers
??? Middleware/
?   ??? TypedNotificationMiddleware.cs # Type-constrained middleware
??? Services/
?   ??? Runner.cs                       # Demo orchestration
?   ??? NotificationPipelineDisplayer.cs # Pipeline analysis
??? Program.cs                          # Application entry point
??? README.md                          # This file
```

## ?? Notification Types & Constraints

### Marker Interfaces (Type Constraints)
- **`IOrderNotification`**: Order-related notifications (OrderId, CustomerEmail)
- **`ICustomerNotification`**: Customer-related notifications (CustomerEmail, CustomerName)  
- **`IInventoryNotification`**: Inventory-related notifications (ProductId, Quantity)

### Concrete Notifications
- **`OrderCreatedNotification`**: Implements `IOrderNotification`
- **`OrderStatusChangedNotification`**: Implements `IOrderNotification`
- **`CustomerRegisteredNotification`**: Implements `ICustomerNotification`
- **`InventoryUpdatedNotification`**: Implements `IInventoryNotification`

## ?? Automatic Handler Discovery

### EmailNotificationHandler
- **Auto-Discovery**: Automatically registered for multiple notification types
- **Handles**: OrderCreated, OrderStatusChanged, CustomerRegistered
- **Functionality**: Sends confirmation emails, status updates, welcome messages

### InventoryNotificationHandler  
- **Auto-Discovery**: Automatically registered for inventory notifications
- **Handles**: InventoryUpdated
- **Functionality**: Stock level management, low stock alerts, out-of-stock warnings

### BusinessOperationsHandler
- **Auto-Discovery**: Automatically registered for business logic
- **Handles**: OrderCreated, OrderStatusChanged
- **Functionality**: Business rules, VIP processing, workflow management

### AuditNotificationHandler
- **Auto-Discovery**: Automatically registered for all notification types
- **Handles**: All notifications (OrderCreated, OrderStatusChanged, CustomerRegistered, InventoryUpdated)
- **Functionality**: Comprehensive audit logging for compliance

## ?? Middleware Pipeline (Order of Execution)

### 1. NotificationLoggingMiddleware [100]
- **Constraint**: All notifications (`TNotification : INotification`)
- **Function**: Logs start/completion with timing for all notifications
- **Type-Safe**: Runs for every notification type

### 2. OrderNotificationMiddleware [200] 
- **Constraint**: Order notifications only (`TNotification : IOrderNotification`)
- **Function**: Order-specific validation and processing
- **Type-Safe**: Only processes OrderCreated and OrderStatusChanged notifications

### 3. CustomerNotificationMiddleware [250]
- **Constraint**: Customer notifications only (`TNotification : ICustomerNotification`)
- **Function**: Customer-specific validation and email verification
- **Type-Safe**: Only processes CustomerRegistered notifications

### 4. InventoryNotificationMiddleware [300]
- **Constraint**: Inventory notifications only (`TNotification : IInventoryNotification`)  
- **Function**: Inventory-specific validation and quantity checks
- **Type-Safe**: Only processes InventoryUpdated notifications

### 5. NotificationMetricsMiddleware [400]
- **Constraint**: All notifications (`TNotification : INotification`)
- **Function**: Performance metrics collection and reporting
- **Type-Safe**: Captures metrics for all notification types

## ?? Running the Example

```bash
cd src/samples/TypedNotificationHandlerExample
dotnet run
```

### Expected Output

The demo will display:

1. **Pipeline Analysis**: Shows automatically discovered handlers and middleware configuration
2. **Handler Discovery**: Lists all handlers found through automatic discovery
3. **Type-Constrained Processing**: Demonstrates selective middleware execution
4. **Multiple Handler Execution**: Shows multiple handlers processing single notifications
5. **Complex Workflow**: Multi-step business scenario with automatic processing
6. **Performance Statistics**: Timing and metrics for all operations

## ?? Key Observations

### Automatic vs Manual Patterns

| Feature | Automatic Handlers | Manual Subscribers |
|---------|-------------------|-------------------|
| **Registration** | Automatic discovery | Manual subscription required |
| **Interface** | `INotificationHandler<T>` | `INotificationSubscriber<T>` |
| **Discovery** | Assembly scanning | Manual DI registration |
| **Type Safety** | Compile-time checking | Runtime subscription |
| **Maintainability** | Add handler class ? Works | Add class + subscription |

### Middleware Execution per Notification Type

- **OrderCreatedNotification**: Logging [100] ? Order [200] ? Metrics [400]
- **CustomerRegisteredNotification**: Logging [100] ? Customer [250] ? Metrics [400]  
- **InventoryUpdatedNotification**: Logging [100] ? Inventory [300] ? Metrics [400]

### Handler Execution Patterns

Each notification triggers **multiple handlers automatically**:
- OrderCreatedNotification ? EmailNotificationHandler + BusinessOperationsHandler + AuditNotificationHandler
- CustomerRegisteredNotification ? EmailNotificationHandler + AuditNotificationHandler
- InventoryUpdatedNotification ? InventoryNotificationHandler + AuditNotificationHandler

## ?? Learning Outcomes

This example teaches:

1. **Automatic Handler Discovery**: How `INotificationHandler<T>` provides zero-configuration handler registration
2. **Type-Constrained Middleware**: Using generic constraints for selective middleware execution
3. **Multi-Type Handlers**: Single handler classes processing multiple notification types
4. **Middleware Ordering**: How `[Middleware(order)]` attributes control execution sequence
5. **Performance Monitoring**: Automatic metrics collection in middleware pipeline
6. **Exception Handling**: Graceful error handling in both handlers and middleware
7. **Modern Patterns**: Comparing automatic discovery vs manual subscription approaches

## ?? Comparison with TypedNotificationSubscriberExample

| Aspect | TypedNotificationHandlerExample | TypedNotificationSubscriberExample |
|--------|--------------------------------|-----------------------------------|
| **Pattern** | Automatic Discovery | Manual Subscription |
| **Registration** | Zero-configuration | Requires manual DI setup |
| **Handler Interface** | `INotificationHandler<T>` | `INotificationSubscriber<T>` |
| **Discovery Method** | Assembly scanning | Manual service registration |
| **Maintenance** | Add class ? Auto-works | Add class + register service |
| **Type Safety** | Compile-time + Runtime | Runtime subscription |
| **Best For** | Modern applications | Legacy/explicit control needs |

## ?? Related Examples

- **`NotificationHandlerExample`**: Basic automatic handler discovery without type constraints
- **`TypedNotificationSubscriberExample`**: Type-constrained middleware with manual subscription
- **`NotificationSubscriberExample`**: Basic manual notification subscription patterns
- **`MediatorStatisticsExample`**: Advanced statistics and analytics for both patterns

## ?? Best Practices Demonstrated

1. **Use Marker Interfaces**: Define clear contracts for type constraints (`IOrderNotification`, etc.)
2. **Automatic Discovery**: Leverage `INotificationHandler<T>` for zero-configuration setup
3. **Multiple Handlers**: Design handlers to be independent and composable
4. **Middleware Ordering**: Use explicit order attributes for predictable execution
5. **Type Safety**: Combine compile-time checking with runtime validation
6. **Performance Monitoring**: Include metrics middleware for production visibility
7. **Exception Isolation**: Handle errors gracefully without affecting other handlers

---

This example showcases the power and simplicity of combining automatic handler discovery with type-constrained middleware in Blazing.Mediator, providing a modern, maintainable approach to notification processing in .NET applications.