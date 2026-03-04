# Blazing.Mediator - Notification Systems Guide

> [!CAUTION]
> Version 3.0.0 has breaking changes. See the **[Breaking Changes](https://github.com/gragra33/Blazing.Mediator/docs/BREAKING_CHANGES.md)** document for a complete list of API changes with before/after comparisons, and the **[MIGRATION_GUIDE.md](https://github.com/gragra33/Blazing.Mediator/docs/MIGRATION_GUIDE.md)** for full upgrade steps.

## Table of Contents

- [Introduction](#introduction)
- [Quick Reference Tables](#quick-reference-tables)
- [Pattern 1: Manual Subscriber System (Observer Pattern)](#pattern-1-manual-subscriber-system-observer-pattern-1)
    - [Core Components](#core-components)
    - [Implementation Examples](#implementation-examples)
    - [Service Registration and Configuration](#service-registration-and-configuration)
    - [Publishing Notifications with Subscribe / Unsubscribe Lifecycle](#publishing-notifications-with-subscribe--unsubscribe-lifecycle)
    - [Pattern 1 Benefits and Use Cases](#pattern-1-benefits-and-use-cases)
    - [Complete Blazor Client Example](#complete-blazor-client-example)

- [Pattern 2: Automatic Handler System (Publish-Subscribe Pattern)](#pattern-2-automatic-handler-system-publish-subscribe-pattern-1)
    - [Core Components](#core-components-1)
    - [Implementation Examples](#implementation-examples-1)
    - [Pattern 2 Handler Examples](#pattern-2-handler-examples)
    - [Service Registration and Configuration](#service-registration-and-configuration-1)
    - [Publishing Notifications](#publishing-notifications-1)
    - [Handler Type Comparison](#handler-type-comparison)
    - [Pattern 2 Benefits and Use Cases](#pattern-2-benefits-and-use-cases)
    - [Complete ECommerce Example](#complete-ecommerce-example)

## Introduction

Blazing.Mediator implements two distinct yet complementary notification systems that enable sophisticated event-driven communication patterns within .NET applications. These dual systems provide developers with maximum flexibility to choose the most appropriate notification pattern for their specific architectural requirements, or to combine both approaches within the same application.

The notification architecture supports full bidirectional middleware integration, allowing for comprehensive cross-cutting concerns such as logging, validation, error handling, and performance monitoring to be applied consistently across all notification processing.

### Dual Notification System Architecture

#### Pattern 1: Manual Subscriber System (Observer Pattern)

The Manual Subscriber System implements the classic Observer Pattern, where notification consumers actively register their interest in specific notification types through explicit subscription calls. This system provides runtime flexibility, allowing subscribers to dynamically start and stop listening to notifications based on application state, user preferences, or business logic conditions.

**Key Characteristics:**

- **Runtime Subscription Management**: Subscribers can join or leave notification streams during application execution
- **Explicit Control**: Clear, intentional subscribe and unsubscribe operations
- **Dynamic Behavior**: Subscription state can change based on runtime conditions
- **Observer Pattern Compliance**: Follows the traditional Gang of Four Observer Pattern implementation
- **Lifecycle Awareness**: Subscribers manage their own notification lifecycle
- **Client-Optimized**: Particularly well-suited for client applications like Blazor WebAssembly, MAUI, WPF, WinForms, Console applications, and other desktop/mobile scenarios where dynamic subscription management is essential

#### Pattern 2: Automatic Handler System (Publish-Subscribe Pattern)

The Automatic Handler System implements a Publish-Subscribe Pattern with automatic handler discovery through the Dependency Injection container. This system provides a convention-based approach where notification handlers are automatically registered during application startup and invoked without explicit subscription management.

**Key Characteristics:**

- **Automatic Discovery**: Handlers are discovered and registered through DI container scanning
- **Convention-Based**: Follows standard dependency injection registration patterns
- **Compile-Time Registration**: Handler relationships are established during application startup
- **Zero Configuration**: No manual subscription code required for basic scenarios
- **DI Integration**: Leverages existing dependency injection infrastructure and patterns
- **Server-Optimized**: Ideal for server applications like ASP.NET Core, Web API, Blazor Server, and microservices, but equally effective for client applications requiring structured, predictable notification handling

### Middleware Integration Architecture

Both notification systems seamlessly integrate with a sophisticated bidirectional middleware pipeline that supports comprehensive request and response processing. The middleware architecture provides:

**Bidirectional Processing:**

- **Pre-notification Processing**: Middleware executes before notification delivery to subscribers/handlers
- **Post-notification Processing**: Middleware executes after notification processing completes
- **Exception Handling**: Comprehensive error handling and recovery mechanisms
- **State Management**: Maintaining context and state throughout the notification pipeline

**Multiple Processor Support:**

- **Concurrent Processing**: Multiple subscribers and handlers can process the same notification simultaneously
- **Independent Execution**: Each processor operates independently with isolated error handling
- **Aggregated Results**: Middleware can collect and analyze results from multiple processors
- **Performance Monitoring**: Individual and aggregate performance metrics for all processors

### Notification Flow Architecture

The visual diagram below illustrates how notifications flow through the Blazing.Mediator system. The two patterns are **mutually exclusive dispatch paths**: a notification type is handled by _either_ the source-gen handler path _or_ the subscriber path — never both. Both paths support notification middleware, but via different mechanisms.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                   Blazing.Mediator Notification Dispatch Flow                   │
└─────────────────────────────────────────────────────────────────────────────────┘

📢 IMediator.Publish<TNotification>(notification, ct)
    │
    ▼
┌──────────────────────────────────────────────────────────────┐
│  GetDispatcher()  ─  retrieve source-gen singleton           │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
              ┌────────────────────────────────┐
              │  IsNotificationHandled         │
              │      <TNotification>()         │
              │  (JIT-constant, zero overhead) │
              └──────────────┬─────────────────┘
                             │
              ┌──────────────┴────────────────┐
              │ true                          │ false
              ▼                               ▼
┌─────────────────────────┐   ┌──────────────────────────────────────────────┐
│  PATTERN 2              │   │  PATTERN 1                                   │
│  Source-gen Handler     │   │  Subscriber-only Path                        │
│  Path                   │   │                                              │
│                         │   │  ┌────────────────────────────────────────┐  │
│  d.PublishAsync()       │   │  │  Notification Middleware Pipeline      │  │
│  (generated dispatcher) │   │  │  (pre-processing, runtime DI)          │  │
│         │               │   │  └────────────────────┬───────────────────┘  │
│         ▼               │   │                       │                      │
│  NotificationHandler    │   │       ┌───────────────┴────────────────┐     │
│  Wrapper_{TypeName}     │   │       │                                │     │
│  • pre-resolved at      │   │       ▼                                ▼     │
│    Init() time          │   │  ┌────────────────────┐  ┌─────────────────┐ │
│  • zero DI per call     │   │  │INotificationSubscri│  │INotification    │ │
│         │               │   │  │ber<TNotification>  │  │Subscriber       │ │
│         ▼               │   │  │(specific typed)    │  │(generic, any T) │ │
│  ┌─────────────────┐    │   │  └────────────────────┘  └─────────────────┘ │
│  │ Notification    │    │   │                                              │
│  │ Middleware Chain│    │   │  ┌────────────────────────────────────────┐  │
│  │ INotification   │    │   │  │  Notification Middleware Pipeline      │  │
│  │ Middleware      │    │   │  │  (post-processing + telemetry)         │  │
│  │ <TNotification> │    │   │  └────────────────────────────────────────┘  │
│  │ (source-gen,    │    │   └──────────────────────────────────────────────┘
│  │  zero DI/call)  │    │
│  └────────┬────────┘    │
│           │             │
│           ▼             │
│  INotificationPublisher │
│  .Publish(handlers,...) │
│         │               │
│  ┌──────┴──────┐        │
│  │ Sequential  │        │
│  │ (default)   │        │
│  │  ─OR─       │        │
│  │ Concurrent  │        │
│  └──────┬──────┘        │
│         │               │
│         ▼               │
│  INotificationHandler   │
│  <TNotification>[]      │
│  ┌──────┐ ┌──────┐      │
│  │  A   │ │  B   │ ...  │
│  └──────┘ └──────┘      │
└─────────────────────────┘

KEY (middleware):
  Pattern 2 — INotificationMiddleware<TNotification> chain baked by source generator;
               pre-resolved at Init(), zero DI lookups per call
  Pattern 1 — INotificationMiddleware (non-generic) executed by runtime pipeline builder;
               resolved from DI on each publish

═══════════════════════════════════════════════════════════════════════════════════
```

#### Source-Gen Handler Dispatch Detail

The following diagram shows how the source-gen path resolves and invokes handlers at runtime:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                Source-Gen Handler Dispatch (Pattern 2 deep-dive)                │
└─────────────────────────────────────────────────────────────────────────────────┘

Build time (Roslyn source generator):
┌─────────────────────────────────────────────────────────────────────────────────┐
│  Blazing.Mediator.SourceGenerators emits  →  Mediator.g.cs                      │
│                                                                                 │
│  class ContainerMetadata : MediatorDispatcherBase                               │
│  {                                                                              │
│    NotificationHandlerWrapper_OrderCreated  _wrap_OrderCreated;                 │
│    NotificationHandlerWrapper_UserRegistered _wrap_UserRegistered;              │
│    ...                                                                          │
│                                                                                 │
│    bool IsNotificationHandled<T>() → compile-time type switch (constant)        │
│    ValueTask PublishAsync<T>()     → compile-time type switch dispatch          │
│  }                                                                              │
└─────────────────────────────────────────────────────────────────────────────────┘

Startup (Init called once by AddMediator):
┌─────────────────────────────────────────────────────────────────────────────────┐
│  NotificationHandlerWrapper_OrderCreated.Init(IServiceProvider sp)              │
│    _handlers = sp.GetServices<INotificationHandler<OrderCreated>>().ToArray()   │
│    _publisher = sp.GetRequiredService<INotificationPublisher>()                 │
│    _nmw0     = sp.GetRequiredService<INotificationMiddleware<OrderCreated>>()   │
│    ...  (one _nmwN field per applicable middleware — resolved once)             │
│    (all references stored as fields; no DI at call time)                        │
└─────────────────────────────────────────────────────────────────────────────────┘

Per-call (zero DI lookups):
┌─────────────────────────────────────────────────────────────────────────────────┐
│  PublishAsync<OrderCreated>(notification, ct)                                   │
│    │                                                                            │
│    ▼                                                                            │
│  _wrap_OrderCreated.Handle(notification, ct)                                    │
│    │                                                                            │
│    ├─ if middleware registered (notifMiddleware.Count > 0):                     │
│    │     NotificationDelegate<OrderCreated> next =                              │
│    │       (n,ct) => _publisher.Publish(                                        │
│    │           new NotificationHandlers<OrderCreated>(_handlers), n, ct)        │
│    │     // chain built in reverse order (outermost middleware last):           │
│    │     next = (n,ct) => _nmw0.InvokeAsync(n, prev0, ct)                       │
│    │     return next(notification, ct)                                          │
│    │                                                                            │
│    └─ if no middleware:                                                         │
│          return _publisher.Publish(                                             │
│              new NotificationHandlers<OrderCreated>(_handlers),                 │
│              notification, ct)                                                  │
│    │                                                                            │
│    INotificationPublisher strategies:                                           │
│    ├── SequentialNotificationPublisher (default)                                │
│    │     foreach handler → await handler.Handle(notification, ct)               │
│    │                                                                            │
│    └── ConcurrentNotificationPublisher (opt-in)                                 │
│          await Task.WhenAll(handlers.Select(h => h.Handle(notification, ct)))   │
└─────────────────────────────────────────────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════════════════════════
```

#### System Characteristics:

- ✅ Dual Pattern Support: Both manual subscriber and automatic handler patterns available
- ✅ Mutually Exclusive Dispatch: Each notification type routes to exactly one path (no double-firing)
- ✅ Zero-DI Handler Dispatch: Source-gen path pre-resolves handlers at startup — no DI per call
- ✅ Middleware on Both Paths: Pattern 2 uses source-gen compiled `INotificationMiddleware<TNotification>` chains (zero DI per call); Pattern 1 uses the runtime `_notificationPipelineBuilder` with `INotificationMiddleware`
- ✅ Pluggable Publisher Strategy: Sequential (default) or Concurrent handler invocation via `INotificationPublisher`
- ✅ Comprehensive Telemetry: Detailed metrics and monitoring on the subscriber path
- ✅ Flexible Architecture: Mix and match patterns based on requirements

### System Integration Benefits

The dual notification system architecture provides several key advantages:

- **Architectural Flexibility**: Choose the most appropriate pattern for each notification scenario, or combine both patterns within the same application for maximum flexibility.

- **Gradual Migration**: Existing applications can migrate from one pattern to another incrementally, supporting both systems during transition periods.

- **Performance Optimization**: Manual subscribers provide runtime control for performance-critical scenarios, while automatic handlers offer zero-configuration convenience.

- **Testing Strategies**: Each pattern supports different testing approaches - manual subscribers enable dynamic test scenarios, while automatic handlers support standard dependency injection testing patterns.

- **Scalability Options**: Different scaling strategies can be applied to each pattern based on specific performance and resource requirements.

### Middleware Pipeline Features

The notification middleware pipeline provides comprehensive processing capabilities:

- **Cross-Cutting Concerns**: Apply logging, validation, authorization, caching, and other cross-cutting concerns uniformly across all notifications.

- **Error Handling**: Sophisticated error handling with the ability to continue processing other subscribers/handlers even when individual processors fail.

- **Performance Monitoring**: Detailed telemetry and metrics collection for monitoring notification processing performance and success rates.

- **Conditional Processing**: Middleware can selectively process notifications based on type, content, or runtime conditions.

- **State Management**: Maintain processing context and state throughout the entire notification pipeline execution.

### Outbox Pattern Integration

Notification handlers, particularly the Automatic Handler System, provide an excellent foundation for implementing the **Outbox Pattern** in distributed systems. The Outbox Pattern is a critical microservices design pattern that ensures reliable message delivery and maintains data consistency across service boundaries by storing outbound messages in the same database transaction as business data changes.

**Pattern Purpose:**
The Outbox Pattern solves the dual-write problem in distributed systems, where applications need to atomically update their local database and publish messages to external systems. Without proper coordination, these operations can fail independently, leading to data inconsistencies and lost messages.

**Integration Benefits with Blazing.Mediator:**
Notification handlers serve as the perfect first step in the Outbox Pattern implementation. When domain events are published within a transaction, notification handlers can capture these events and store them in an outbox table within the same database transaction, ensuring atomicity between business data changes and message preparation.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                    Outbox Pattern with Blazing.Mediator                         │
└─────────────────────────────────────────────────────────────────────────────────┘

📊 Business Operation (e.g., Create Order)
    │
    ├─── Database Transaction Begins ─────────────────────────────┐
    │                                                             │
    │    ┌─────────────────────────────────────────────────────┐  │
    │    │              Business Logic Layer                   │  │
    │    │                                                     │  │
    │    │  1. Update Business Entities                        │  │
    │    │     └─ Order Status = "Created"                     │  │
    │    │     └─ Inventory Reduced                            │  │
    │    │     └─ Customer Points Updated                      │  │
    │    │                                                     │  │
    │    │  2. Publish Domain Events                           │  │
    │    │     └─ OrderCreatedNotification                     │  │
    │    │     └─ InventoryReducedNotification                 │  │
    │    │     └─ CustomerPointsUpdatedNotification            │  │
    │    └─────────────────────────────────────────────────────┘  │
    │                                   │                         │
    │                                   ▼                         │
    │    ┌─────────────────────────────────────────────────────┐  │
    │    │        Blazing.Mediator Notification System         │  │
    │    │                                                     │  │
    │    │    ┌─────────────────────────────────────────┐      │  │
    │    │    │     Outbox Notification Handler         │      │  │  ATOMIC
    │    │    │   (INotificationHandler<OrderCreated>)  │      │  │ DATABASE
    │    │    │                                         │      │  │ TRANSACTION
    │    │    │  3. Store Messages in Outbox Table      │      │  │
    │    │    │     └─ EventId: GUID                    │      │  │
    │    │    │     └─ EventType: "OrderCreated"        │      │  │
    │    │    │     └─ Payload: JSON Serialized         │      │  │
    │    │    │     └─ Status: "Pending"                │      │  │
    │    │    │     └─ CreatedAt: Timestamp             │      │  │
    │    │    │     └─ RetryCount: 0                    │      │  │
    │    │    └─────────────────────────────────────────┘      │  │
    │    └─────────────────────────────────────────────────────┘  │
    │                                                             │
    ├─── Database Transaction Commits ────────────────────────────┘
    │
    │    ┌─────────────────────────────────────────────────────┐
    │    │              Outbox Publisher                       │
    │    │          (Background Service)                       │
    │    │                                                     │
    │    │  4. Poll Outbox Table                               │
    │    │     └─ SELECT * FROM Outbox                         │
    │    │       WHERE Status = 'Pending'                      │
    │    │       ORDER BY CreatedAt                            │
    │    │                                                     │
    │    │  5. Process Pending Messages                        │
    │    │     ├─ Deserialize Payload                          │
    │    │     ├─ Publish to Message Broker                    │
    │    │     │   └─ Service Bus / RabbitMQ / Kafka           │
    │    │     ├─ Call External APIs                           │
    │    │     └─ Send Webhooks                                │
    │    │                                                     │
    │    │  6. Update Message Status                           │
    │    │     ├─ Success: Status = "Processed"                │
    │    │     ├─ Failure: Status = "Failed"                   │
    │    │     └─ RetryCount++                                 │
    │    │                                                     │
    │    │  7. Retry Logic & Dead Letter Handling              │
    │    │     ├─ Exponential Backoff                          │
    │    │     ├─ Maximum Retry Attempts                       │
    │    │     └─ Dead Letter Queue for Failed Messages        │
    │    └─────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────┐
│                External Systems                             │
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │  Payment    │  │  Shipping   │  │   Email     │          │
│  │  Service    │  │  Service    │  │  Service    │          │
│  │             │  │             │  │             │          │
│  │ • Process   │  │ • Create    │  │ • Send      │          │
│  │   Payment   │  │   Shipment  │  │   Receipt   │          │
│  │ • Update    │  │ • Track     │  │ • Notify    │          │
│  │   Status    │  │   Package   │  │   Customer  │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

#### Outbox Pattern Benefits:

- ✅ Atomic Operations: Business data and message storage in single transaction
- ✅ Reliability: Guaranteed message delivery through persistent storage
- ✅ Consistency: Prevents dual-write problems and data inconsistencies
- ✅ Resilience: Built-in retry mechanisms and error handling
- ✅ Observability: Full audit trail of message processing
- ✅ Scalability: Asynchronous processing decoupled from business operations

#### Implementation Considerations:

- 🔧 Database Storage: Outbox table in same database as business data
- 🔧 Background Processing: Separate service polls and processes messages
- 🔧 Idempotency: Ensure message processing is idempotent
- 🔧 Monitoring: Track message processing success rates and latency
- 🔧 Cleanup: Regular cleanup of processed messages to prevent table growth

## Quick Reference Tables

### Notification Patterns Comparison

Understanding the two notification systems helps you choose the right approach for your specific requirements. Each pattern serves different architectural needs and provides unique advantages depending on your application's structure and usage patterns.

| **Aspect**        | **Manual Subscriber System**           | **Automatic Handler System**                   |
| ----------------- | -------------------------------------- | ---------------------------------------------- |
| **Pattern Type**  | Observer Pattern                       | Publish-Subscribe Pattern                      |
| **Registration**  | Runtime subscription/unsubscription    | Compile-time source-gen + DI registration      |
| **Discovery**     | Manual subscription management         | Source-gen compiled dispatch (zero reflection) |
| **Interface**     | `INotificationSubscriber<T>`           | `INotificationHandler<T>`                      |
| **Configuration** | `mediator.Subscribe(handler)`          | Zero configuration required                    |
| **Lifecycle**     | Dynamic runtime control                | Application startup registration               |
| **Best For**      | Client applications, dynamic scenarios | Server applications, structured processing     |
| **Examples**      | Blazor WebAssembly, MAUI, WPF          | ASP.NET Core, Web API, microservices           |

### Core Interfaces

These interfaces form the foundation of both notification systems, providing type-safe contracts for publishing and handling domain events throughout your application.

| **Interface**                | **Purpose**                                       | **Pattern** | **Example Usage**                                                              |
| ---------------------------- | ------------------------------------------------- | ----------- | ------------------------------------------------------------------------------ |
| `INotification`              | Marker interface for all notifications            | Both        | `public class OrderCreatedNotification : INotification`                        |
| `INotificationSubscriber<T>` | Type-specific manual subscription interface       | Pattern 1   | `public class EmailSubscriber : INotificationSubscriber<OrderCreated>`         |
| `INotificationSubscriber`    | Broadcast manual subscription (all notifications) | Pattern 1   | `public class AuditSubscriber : INotificationSubscriber`                       |
| `INotificationHandler<T>`    | Automatic handler interface (source-gen dispatch) | Pattern 2   | `public class EmailHandler : INotificationHandler<OrderCreated>`               |
| `INotificationPublisher`     | Pluggable handler dispatch strategy               | Pattern 2   | Built-in: `SequentialNotificationPublisher`, `ConcurrentNotificationPublisher` |
| `IMediator.Publish()`        | Publish notifications                             | Both        | `await _mediator.Publish(new OrderCreatedNotification(...))`                   |
| `IMediator.Subscribe()`      | Manual subscription                               | Pattern 1   | `_mediator.Subscribe(emailSubscriber)`                                         |
| `IMediator.Unsubscribe()`    | Manual unsubscription                             | Pattern 1   | `_mediator.Unsubscribe(emailSubscriber)`                                       |

### Middleware Types

The notification middleware pipeline supports three distinct types of middleware, each providing different levels of type safety and processing scope for comprehensive cross-cutting concerns.

| **Middleware Type**     | **Interface**                                          | **Scope**                   | **Use Case**               | **Example**                         |
| ----------------------- | ------------------------------------------------------ | --------------------------- | -------------------------- | ----------------------------------- |
| **Standard Generic**    | `INotificationMiddleware<INotification>`               | All notifications           | Cross-cutting concerns     | Logging, validation, metrics        |
| **Type-Constrained**    | `INotificationMiddleware<IOrderNotification>`          | Specific interface types    | Domain-specific processing | Order auditing, customer validation |
| **Generic Constraints** | `where TNotification : INotification, IAuditableEvent` | Complex type requirements   | Advanced scenarios         | Security validation, compliance     |
| **Non-Generic**         | `INotificationMiddleware`                              | All notifications (untyped) | Basic processing           | Simple logging, error handling      |

### Configuration Options

Choose the appropriate configuration method based on your application's complexity and requirements.

| **Configuration Method**                                                                   | **Pattern** | **Registration Style**                  | **Use Case**                            |
| ------------------------------------------------------------------------------------------ | ----------- | --------------------------------------- | --------------------------------------- |
| `AddMediator()`                                                                            | Both        | Source-generated compile-time           | All applications (recommended)          |
| `AddMediator(config, o => o.NotificationPublisher = NotificationPublisherType.Concurrent)` | Both        | Runtime publisher strategy + source gen | Parallel handler execution              |
| `AddMediator(config => { config.WithStats... })`                                           | Both        | Runtime options + source gen            | When statistics/telemetry needed        |
| `services.AddScoped<INotificationSubscriber<T>>()`                                         | Pattern 1   | Manual subscriber registration          | Client applications (Blazor WASM, MAUI) |
| `mediator.Subscribe(subscriber)`                                                           | Pattern 1   | Runtime subscription                    | Dynamic scenarios                       |

#### MediatorOptions

Passed as an optional `Action<MediatorOptions>` to any `AddMediator()` overload to tune low-level runtime behaviour:

```csharp
builder.Services.AddMediator(config, options =>
{
    // Choose notification dispatch strategy (default: Sequential)
    options.NotificationPublisher = NotificationPublisherType.Concurrent;

    // DI service lifetime for handlers, middleware, and IMediator (default: Singleton)
    options.ServiceLifetime = ServiceLifetime.Scoped;

    // Eagerly or lazily initialise handler wrappers (default: Eager)
    options.CachingMode = CachingMode.Lazy;
});
```

| **Option**                  | **Type**                    | **Default**  | **Description**                                             |
| --------------------------- | --------------------------- | ------------ | ----------------------------------------------------------- |
| `NotificationPublisher`     | `NotificationPublisherType` | `Sequential` | Sequential or concurrent handler dispatch strategy          |
| `ServiceLifetime`           | `ServiceLifetime`           | `Singleton`  | DI lifetime for handlers, middleware, and `IMediator`       |
| `CachingMode`               | `CachingMode`               | `Eager`      | Eager or lazy initialisation of source-gen handler wrappers |
| `FrozenDictionaryThreshold` | `int`                       | `20`         | Handler count above which a `FrozenDictionary` is used      |

### Notification Publisher Strategies (v3.0.0)

v3.0.0 introduces the `INotificationPublisher` pluggable strategy that controls how `INotificationHandler<T>` handlers are invoked by the source-generated dispatch path. Two built-in strategies are provided:

| **Strategy**                      | **`NotificationPublisherType`** | **Behaviour**                                                                                                                             |
| --------------------------------- | ------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `SequentialNotificationPublisher` | `Sequential` (**default**)      | Invokes handlers one-by-one via `foreach await`. Unrolled fast-paths for 1–3 handlers.                                                    |
| `ConcurrentNotificationPublisher` | `Concurrent`                    | Starts all handlers synchronously before the first `await`, then awaits pending tasks. Throws `AggregateException` if any handler faults. |

Both strategies also have telemetry variants (`TelemetrySequentialPublisher`, `TelemetryConcurrentPublisher`) that are automatically selected when OpenTelemetry with per-handler child spans is configured.

You can also provide a **custom publisher** by registering your own `INotificationPublisher` implementation _before_ calling `AddMediator()`:

```csharp
// Custom publisher registered before AddMediator() — takes precedence over the built-in default
builder.Services.AddSingleton<INotificationPublisher, MyCustomPublisher>();
builder.Services.AddMediator(config);
```

> **Note:** The `INotificationPublisher` strategy only applies to `INotificationHandler<T>` (Pattern 2 — Automatic Handler System). Manual `INotificationSubscriber<T>` registrations (Pattern 1) are dispatched separately via the subscriber pipeline and are not affected by this setting.

### Error Handling Strategies

Robust error handling ensures notification processing continues even when individual subscribers or handlers fail, maintaining system stability and reliability.

| **Error Handling Approach**   | **Implementation**               | **Benefits**                            | **Example**                                              |
| ----------------------------- | -------------------------------- | --------------------------------------- | -------------------------------------------------------- |
| **Isolated Processing**       | Try-catch per handler/subscriber | Individual failures don't affect others | `try { await handler.Handle(); } catch { log.Error(); }` |
| **Middleware Error Handling** | Global error middleware          | Centralized error processing            | `INotificationMiddleware` with exception handling        |
| **Logging Strategy**          | Structured logging               | Comprehensive error tracking            | `_logger.LogError(ex, "Handler {HandlerType} failed")`   |
| **Circuit Breaker**           | Resilience patterns              | Prevent cascade failures                | `Polly` integration with handlers                        |
| **Dead Letter Queue**         | Failed message storage           | Retry failed notifications              | Store failed notifications for later processing          |

### Performance Considerations

Optimize notification processing performance by understanding the characteristics and trade-offs of different implementation approaches and architectural decisions.

| **Performance Factor**    | **Manual Subscribers**    | **Automatic Handlers**                                     | **Optimization Tips**                                     |
| ------------------------- | ------------------------- | ---------------------------------------------------------- | --------------------------------------------------------- |
| **Registration Overhead** | Runtime subscription cost | Compile-time registration (source gen)                     | Use automatic handlers for zero-overhead startup          |
| **Memory Usage**          | Dynamic subscriber lists  | Singleton pre-cached handler arrays                        | Unsubscribe unused manual subscribers                     |
| **Processing Speed**      | Direct method calls       | Source-gen compiled dispatch (zero DI lookups per call)    | Choose `Sequential` or `Concurrent` publisher             |
| **Concurrency**           | Thread-safe subscription  | `Sequential` (ordered) or `Concurrent` (parallel) handlers | Use `NotificationPublisherType.Concurrent` for throughput |
| **Middleware Pipeline**   | Same pipeline overhead    | Same pipeline overhead                                     | Use conditional middleware for selective processing       |

### Testing Strategies

Each notification pattern supports different testing approaches, enabling comprehensive test coverage for various scenarios and requirements.

| **Testing Approach**    | **Manual Subscribers**                  | **Automatic Handlers**                  | **Best Practices**                          |
| ----------------------- | --------------------------------------- | --------------------------------------- | ------------------------------------------- |
| **Unit Testing**        | Mock `INotificationSubscriber<T>`       | Mock `INotificationHandler<T>`          | Test handlers in isolation                  |
| **Integration Testing** | Test actual subscription/unsubscription | Test handler discovery and registration | Verify end-to-end notification flow         |
| **Middleware Testing**  | Same pipeline testing                   | Same pipeline testing                   | Test middleware in isolation and integrated |
| **Performance Testing** | Measure subscription overhead           | Measure handler throughput              | Use performance counters and metrics        |
| **Mocking Strategy**    | Mock mediator subscription calls        | Mock handler dependencies               | Use dependency injection for testability    |

## Pattern 1: Manual Subscriber System (Observer Pattern)

The Manual Subscriber System provides runtime control over notification subscription and unsubscription. This pattern follows the classic Observer Pattern where subscribers explicitly register their interest in specific notification types. The system offers maximum flexibility for dynamic scenarios where subscription state needs to change based on runtime conditions.

### Core Components

#### Notification Interface

All notifications must implement the `INotification` marker interface:

```csharp
public interface INotification
{
    // Marker interface - no methods required
}
```

#### Notification Subscriber Interface

Subscribers implement `INotificationSubscriber<TNotification>` for type-specific subscriptions, or the non-generic `INotificationSubscriber` to receive all notifications:

```csharp
// Type-specific subscriber — receives only notifications of TNotification
public interface INotificationSubscriber<in TNotification> where TNotification : INotification
{
    Task OnNotification(TNotification notification, CancellationToken cancellationToken = default);
}

// Generic (broadcast) subscriber — receives every published notification
public interface INotificationSubscriber
{
    Task OnNotification(INotification notification, CancellationToken cancellationToken = default);
}
```

> **Note:** `INotificationSubscriber<T>` and `INotificationSubscriber` are independent interfaces.
> The typed variant does **not** inherit from the non-generic one.

### Implementation Examples

These practical examples demonstrate how to implement the Manual Subscriber System using real-world business scenarios with .NET Core and Entity Framework. Each example shows complete working code that you can adapt to your own applications.

#### Basic Notification Definition

Notification classes carry data about business events and should contain all the information subscribers need to process the event effectively.

```csharp
// Define a notification for order creation events
public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }
    public DateTime OrderDate { get; }

    public OrderCreatedNotification(int orderId, string customerEmail, decimal totalAmount, DateTime orderDate)
    {
        OrderId = orderId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        OrderDate = orderDate;
    }
}

// Define a notification for user registration events
public class UserRegisteredNotification : INotification
{
    public int UserId { get; }
    public string Email { get; }
    public string UserName { get; }
    public DateTime RegistrationDate { get; }

    public UserRegisteredNotification(int userId, string email, string userName, DateTime registrationDate)
    {
        UserId = userId;
        Email = email;
        UserName = userName;
        RegistrationDate = registrationDate;
    }
}
```

#### Single Notification Subscriber

Subscribers that focus on handling a single notification type are ideal when you need focused, specific processing for individual event types.

```csharp
// Subscriber that handles order confirmation emails
public class OrderEmailNotificationSubscriber : INotificationSubscriber<OrderCreatedNotification>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderEmailNotificationSubscriber> _logger;

    public OrderEmailNotificationSubscriber(
        IEmailService emailService,
        ILogger<OrderEmailNotificationSubscriber> logger)
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

            _logger.LogInformation(
                "✅ Order confirmation email sent for order {OrderId} to {CustomerEmail}",
                notification.OrderId, notification.CustomerEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to send order confirmation email for order {OrderId}",
                notification.OrderId);
            // Don't rethrow - we don't want to fail other subscribers
        }
    }
}
```

#### Multi-Notification Subscriber

Subscribers that handle multiple notification types by implementing multiple interfaces are useful when you need to apply the same processing logic across different event types, such as auditing or logging.

```csharp
// Subscriber that handles multiple notification types for auditing
public class AuditNotificationSubscriber :
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<UserRegisteredNotification>
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditNotificationSubscriber> _logger;

    public AuditNotificationSubscriber(
        IAuditService auditService,
        ILogger<AuditNotificationSubscriber> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await _auditService.LogEventAsync(
            "OrderCreated",
            notification.OrderId,
            new { notification.CustomerEmail, notification.TotalAmount, notification.OrderDate },
            cancellationToken);

        _logger.LogInformation("📋 Audit logged for order creation {OrderId}", notification.OrderId);
    }

    public async Task OnNotification(UserRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        await _auditService.LogEventAsync(
            "UserRegistered",
            notification.UserId,
            new { notification.Email, notification.UserName, notification.RegistrationDate },
            cancellationToken);

        _logger.LogInformation("📋 Audit logged for user registration {UserId}", notification.UserId);
    }
}
```

### Service Registration and Configuration

Configure the Manual Subscriber System in your application's dependency injection container and set up the necessary services for notification processing.

#### Basic Registration

With v3.0.0, the source generator auto-discovers all subscribers and middleware at compile time:

```csharp
// Program.cs — source generator discovers all subscribers automatically
builder.Services.AddMediator();

// Manually register specific subscribers if needed
builder.Services.AddScoped<INotificationSubscriber<OrderCreatedNotification>, OrderEmailNotificationSubscriber>();
builder.Services.AddScoped<INotificationSubscriber<UserRegisteredNotification>, AuditNotificationSubscriber>();
```

#### Advanced Registration with Configuration

With v3.0.0, notification middleware is auto-discovered — only runtime options need configuration:

```csharp
// Program.cs — runtime configuration only; middleware is auto-discovered by source generator
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking();
    // Notification middleware is auto-discovered at compile time
});
```

### Publishing Notifications with Subscribe / Unsubscribe Lifecycle

The defining characteristic of Pattern 1 is that subscribers control their own lifecycle — they explicitly subscribe when they become active and unsubscribe when they are disposed. The publisher simply calls `IMediator.Publish()` without any knowledge of who is listening.

#### In a Blazor Component

Blazor components are a primary use case for Pattern 1. Subscribe during `OnInitialized` and unsubscribe in `Dispose()` to match the component lifecycle:

```csharp
@implements IDisposable
@inject IMediator Mediator
@inject ILogger<CheckoutPage> Logger
@inject OrderEmailNotificationSubscriber EmailSubscriber

<button @onclick="PlaceOrderAsync">Place Order</button>

@code {
    protected override void OnInitialized()
    {
        // Subscribe when the component becomes active
        Mediator.Subscribe(EmailSubscriber);
    }

    private async Task PlaceOrderAsync()
    {
        // ... local state / validation ...

        // Publish — all currently subscribed Pattern 1 subscribers are notified
        await Mediator.Publish(new OrderCreatedNotification(
            orderId: 42,
            customerEmail: "user@example.com",
            totalAmount: 99.99m,
            orderDate: DateTime.UtcNow));

        Logger.LogInformation("📢 OrderCreatedNotification published");
    }

    public void Dispose()
    {
        // Unsubscribe when the component is torn down — no memory leaks
        Mediator.Unsubscribe(EmailSubscriber);
    }
}
```

#### In a ViewModel (MAUI / WPF / WinForms)

ViewModels in desktop and mobile applications manage subscription lifetime in their constructor and `Dispose()` method:

```csharp
public class CheckoutViewModel : IDisposable
{
    private readonly IMediator _mediator;
    private readonly ILogger<CheckoutViewModel> _logger;
    private readonly OrderEmailNotificationSubscriber _emailSubscriber;
    private readonly AuditNotificationSubscriber _auditSubscriber;

    public CheckoutViewModel(
        IMediator mediator,
        ILogger<CheckoutViewModel> logger,
        OrderEmailNotificationSubscriber emailSubscriber,
        AuditNotificationSubscriber auditSubscriber)
    {
        _mediator = mediator;
        _logger = logger;
        _emailSubscriber = emailSubscriber;
        _auditSubscriber = auditSubscriber;

        // Subscribe when the ViewModel becomes active
        _mediator.Subscribe(_emailSubscriber);
        _mediator.Subscribe(_auditSubscriber);
    }

    public async Task PlaceOrderAsync(string customerEmail, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        // ... local validation / state update ...

        // Publish — currently subscribed handlers are notified
        await _mediator.Publish(new OrderCreatedNotification(
            orderId: 42,
            customerEmail: customerEmail,
            totalAmount: totalAmount,
            orderDate: DateTime.UtcNow), cancellationToken);

        _logger.LogInformation("📢 OrderCreatedNotification published from CheckoutViewModel");
    }

    public void Dispose()
    {
        // Unsubscribe when the ViewModel is destroyed
        _mediator.Unsubscribe(_emailSubscriber);
        _mediator.Unsubscribe(_auditSubscriber);
    }
}
```

### Pattern 1 Benefits and Use Cases

The Manual Subscriber System provides key advantages in scenarios requiring runtime flexibility and explicit control over notification processing.

**Benefits:**

- **Runtime Flexibility**: Subscribe and unsubscribe dynamically based on conditions
- **Explicit Control**: Clear, intentional subscription management
- **Testability**: Easy to mock and test individual subscribers
- **Loose Coupling**: Publishers don't know about specific subscribers
- **Error Isolation**: Failed subscribers don't affect others

**Ideal Use Cases:**

- **Dynamic Subscription Requirements**: When subscription state changes based on user preferences, application state, or business rules
- **Client Applications**: Desktop (WPF, WinForms), mobile (MAUI), and web client applications (Blazor WebAssembly)
- **Interactive Scenarios**: Real-time notifications, user interface updates, event-driven workflows
- **Conditional Processing**: When notifications should only be processed under certain conditions
- **Plugin Architectures**: Where modules can dynamically register for specific events

### Complete Blazor Client Example

A complete Blazor WebAssembly shopping cart scenario demonstrates how multiple typed subscribers handle different aspects of an order event, with the component managing the full subscribe/unsubscribe lifecycle.

```csharp
// Shared notification (defined in a shared project, e.g. MyApp.Shared)
public class OrderPlacedNotification : INotification
{
    public int OrderId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }
    public DateTime OrderDate { get; }

    public OrderPlacedNotification(int orderId, string customerEmail, decimal totalAmount, DateTime orderDate)
    {
        OrderId = orderId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        OrderDate = orderDate;
    }
}

// Subscriber 1: updates local UI state (toast / banner)
public class OrderConfirmationUiSubscriber : INotificationSubscriber<OrderPlacedNotification>
{
    private readonly IToastService _toast;

    public OrderConfirmationUiSubscriber(IToastService toast) => _toast = toast;

    public Task OnNotification(OrderPlacedNotification notification, CancellationToken cancellationToken = default)
    {
        _toast.ShowSuccess($"✅ Order #{notification.OrderId} placed for ${notification.TotalAmount:F2}");
        return Task.CompletedTask;
    }
}

// Subscriber 2: clears the local cart state
public class CartClearSubscriber : INotificationSubscriber<OrderPlacedNotification>
{
    private readonly ICartState _cart;

    public CartClearSubscriber(ICartState cart) => _cart = cart;

    public async Task OnNotification(OrderPlacedNotification notification, CancellationToken cancellationToken = default)
    {
        await _cart.ClearAsync(cancellationToken);
    }
}

// Blazor component — manages full subscribe/unsubscribe lifecycle
@page "/checkout"
@implements IDisposable
@inject IMediator Mediator
@inject OrderConfirmationUiSubscriber UiSubscriber
@inject CartClearSubscriber CartSubscriber
@inject ILogger<CheckoutPage> Logger

<h2>Checkout</h2>
<button @onclick="PlaceOrderAsync" disabled="@_isProcessing">Place Order</button>

@code {
    private bool _isProcessing;

    protected override void OnInitialized()
    {
        // Subscribe when the page becomes active
        Mediator.Subscribe(UiSubscriber);
        Mediator.Subscribe(CartSubscriber);
    }

    private async Task PlaceOrderAsync()
    {
        _isProcessing = true;
        try
        {
            // ... call API / local logic to create the order record ...
            int newOrderId = 42; // result from API call

            // Publish — notifies all currently subscribed Pattern 1 subscribers
            await Mediator.Publish(new OrderPlacedNotification(
                orderId: newOrderId,
                customerEmail: "user@example.com",
                totalAmount: 99.99m,
                orderDate: DateTime.UtcNow));

            Logger.LogInformation("📢 OrderPlacedNotification published for order {OrderId}", newOrderId);
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public void Dispose()
    {
        // Unsubscribe when the component is torn down — prevents stale subscriptions
        Mediator.Unsubscribe(UiSubscriber);
        Mediator.Unsubscribe(CartSubscriber);
    }
}

// DI registration in Program.cs (Blazor WASM / MAUI client)
builder.Services.AddMediator();
builder.Services.AddScoped<OrderConfirmationUiSubscriber>();
builder.Services.AddScoped<CartClearSubscriber>();
```

## Pattern 2: Automatic Handler System (Publish-Subscribe Pattern)

The Automatic Handler System implements a Publish-Subscribe Pattern with compile-time handler discovery via the Roslyn source generator. Handlers are registered in DI and wired into a pre-compiled dispatch chain at build time — no runtime assembly scanning, no reflection at call time. Invoke them by simply calling `IMediator.Publish()` and the source-generated dispatcher routes the notification to all registered handlers with zero DI lookups per call.

### Core Components

The fundamental interfaces and types that make up the Automatic Handler System include the handler interface and three types of middleware for cross-cutting concerns.

#### Notification Handler Interface

The primary interface that all automatic handlers must implement to receive notifications from the mediator.

Handlers implement `INotificationHandler<T>`:

```csharp
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken = default);
}
```

> **Note:** There is no non-generic `INotificationHandler` base interface. Each handler directly implements the typed generic variant.

#### Notification Middleware Interface

The system supports three distinct types of notification middleware, each providing different levels of type safety and processing scope:

##### 1. Standard Generic Middleware

Generic middleware that processes all notifications implementing `INotification`:

```csharp
// Non-generic base interface — handles every notification in the system
public interface INotificationMiddleware
{
    // Execution order — lower numbers run first. Defaults to 0.
    int Order => 0;

    ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification;
}

// Generic (type-constrained) interface — extends the non-generic base
public interface INotificationMiddleware<TNotification> : INotificationMiddleware
    where TNotification : INotification
{
    ValueTask InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken);
}
```

##### 2. Type-Constrained Middleware

Middleware constrained to specific notification types or base interfaces:

```csharp
// Example: Middleware that only processes order-related notifications
public interface IOrderNotification : INotification
{
    int OrderId { get; }
    DateTime OrderDate { get; }
}

public class OrderNotificationMiddleware : INotificationMiddleware<IOrderNotification>
{
    public async ValueTask InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken = default)
    {
        // Process only order notifications
        await next(notification, cancellationToken);
    }
}
```

##### 3. Generic Type Constraints

Middleware with complex generic type constraints for advanced scenarios:

```csharp
// Middleware with multiple interface constraints
public class ComplexNotificationMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification, IOrderNotification
{
    public async ValueTask InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
    {
        // Process notifications that implement both INotification and IOrderNotification
        await next(notification, cancellationToken);
    }
}
```

### Implementation Examples

Comprehensive examples demonstrate how to implement the Automatic Handler System with all three middleware types, showing real-world scenarios with complete working code that integrates with .NET Core and Entity Framework.

#### Basic Notification Definitions

Notification classes and interfaces used throughout the Pattern 2 examples establish a consistent foundation for all handler demonstrations.

```csharp
// Base order notification interface
public interface IOrderNotification : INotification
{
    int OrderId { get; }
    DateTime OrderDate { get; }
}

// Specific order notifications
public class OrderCreatedNotification : IOrderNotification
{
    public int OrderId { get; }
    public DateTime OrderDate { get; }
    public int CustomerId { get; }
    public decimal TotalAmount { get; }
    public List<OrderItem> Items { get; }

    public OrderCreatedNotification(int orderId, int customerId, decimal totalAmount, List<OrderItem> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Items = items ?? new List<OrderItem>();
        OrderDate = DateTime.UtcNow;
    }
}

public class OrderShippedNotification : IOrderNotification
{
    public int OrderId { get; }
    public DateTime OrderDate { get; }
    public string TrackingNumber { get; }
    public DateTime ShippedDate { get; }
    public string ShippingCarrier { get; }

    public OrderShippedNotification(int orderId, DateTime orderDate, string trackingNumber, string shippingCarrier)
    {
        OrderId = orderId;
        OrderDate = orderDate;
        TrackingNumber = trackingNumber;
        ShippingCarrier = shippingCarrier;
        ShippedDate = DateTime.UtcNow;
    }
}

public class UserRegisteredNotification : INotification
{
    public int UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime RegistrationDate { get; }

    public UserRegisteredNotification(int userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        RegistrationDate = DateTime.UtcNow;
    }
}
```

### Pattern 2 Handler Examples

Three complete example sets demonstrate different middleware types with their corresponding notification handlers, showing how each approach handles type safety and processing scope differently.

#### Example Set 1: Standard Generic Middleware

Middleware that processes all notifications in the system is ideal for cross-cutting concerns like logging, validation, and performance monitoring that should apply to every notification type.

**Notification Handler:**

Clean, focused processing of specific notification types with proper dependency injection and error handling patterns.

```csharp
// Handler for order created events
public class OrderCreatedHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(
        IOrderRepository orderRepository,
        IInventoryService inventoryService,
        ILogger<OrderCreatedHandler> logger)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order created event for Order {OrderId}", notification.OrderId);

        // Reserve inventory for order items
        foreach (var item in notification.Items)
        {
            await _inventoryService.ReserveInventoryAsync(item.ProductId, item.Quantity, cancellationToken);
        }

        // Update order status
        await _orderRepository.UpdateOrderStatusAsync(notification.OrderId, OrderStatus.Confirmed, cancellationToken);

        _logger.LogInformation("Order {OrderId} processing completed successfully", notification.OrderId);
    }
}

// Handler for user registration events
public class UserRegisteredHandler : INotificationHandler<UserRegisteredNotification>
{
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserRegisteredHandler> _logger;

    public UserRegisteredHandler(
        IEmailService emailService,
        IUserRepository userRepository,
        ILogger<UserRegisteredHandler> logger)
    {
        _emailService = emailService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async ValueTask Handle(UserRegisteredNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing user registration for User {UserId}", notification.UserId);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(notification.Email, notification.FirstName, cancellationToken);

        // Update user status
        await _userRepository.UpdateUserStatusAsync(notification.UserId, UserStatus.Active, cancellationToken);

        _logger.LogInformation("User {UserId} registration processing completed", notification.UserId);
    }
}
```

**Standard Generic Middleware:**

Cross-cutting concerns that apply to all notifications can be implemented using both the generic and non-generic interfaces for maximum flexibility.

```csharp
// Middleware that processes ALL notifications in the system
public class LoggingNotificationMiddleware : INotificationMiddleware<INotification>
{
    private readonly ILogger<LoggingNotificationMiddleware> _logger;

    public LoggingNotificationMiddleware(ILogger<LoggingNotificationMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync(INotification notification, NotificationDelegate<INotification> next, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType().Name;
        _logger.LogInformation("Processing notification: {NotificationType}", notificationType);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(notification, cancellationToken);
            stopwatch.Stop();
            _logger.LogInformation("Notification {NotificationType} processed successfully in {ElapsedMs}ms",
                notificationType, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing notification {NotificationType} after {ElapsedMs}ms",
                notificationType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

// Non-generic middleware alternative
public class ValidationNotificationMiddleware : INotificationMiddleware
{
    private readonly ILogger<ValidationNotificationMiddleware> _logger;

    public ValidationNotificationMiddleware(ILogger<ValidationNotificationMiddleware> logger)
    {
        _logger = logger;
    }

    public async ValueTask InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        // Basic validation for all notifications
        if (notification == null)
        {
            _logger.LogError("Received null notification");
            throw new ArgumentNullException(nameof(notification));
        }

        var notificationType = notification.GetType();
        _logger.LogDebug("Validating notification: {NotificationType}", notificationType.Name);

        // Perform basic validation
        var validationResults = ValidateNotification(notification);
        if (validationResults.Any())
        {
            _logger.LogWarning("Validation failed for {NotificationType}: {ValidationErrors}",
                notificationType.Name, string.Join(", ", validationResults));
            throw new ValidationException($"Notification validation failed: {string.Join(", ", validationResults)}");
        }

        await next(notification, cancellationToken);
    }

    private List<string> ValidateNotification(INotification notification)
    {
        var errors = new List<string>();

        // Add common validation logic here
        var properties = notification.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(notification);
            if (property.PropertyType == typeof(string) && value == null)
            {
                errors.Add($"Property {property.Name} cannot be null");
            }
        }

        return errors;
    }
}
```

#### Example Set 2: Type-Constrained Middleware

Middleware that only processes notifications implementing specific interfaces provides type-safe, domain-focused processing for related notification groups.

**Notification Handler:**

Processing order shipping events with customer notification and proper error handling for missing entities demonstrates robust event handling patterns.

```csharp
// Handler for order shipped events
public class OrderShippedHandler : INotificationHandler<OrderShippedNotification>
{
    private readonly IEmailService _emailService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderShippedHandler> _logger;

    public OrderShippedHandler(
        IEmailService emailService,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        ILogger<OrderShippedHandler> logger)
    {
        _emailService = emailService;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async ValueTask Handle(OrderShippedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order shipped event for Order {OrderId}", notification.OrderId);

        // Get order details
        var order = await _orderRepository.GetOrderAsync(notification.OrderId, cancellationToken);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", notification.OrderId);
            return;
        }

        // Get customer details
        var customer = await _customerRepository.GetCustomerAsync(order.CustomerId, cancellationToken);
        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found for Order {OrderId}", order.CustomerId, notification.OrderId);
            return;
        }

        // Send shipping notification email
        await _emailService.SendShippingNotificationAsync(
            customer.Email,
            customer.FirstName,
            notification.OrderId,
            notification.TrackingNumber,
            notification.ShippingCarrier,
            cancellationToken);

        _logger.LogInformation("Shipping notification sent for Order {OrderId}", notification.OrderId);
    }
}
```

**Type-Constrained Middleware:**

Domain-specific processing that only applies to notifications implementing a particular interface enables targeted business logic and auditing.

```csharp
// Middleware that ONLY processes order-related notifications
public class OrderNotificationMiddleware : INotificationMiddleware<IOrderNotification>
{
    private readonly IOrderAuditService _auditService;
    private readonly ILogger<OrderNotificationMiddleware> _logger;

    public OrderNotificationMiddleware(
        IOrderAuditService auditService,
        ILogger<OrderNotificationMiddleware> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public async ValueTask InvokeAsync(IOrderNotification notification, NotificationDelegate<IOrderNotification> next, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType().Name;
        _logger.LogInformation("Processing order notification: {NotificationType} for Order {OrderId}",
            notificationType, notification.OrderId);

        // Audit order event
        await _auditService.LogOrderEventAsync(
            notification.OrderId,
            notificationType,
            notification.OrderDate,
            cancellationToken);

        // Validate order-specific business rules
        if (notification.OrderDate > DateTime.UtcNow.AddDays(1))
        {
            _logger.LogWarning("Future-dated order notification detected: {OrderId}", notification.OrderId);
        }

        try
        {
            await next(notification, cancellationToken);

            // Log successful processing
            await _auditService.LogOrderEventCompletionAsync(
                notification.OrderId,
                notificationType,
                true,
                null,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order notification {NotificationType} for Order {OrderId}",
                notificationType, notification.OrderId);

            // Log failed processing
            await _auditService.LogOrderEventCompletionAsync(
                notification.OrderId,
                notificationType,
                false,
                ex.Message,
                cancellationToken);

            throw;
        }
    }
}
```

#### Example Set 3: Generic Type Constraints

Advanced handlers and middleware with complex generic constraints enable sophisticated type safety and reusable processing logic across multiple notification types.

**Notification Handler:**

Generic handlers show how to create reusable processing logic that works with any notification meeting specific interface requirements, with concrete implementations for type safety.

```csharp
// Handler that processes any notification implementing both INotification and IOrderNotification
public class ComplexOrderHandler<TNotification> : INotificationHandler<TNotification>
    where TNotification : INotification, IOrderNotification
{
    private readonly IOrderService _orderService;
    private readonly INotificationHistoryService _historyService;
    private readonly ILogger<ComplexOrderHandler<TNotification>> _logger;

    public ComplexOrderHandler(
        IOrderService orderService,
        INotificationHistoryService historyService,
        ILogger<ComplexOrderHandler<TNotification>> logger)
    {
        _orderService = orderService;
        _historyService = historyService;
        _logger = logger;
    }

    public async ValueTask Handle(TNotification notification, CancellationToken cancellationToken = default)
    {
        var notificationType = typeof(TNotification).Name;
        _logger.LogInformation("Processing complex order notification: {NotificationType} for Order {OrderId}",
            notificationType, notification.OrderId);

        // Record notification history
        await _historyService.RecordNotificationAsync(
            notification.OrderId,
            notificationType,
            notification.OrderDate,
            cancellationToken);

        // Perform order-specific processing
        await _orderService.ProcessOrderEventAsync(notification.OrderId, notificationType, cancellationToken);

        _logger.LogInformation("Complex order notification {NotificationType} processed for Order {OrderId}",
            notificationType, notification.OrderId);
    }
}

// Concrete implementations for specific types
public class ConcreteOrderCreatedHandler : ComplexOrderHandler<OrderCreatedNotification>
{
    public ConcreteOrderCreatedHandler(
        IOrderService orderService,
        INotificationHistoryService historyService,
        ILogger<ComplexOrderHandler<OrderCreatedNotification>> logger)
        : base(orderService, historyService, logger)
    {
    }
}

public class ConcreteOrderShippedHandler : ComplexOrderHandler<OrderShippedNotification>
{
    public ConcreteOrderShippedHandler(
        IOrderService orderService,
        INotificationHistoryService historyService,
        ILogger<ComplexOrderHandler<OrderShippedNotification>> logger)
        : base(orderService, historyService, logger)
    {
    }
}
```

**Generic Type Constraints Middleware:**

Advanced middleware with multiple generic constraints implements sophisticated processing logic for complex validation, security, and monitoring scenarios while maintaining type safety.

```csharp
// Middleware with complex generic constraints
public class AdvancedOrderMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification, IOrderNotification
{
    private readonly IOrderValidationService _validationService;
    private readonly IPerformanceMonitoringService _monitoringService;
    private readonly ILogger<AdvancedOrderMiddleware<TNotification>> _logger;

    public AdvancedOrderMiddleware(
        IOrderValidationService validationService,
        IPerformanceMonitoringService monitoringService,
        ILogger<AdvancedOrderMiddleware<TNotification>> logger)
    {
        _validationService = validationService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    public async ValueTask InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
    {
        var notificationType = typeof(TNotification).Name;
        var performanceContext = _monitoringService.StartMonitoring($"OrderNotification_{notificationType}", notification.OrderId);

        _logger.LogInformation("Advanced processing for {NotificationType} - Order {OrderId}",
            notificationType, notification.OrderId);

        try
        {
            // Perform advanced order validation
            var validationResult = await _validationService.ValidateOrderNotificationAsync(
                notification.OrderId,
                notification.OrderDate,
                cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Order validation failed for {OrderId}: {ValidationErrors}",
                    notification.OrderId, string.Join(", ", validationResult.Errors));
                throw new InvalidOperationException($"Order validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Process the notification
            await next(notification, cancellationToken);

            performanceContext.RecordSuccess();
            _logger.LogInformation("Advanced processing completed for {NotificationType} - Order {OrderId}",
                notificationType, notification.OrderId);
        }
        catch (Exception ex)
        {
            performanceContext.RecordFailure(ex);
            _logger.LogError(ex, "Advanced processing failed for {NotificationType} - Order {OrderId}",
                notificationType, notification.OrderId);
            throw;
        }
        finally
        {
            performanceContext.Dispose();
        }
    }
}

// Multi-constraint middleware
public class SecurityOrderMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : class, INotification, IOrderNotification
{
    private readonly ISecurityService _securityService;
    private readonly ILogger<SecurityOrderMiddleware<TNotification>> _logger;

    public SecurityOrderMiddleware(
        ISecurityService securityService,
        ILogger<SecurityOrderMiddleware<TNotification>> logger)
    {
        _securityService = securityService;
        _logger = logger;
    }

    public async ValueTask InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
    {
        // Security validation for order notifications
        var isAuthorized = await _securityService.ValidateOrderAccessAsync(
            notification.OrderId,
            typeof(TNotification).Name,
            cancellationToken);

        if (!isAuthorized)
        {
            _logger.LogWarning("Unauthorized access attempt for Order {OrderId} with notification {NotificationType}",
                notification.OrderId, typeof(TNotification).Name);
            throw new UnauthorizedAccessException($"Unauthorized access to order {notification.OrderId}");
        }

        await next(notification, cancellationToken);
    }
}
```

### Service Registration and Configuration

Configure the Automatic Handler System in your application's dependency injection container with examples for both basic and advanced scenarios.

#### Basic Registration

With v3.0.0, the source generator auto-discovers all handlers and middleware at compile time:

```csharp
// Source generator discovers all INotificationHandler<T> implementations automatically
builder.Services.AddMediator();

// The following are automatically registered by the source generator:
// - OrderCreatedHandler
// - OrderShippedHandler
// - UserRegisteredHandler
// - All middleware implementations
```

#### Advanced Registration with Middleware

With v3.0.0, notification middleware is auto-discovered — only runtime options need configuration:

```csharp
// Program.cs — runtime configuration only; all middleware auto-discovered by source generator
builder.Services.AddMediator(config =>
{
    config.WithStatisticsTracking();
    // Notification middleware is auto-discovered at compile time
});

// Register supporting services
builder.Services.AddScoped<IOrderAuditService, OrderAuditService>();
builder.Services.AddScoped<IOrderValidationService, OrderValidationService>();
builder.Services.AddScoped<IPerformanceMonitoringService, PerformanceMonitoringService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
```

### Publishing Notifications

Publish notifications from your business logic to automatically trigger all registered handlers through the mediator without explicit subscription management.

#### In Business Logic Services

Publishing notifications from business services automatically invokes handlers through the mediator without explicit subscription management, maintaining clean separation of concerns.

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMediator _mediator;

    public OrderService(IOrderRepository orderRepository, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
    }

    public async Task<int> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        // Create the order
        var order = new Order
        {
            CustomerId = request.CustomerId,
            TotalAmount = request.TotalAmount,
            Items = request.Items,
            CreatedDate = DateTime.UtcNow
        };

        await _orderRepository.AddOrderAsync(order, cancellationToken);

        // Publish notification - handlers will be invoked automatically
        var notification = new OrderCreatedNotification(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Items);

        await _mediator.Publish(notification, cancellationToken);

        return order.Id;
    }

    public async Task ShipOrderAsync(int orderId, string trackingNumber, string carrier, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetOrderAsync(orderId, cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found");

        // Update order status
        order.Status = OrderStatus.Shipped;
        order.ShippedDate = DateTime.UtcNow;
        order.TrackingNumber = trackingNumber;

        await _orderRepository.UpdateOrderAsync(order, cancellationToken);

        // Publish notification - handlers will be invoked automatically
        var notification = new OrderShippedNotification(
            orderId,
            order.CreatedDate,
            trackingNumber,
            carrier);

        await _mediator.Publish(notification, cancellationToken);
    }
}
```

### Handler Type Comparison

Choose the right middleware type for your specific requirements by understanding the key differences in scope, use cases, type safety, and registration approaches.

#### Standard Generic Middleware

- **Scope**: Processes ALL notifications in the system
- **Use Case**: Cross-cutting concerns like logging, validation, performance monitoring
- **Type Safety**: Works with `INotification` base interface
- **Registration**: `INotificationMiddleware<INotification>` or `INotificationMiddleware`

#### Type-Constrained Middleware

- **Scope**: Processes notifications implementing specific interfaces or base types
- **Use Case**: Domain-specific logic for related notification groups
- **Type Safety**: Strongly typed to specific interfaces (e.g., `IOrderNotification`)
- **Registration**: `INotificationMiddleware<IOrderNotification>`

#### Generic Type Constraints

- **Scope**: Processes notifications meeting complex generic constraints
- **Use Case**: Advanced scenarios requiring multiple interface implementations
- **Type Safety**: Multiple generic constraints with `where` clauses
- **Registration**: Open generic types like `typeof(AdvancedOrderMiddleware<>)`

### Pattern 2 Benefits and Use Cases

Key advantages of the Automatic Handler System and scenarios where convention-based approaches provide the most value for enterprise and server applications.

**Benefits:**

- **Zero Configuration**: Handlers are discovered and registered automatically
- **Convention-Based**: Follows standard DI patterns and conventions
- **Compile-Time Safety**: Strongly typed handler interfaces with compile-time checking
- **Separation of Concerns**: Each handler focuses on a single notification type
- **Testability**: Easy to unit test individual handlers in isolation
- **Scalability**: Automatic registration scales with application growth
- **Middleware Integration**: Seamless integration with sophisticated middleware pipeline
- **Multiple Handler Support**: Multiple handlers can process the same notification type

**Ideal Use Cases:**

- **Server Applications**: ASP.NET Core, Web API, Blazor Server, microservices
- **Background Services**: Worker services, scheduled tasks, message processors
- **Event-Driven Architecture**: Domain events, integration events, system events
- **CQRS Implementation**: Command and query responsibility separation
- **Clean Architecture**: Domain layer event handling and cross-cutting concerns

### Complete ECommerce Example

A complete ASP.NET Core Web API e-commerce order workflow demonstrating Pattern 2's compile-time handler discovery. Multiple `INotificationHandler<T>` implementations are automatically discovered and wired by the source generator — the API endpoint simply publishes; no subscription management required.

```csharp
// ── Notifications ─────────────────────────────────────────────────────────────

public class OrderCreatedNotification : INotification
{
    public int OrderId { get; }
    public string CustomerEmail { get; }
    public decimal TotalAmount { get; }
    public List<OrderItem> Items { get; }

    public OrderCreatedNotification(int orderId, string customerEmail, decimal totalAmount, List<OrderItem> items)
    {
        OrderId = orderId;
        CustomerEmail = customerEmail;
        TotalAmount = totalAmount;
        Items = items;
    }
}

public class OrderShippedNotification : INotification
{
    public int OrderId { get; }
    public string TrackingNumber { get; }
    public string Carrier { get; }

    public OrderShippedNotification(int orderId, string trackingNumber, string carrier)
    {
        OrderId = orderId;
        TrackingNumber = trackingNumber;
        Carrier = carrier;
    }
}

// ── Handlers for OrderCreatedNotification (auto-discovered at compile time) ────

// Sends order confirmation email
public class OrderConfirmationEmailHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IEmailService _email;
    private readonly ILogger<OrderConfirmationEmailHandler> _logger;

    public OrderConfirmationEmailHandler(IEmailService email, ILogger<OrderConfirmationEmailHandler> logger)
    {
        _email = email;
        _logger = logger;
    }

    public async ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await _email.SendOrderConfirmationAsync(
            notification.CustomerEmail, notification.OrderId, notification.TotalAmount, cancellationToken);

        _logger.LogInformation("📧 Confirmation email sent for order {OrderId}", notification.OrderId);
    }
}

// Reserves inventory for all order items
public class InventoryReservationHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IInventoryService _inventory;
    private readonly ILogger<InventoryReservationHandler> _logger;

    public InventoryReservationHandler(IInventoryService inventory, ILogger<InventoryReservationHandler> logger)
    {
        _inventory = inventory;
        _logger = logger;
    }

    public async ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        foreach (var item in notification.Items)
            await _inventory.ReserveAsync(item.ProductId, item.Quantity, cancellationToken);

        _logger.LogInformation("📦 Inventory reserved for order {OrderId} ({Count} lines)",
            notification.OrderId, notification.Items.Count);
    }
}

// Writes an audit record
public class OrderAuditHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly IAuditService _audit;

    public OrderAuditHandler(IAuditService audit) => _audit = audit;

    public async ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await _audit.RecordAsync("OrderCreated", notification.OrderId,
            new { notification.CustomerEmail, notification.TotalAmount }, cancellationToken);
    }
}

// ── Handler for OrderShippedNotification ──────────────────────────────────────

public class ShipmentEmailHandler : INotificationHandler<OrderShippedNotification>
{
    private readonly IEmailService _email;
    private readonly ILogger<ShipmentEmailHandler> _logger;

    public ShipmentEmailHandler(IEmailService email, ILogger<ShipmentEmailHandler> logger)
    {
        _email = email;
        _logger = logger;
    }

    public async ValueTask Handle(OrderShippedNotification notification, CancellationToken cancellationToken = default)
    {
        await _email.SendShipmentNotificationAsync(
            notification.OrderId, notification.TrackingNumber, notification.Carrier, cancellationToken);

        _logger.LogInformation("🚚 Shipment notification sent for order {OrderId} via {Carrier}",
            notification.OrderId, notification.Carrier);
    }
}

// ── CQRS command handler — publishes after persisting ─────────────────────────

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _orders;
    private readonly IMediator _mediator;

    public CreateOrderCommandHandler(IOrderRepository orders, IMediator mediator)
    {
        _orders = orders;
        _mediator = mediator;
    }

    public async ValueTask<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(command.CustomerEmail, command.Items);
        await _orders.AddAsync(order, cancellationToken);
        await _orders.SaveChangesAsync(cancellationToken);

        // Publish — source-gen dispatcher routes to all three handlers above automatically
        await _mediator.Publish(new OrderCreatedNotification(
            order.Id, order.CustomerEmail, order.TotalAmount, order.Items), cancellationToken);

        return order.Id;
    }
}

// ── ASP.NET Core Minimal API endpoint ─────────────────────────────────────────

app.MapPost("/orders", async (
    CreateOrderRequest req,
    IMediator mediator,
    CancellationToken ct) =>
{
    // Send CQRS command — handler persists order and publishes notification
    var orderId = await mediator.Send(new CreateOrderCommand(req.CustomerEmail, req.Items), ct);
    return Results.Created($"/orders/{orderId}", new { orderId });
});

// ── Program.cs registration ───────────────────────────────────────────────────

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// AddMediator() — source generator has already discovered and wired all three
// OrderCreatedNotification handlers and the ShipmentEmailHandler at compile time.
// No explicit handler registration required.
builder.Services.AddMediator();
```
