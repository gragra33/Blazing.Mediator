# Migration Guide — Blazing.Mediator v2.0.1 → v3.0.0

This guide walks through every change required to migrate a project from Blazing.Mediator v2.0.1 to v3.0.0. Code examples are drawn from the bundled sample projects.

For a concise list of breaking changes, see [BREAKING_CHANGES.md](BREAKING_CHANGES.md).

---

## Overview of what changed

| Area                                | v2.0.1                                           | v3.0.0                                                                  |
| ----------------------------------- | ------------------------------------------------ | ----------------------------------------------------------------------- |
| Handler return types                | `Task` / `Task<T>`                               | `ValueTask` / `ValueTask<T>`                                            |
| Notification handler return type    | `Task`                                           | `ValueTask`                                                             |
| Middleware return types             | `Task` / `Task<T>`                               | `ValueTask` / `ValueTask<T>`                                            |
| `IMediator.Send` / `Publish`        | `Task` / `Task<T>`                               | `ValueTask` / `ValueTask<T>`                                            |
| `#if USE_SOURCE_GENERATORS` guards  | Required in consuming projects                   | Removed — no longer needed                                              |
| Dispatch implementation             | Runtime reflection — handler resolved at runtime | Compile-time source generation — fully typed, zero-alloc dispatch table |
| Middleware registration             | Explicit `config.AddMiddleware(typeof(...))`     | Auto-discovered by source generator                                     |
| `AddMediator()` overload signatures | Library-shipped extension methods                | Source-generator emitted                                                |
| Default service lifetime            | Scoped                                           | Singleton                                                               |
| Notification publishers             | Sequential hard-coded                            | Pluggable `INotificationPublisher`                                      |

---

## Step 1 — Update NuGet package references

```xml
<!-- v2.0.1 — old -->
<PackageReference Include="Blazing.Mediator" Version="2.0.1" />

<!-- v3.0.0 — new -->
<PackageReference Include="Blazing.Mediator" Version="3.0.0" />
<PackageReference Include="Blazing.Mediator.SourceGenerators" Version="3.0.0" />
```

Both packages are required. v3.0.0 stripped all runtime reflection from the dispatch pipeline and replaced it with compile-time source generation — there is no fallback path. Without `Blazing.Mediator.SourceGenerators`, any call to `Send`, `Publish`, or `SendStream` throws `InvalidOperationException`. The generator activates automatically once referenced — no `#if USE_SOURCE_GENERATORS` is needed.

---

## Step 2 — Remove `#if USE_SOURCE_GENERATORS` preprocessor blocks

Remove the `<DefineConstants>` line from your `.csproj` and delete all `#if USE_SOURCE_GENERATORS` / `#endif` guards.

```xml
<!-- v2.0.1 — delete this -->
<DefineConstants>USE_SOURCE_GENERATORS</DefineConstants>
```

---

## Step 3 — Update DI registration

### Basic registration (no options)

```csharp
// v2.0.1 — old
services.AddMediator(typeof(MyHandler).Assembly);

// v3.0.0 — new
services.AddMediator(); // source generator discovers handlers automatically
```

### Registration with configuration

In v2.0.1, middleware had to be registered explicitly inside the `AddMediator` callback. In v3.0.0 the source generator discovers all middleware types at compile time — you only configure options.

```csharp
// v2.0.1 — old (from ECommerce.Api)
services.AddMediator(config =>
{
    config.WithStatisticsTracking();

    // Middleware had to be listed manually
    config.AddMiddleware(typeof(StatisticsTrackingMiddleware<,>));
    config.AddMiddleware(typeof(StatisticsTrackingVoidMiddleware<>));
    config.AddMiddleware(typeof(OrderLoggingMiddleware<,>));
    config.AddMiddleware(typeof(ProductLoggingMiddleware<,>));
    config.AddNotificationMiddleware<NotificationLoggingMiddleware>();
    config.AddNotificationMiddleware<NotificationMetricsMiddleware>();
}, typeof(ServiceCollectionExtensions).Assembly);

// v3.0.0 — new (from ECommerce.Api)
var mediatorConfig = new MediatorConfiguration();
mediatorConfig.WithStatisticsTracking();
// Middleware is auto-discovered by the source generator at compile time — no manual registration
services.AddMediator(mediatorConfig);
```

### Registration with environment-aware presets

The static presets work the same way in both versions:

```csharp
// v3.0.0 — unchanged API
services.AddMediator(MediatorConfiguration.Development());
services.AddMediator(MediatorConfiguration.Production());
services.AddMediator(MediatorConfiguration.Minimal());
```

### Registration with notification handler discovery (MiddlewareExample pattern)

```csharp
// v2.0.1 — old
mediatorConfig.WithStatisticsTracking(...)
    .WithMiddlewareDiscovery();
services.AddMediator(mediatorConfig);

// v3.0.0 — new: notification handler discovery is a first-class option
var mediatorConfig = new MediatorConfiguration();
mediatorConfig
    .WithStatisticsTracking(options =>
    {
        options.EnableRequestMetrics = true;
        options.EnableNotificationMetrics = true;
        options.EnableMiddlewareMetrics = true;
        options.EnablePerformanceCounters = true;
        options.EnableDetailedAnalysis = true;
        options.MetricsRetentionPeriod = TimeSpan.FromHours(1);
        options.CleanupInterval = TimeSpan.FromMinutes(15);
    })
    .WithMiddlewareDiscovery()
    .WithNotificationHandlerDiscovery()  // new: auto-discovers INotificationHandler<T>
    .WithoutLogging()
    .WithoutTelemetry();
services.AddMediator(mediatorConfig);
```

---

## Step 4 — Update request handlers

Change the return type of `Handle` from `Task<T>` / `Task` to `ValueTask<T>` / `ValueTask`. No other change is needed — the method signature, constructor injection, and business logic remain as-is.

### Response handler (from ECommerce.Api `CreateOrderHandler`)

```csharp
// v2.0.1 — old
public class CreateOrderHandler(ECommerceDbContext context, IValidator<CreateOrderCommand> validator, IMediator mediator)
    : IRequestHandler<CreateOrderCommand, OperationResult<int>>
{
    public async Task<OperationResult<int>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return OperationResult<int>.ErrorResult("Validation failed", ...);

        // ... create order, save to db
        await context.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new OrderCreatedNotification(...), cancellationToken);
        return OperationResult<int>.SuccessResult(order.Id, "Order created");
    }
}

// v3.0.0 — new (only the return type changes)
public class CreateOrderHandler(ECommerceDbContext context, IValidator<CreateOrderCommand> validator, IMediator mediator)
    : IRequestHandler<CreateOrderCommand, OperationResult<int>>
{
    public async ValueTask<OperationResult<int>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return OperationResult<int>.ErrorResult("Validation failed", ...);

        // ... create order, save to db (unchanged)
        await context.SaveChangesAsync(cancellationToken);
        await mediator.Publish(new OrderCreatedNotification(...), cancellationToken);
        return OperationResult<int>.SuccessResult(order.Id, "Order created");
    }
}
```

### Query handler (from ECommerce.Api `GetProductByIdHandler`)

```csharp
// v2.0.1 — old
public class GetProductByIdHandler(ECommerceDbContext context) : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        return product?.ToDto() ?? throw new InvalidOperationException("Product not found");
    }
}

// v3.0.0 — new
public class GetProductByIdHandler(ECommerceDbContext context) : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async ValueTask<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        return product?.ToDto() ?? throw new InvalidOperationException("Product not found");
    }
}
```

### Void / command handler

```csharp
// v2.0.1 — old
public class DeleteProductHandler(ECommerceDbContext context) : ICommandHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product is null) return;
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}

// v3.0.0 — new
public class DeleteProductHandler(ECommerceDbContext context) : ICommandHandler<DeleteProductCommand>
{
    public async ValueTask Handle(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        var product = await context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product is null) return;
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## Step 5 — Update notification handlers

`INotificationHandler<T>.Handle` is also now `ValueTask`.

```csharp
// v2.0.1 — old
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    : INotificationHandler<OrderCreatedNotification>
{
    public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[EMAIL] Order confirmation sent for order {OrderId}", notification.OrderId);
    }
}

// v3.0.0 — new (from NotificationHandlerExample)
public class EmailNotificationHandler(ILogger<EmailNotificationHandler> logger)
    : INotificationHandler<OrderCreatedNotification>
{
    public async ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[EMAIL] Order confirmation sent for order {OrderId}", notification.OrderId);
    }
}
```

> `INotificationSubscriber<T>.OnNotification` is **unchanged** — it still returns `Task`. Manual subscribe/unsubscribe registration is identical to v2.0.1.

### Notification handler discovery (no manual subscription needed)

```csharp
// v2.0.1 — old: handlers had to be subscribed manually
var emailHandler = scope.ServiceProvider.GetRequiredService<EmailNotificationHandler>();
mediator.Subscribe(emailHandler);

// v3.0.0 — new: the source generator discovers all INotificationHandler<T> implementations
// at compile time. No Subscribe() calls or WithNotificationHandlerDiscovery() needed.
services.AddMediator();
```

---

## Step 6 — Update middleware

Change `Task` / `Task<T>` to `ValueTask` / `ValueTask<T>` on `HandleAsync` and `InvokeAsync`. Everything else stays the same.

### Request middleware (from MiddlewareExample `ValidationMiddleware`)

```csharp
// v2.0.1 — old
public class ValidationMiddleware<TRequest, TResponse>(IServiceProvider sp, ILogger logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 100;

    public async Task<TResponse> HandleAsync(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);
        return await next();
    }
}

// v3.0.0 — new (from MiddlewareExample)
[Order(100)]
public class ValidationMiddleware<TRequest, TResponse>(IServiceProvider sp, ILogger logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await ValidateRequestAsync(request, cancellationToken);
        return await next();
    }
}
```

### Void middleware variant

```csharp
// v2.0.1 — old
public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
{
    await ValidateRequestAsync(request, cancellationToken);
    await next();
}

// v3.0.0 — new
public async ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
{
    await ValidateRequestAsync(request, cancellationToken);
    await next();
}
```

### Statistics tracking middleware (from ECommerce.Api)

```csharp
// v2.0.1 — old
public class StatisticsTrackingMiddleware<TRequest, TResponse>(
    MediatorStatisticsTracker statisticsTracker, IHttpContextAccessor httpContextAccessor)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public async Task<TResponse> HandleAsync(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        if (IsQuery(request)) statisticsTracker.TrackQuery(typeof(TRequest), sessionId);
        else statisticsTracker.TrackCommand(typeof(TRequest), sessionId);
        return await next();
    }
}

// v3.0.0 — new
[Order(0)]
public class StatisticsTrackingMiddleware<TRequest, TResponse>(
    MediatorStatisticsTracker statisticsTracker, IHttpContextAccessor httpContextAccessor)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async ValueTask<TResponse> HandleAsync(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sessionId = GetSessionId();
        if (IsQuery(request)) statisticsTracker.TrackQuery(typeof(TRequest), sessionId);
        else statisticsTracker.TrackCommand(typeof(TRequest), sessionId);
        return await next();
    }
}
```

### Notification middleware

```csharp
// v2.0.1 — old
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    public int Order => 0;

    public async Task InvokeAsync<TNotification>(
        TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        _logger.LogInformation("Publishing {Type}", typeof(TNotification).Name);
        await next(notification, cancellationToken);
    }
}

// v3.0.0 — new
[Order(0)]
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    public async ValueTask InvokeAsync<TNotification>(
        TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        _logger.LogInformation("Publishing {Type}", typeof(TNotification).Name);
        await next(notification, cancellationToken);
    }
}
```

---

## Step 7 — Update call sites (optional)

`await mediator.Send(...)` and `await mediator.Publish(...)` continue to compile without changes because `ValueTask` is directly awaitable. Only update if you stored the return value as a `Task<T>`:

```csharp
// v2.0.1 — if you stored the Task explicitly (uncommon)
Task<ProductDto> task = mediator.Send(query, ct);
ProductDto result = await task;

// v3.0.0 — change to ValueTask<T>
ValueTask<ProductDto> task = mediator.Send(query, ct);
ProductDto result = await task;

// or simply await directly (works in both versions)
ProductDto result = await mediator.Send(query, ct);
```

---

## Step 8 — Opt in to new capabilities (optional)

### Concurrent notification publishing

```csharp
// v3.0.0 — new capability: publish to all handlers concurrently
var config = new MediatorConfiguration();
config.WithConcurrentNotificationPublisher();
services.AddMediator(config);
```

### JSON configuration binding

```csharp
// v3.0.0 — bind MediatorConfiguration from appsettings.json
var config = new MediatorConfiguration()
    .WithConfiguration(builder.Configuration.GetSection("Mediator"));
services.AddMediator(config);
```

### Exclude a handler from auto-discovery

```csharp
// v3.0.0 — new: opt a handler out of source-generator discovery
[ExcludeFromAutoDiscovery]
public class InternalTestHandler : IRequestHandler<InternalTestRequest, string>
{
    public ValueTask<string> Handle(InternalTestRequest request, CancellationToken cancellationToken = default)
        => ValueTask.FromResult("test");
}
```

---

## Quick reference — find and replace

The following patterns cover 95%+ of all changes needed. Apply them across your project with a global find-and-replace:

| Find                                                       | Replace                             | Scope                             |
| ---------------------------------------------------------- | ----------------------------------- | --------------------------------- |
| `async Task<` in handler/middleware files                  | `async ValueTask<`                  | Handler and middleware files only |
| `async Task Handle(`                                       | `async ValueTask Handle(`           | Notification handlers             |
| `Task<TResponse> HandleAsync(`                             | `ValueTask<TResponse> HandleAsync(` | Middleware files                  |
| `Task HandleAsync(`                                        | `ValueTask HandleAsync(`            | Middleware files (void)           |
| `Task InvokeAsync<`                                        | `ValueTask InvokeAsync<`            | Notification middleware           |
| `Task<TResponse> Handle(`                                  | `ValueTask<TResponse> Handle(`      | Request handlers                  |
| `Task Handle(`                                             | `ValueTask Handle(`                 | Notification and void handlers    |
| `config.AddMiddleware(typeof`                              | _(delete line)_                     | DI registration only              |
| `config.AddNotificationMiddleware<`                        | _(delete line)_                     | DI registration only              |
| `, typeof(XYZ).Assembly);`                                 | `);`                                | `AddMediator` call sites          |
| `#if USE_SOURCE_GENERATORS`                                | _(delete block)_                    | All files                         |
| `<DefineConstants>USE_SOURCE_GENERATORS</DefineConstants>` | _(delete)_                          | `.csproj` files                   |

> Do **not** apply the `Task` → `ValueTask` replacement to `INotificationSubscriber<T>.OnNotification` — that interface stays `Task`.

---

# Migrating from v3.0.0 to v3.0.1

v3.0.1 is mostly additive and non-breaking, but two patterns are officially deprecated and one introduces a reserved value constraint.

---

## Change 1 — Replace `CaptureMiddlewareDetails` with `MiddlewareCaptureMode`

`TelemetryOptions.CaptureMiddlewareDetails` (bool) is marked `[Obsolete]`. Replace it with the `MiddlewareCaptureMode` enum.

```csharp
// v3.0.0 — old (still works but produces Obsolete warning)
config.WithTelemetry(options =>
{
    options.CaptureMiddlewareDetails = true;  // was: show middleware on span
    options.CaptureMiddlewareDetails = false; // was: no middleware tags
});

// v3.0.1 — new
config.WithTelemetry(options =>
{
    options.MiddlewareCaptureMode = MiddlewareCaptureMode.Applicable; // was: true  — static pipeline shape, low overhead
    options.MiddlewareCaptureMode = MiddlewareCaptureMode.None;       // was: false — zero overhead
    options.MiddlewareCaptureMode = MiddlewareCaptureMode.Executed;   // new option — full runtime tracking (dev/diagnostic only)
});
```

**Quick find-and-replace:**

| Find                               | Replace                                                    |
| ---------------------------------- | ---------------------------------------------------------- |
| `CaptureMiddlewareDetails = true`  | `MiddlewareCaptureMode = MiddlewareCaptureMode.Applicable` |
| `CaptureMiddlewareDetails = false` | `MiddlewareCaptureMode = MiddlewareCaptureMode.None`       |

---

## Change 2 — Use `[Order(n)]` attribute instead of `int Order =>` property

The source generator reads ordering from the `[Order(n)]` class attribute (compiled metadata). Overriding the `int Order { get; }` interface property only works for middleware defined in the same compilation as the generated mediator code; it is silently ignored for middleware in external assemblies.

```csharp
// v3.0.0 — works only for same-assembly middleware (may silently have wrong order in NuGet packages)
public class MyMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 10;
    // ...
}

// v3.0.1 — correct; works everywhere including cross-assembly
[Order(10)]
public class MyMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // ...
}
```

The same applies to `INotificationMiddleware`, `IStreamRequestMiddleware`, and all conditional variants.

---

## Change 3 — Do not use reserved internal order values

The built-in Blazing.Mediator middleware uses the following reserved order values. User middleware must not specify these values:

| Reserved value     | Used by                                                  |
| ------------------ | -------------------------------------------------------- |
| `int.MinValue`     | `TelemetryMiddleware`, `TelemetryNotificationMiddleware` |
| `int.MinValue + 1` | `LoggingMiddleware`                                      |
| `int.MinValue + 2` | `StatisticsMiddleware`                                   |

Use order values of `0` or higher for user middleware (negative values well above `int.MinValue + 2` are also safe).
