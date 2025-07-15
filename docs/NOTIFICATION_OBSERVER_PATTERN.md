# Notification Observer Pattern in Blazing.Mediator

## Overview

The notification system in Blazing.Mediator follows the **pure observer pattern** where:

-   **Publishers** blindly send notifications without caring about recipients
-   **Subscribers** actively choose to subscribe/unsubscribe to notifications they're interested in
-   **No handlers** are required - it's a push mechanism, not a pull mechanism

## Key Principles

### 1. No Handlers Required

Unlike requests that require handlers to process them, notifications are fire-and-forget. The publisher sends a notification and doesn't care if anyone is listening or how many subscribers there are.

### 2. Active Subscription Model

Subscribers must actively subscribe to notifications they want to receive. This is different from request handlers which are automatically discovered and registered.

### 3. Decoupled Communication

Publishers and subscribers are completely decoupled. Publishers don't know about subscribers, and subscribers don't know about publishers.

## Architecture

### Core Components

1. **INotification** (already exists)

    - Marker interface for all notifications
    - Example: `OrderCreatedNotification`, `UserLoggedInNotification`

2. **INotificationSubscriber<TNotification>**

    - Interface for objects that want to receive specific notifications
    - Contains a method like `OnNotification(TNotification notification, CancellationToken cancellationToken)`

3. **INotificationSubscriber** (generic/broadcast)

    - Interface for objects that want to receive all notifications
    - Contains a method like `OnNotification(INotification notification, CancellationToken cancellationToken)`

4. **Subscription Management**

    - `Subscribe<TNotification>(INotificationSubscriber<TNotification> subscriber)`
    - `Subscribe(INotificationSubscriber subscriber)` - for all notifications
    - `Unsubscribe<TNotification>(INotificationSubscriber<TNotification> subscriber)`
    - `Unsubscribe(INotificationSubscriber subscriber)` - from all notifications

5. **Publishing**
    - `Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)`
    - Sends notification to all subscribers of that specific type AND all generic subscribers

## Usage Examples

### Creating a Notification

```csharp
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Subscribing to Specific Notifications

```csharp
public class EmailService : INotificationSubscriber<OrderCreatedNotification>
{
    public Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Send email about order creation
        return SendOrderConfirmationEmail(notification.OrderId, notification.CustomerName);
    }
}

// Subscribe
var emailService = new EmailService();
mediator.Subscribe<OrderCreatedNotification>(emailService);
```

### Subscribing to All Notifications (Generic/Broadcast)

```csharp
public class AuditService : INotificationSubscriber
{
    public Task OnNotification(INotification notification, CancellationToken cancellationToken)
    {
        // Log all notifications for audit purposes
        return LogNotification(notification);
    }
}

// Subscribe to all notifications
var auditService = new AuditService();
mediator.Subscribe(auditService);
```

### Publishing Notifications

```csharp
// Publisher doesn't care who's listening
await mediator.Publish(new OrderCreatedNotification
{
    OrderId = 123,
    CustomerName = "John Doe",
    CreatedAt = DateTime.UtcNow
});
```

### Unsubscribing

```csharp
// Unsubscribe from specific notifications
mediator.Unsubscribe<OrderCreatedNotification>(emailService);

// Unsubscribe from all notifications
mediator.Unsubscribe(auditService);
```

## Middleware Support

### One-Way Middleware

Unlike request middleware which is bidirectional (can modify request/response), notification middleware is one-way (sender to receiver only).

### Notification Middleware Interface

```csharp
public interface INotificationMiddleware
{
    Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification;
}
```

### Conditional Notification Middleware

```csharp
public interface IConditionalNotificationMiddleware : INotificationMiddleware
{
    bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification;
}
```

### Middleware Examples

```csharp
// Logging middleware
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        _logger.LogInformation("Publishing notification: {NotificationType}", typeof(TNotification).Name);
        await next(notification, cancellationToken);
        _logger.LogInformation("Notification published: {NotificationType}", typeof(TNotification).Name);
    }
}

// Conditional middleware - only for specific notification types
public class OrderNotificationMiddleware : IConditionalNotificationMiddleware
{
    public bool ShouldExecute<TNotification>(TNotification notification) where TNotification : INotification
    {
        return notification is OrderCreatedNotification or OrderCancelledNotification;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Do something specific for order notifications
        await next(notification, cancellationToken);
    }
}
```

## Internal Implementation Notes

### Subscription Storage

-   Maintain internal collections of subscribers
-   Separate storage for specific notification subscribers and generic subscribers
-   Thread-safe collections for concurrent access

### Publishing Process

1. Notification enters middleware pipeline
2. Middleware processes notification (logging, validation, etc.)
3. Notification is delivered to all specific subscribers
4. Notification is delivered to all generic subscribers
5. Process is fire-and-forget - no return values or aggregation

### Error Handling

-   Exceptions in one subscriber should not affect other subscribers
-   Consider isolated execution contexts for each subscriber
-   Middleware can handle/log errors before they reach subscribers

## Benefits of This Approach

1. **True Decoupling**: Publishers and subscribers don't know about each other
2. **Scalability**: Easy to add/remove subscribers without affecting publishers
3. **Flexibility**: Subscribers can choose what they want to listen to
4. **Testability**: Easy to test publishers and subscribers independently
5. **Performance**: No need for handler discovery or instantiation overhead
6. **Clear Intent**: Observer pattern makes the intent clear - it's about notifications, not processing

## Integration with Existing Blazing.Mediator

The notification system should integrate seamlessly with the existing request/response patterns:

-   Same `IMediator` interface extended with notification methods
-   Same DI container and lifecycle management
-   Same middleware pipeline concepts (adapted for one-way flow)
-   Same configuration and setup patterns
