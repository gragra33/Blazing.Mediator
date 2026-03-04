# Breaking Changes — Blazing.Mediator v2.0.1 → v3.0.0

This document lists every API change that requires updates to consuming code when upgrading from Blazing.Mediator v2.0.1 (master) to v3.0.0.

---

## 1. Handler return types: `Task` → `ValueTask`

All request handler interfaces now return `ValueTask` instead of `Task`. This is the most widespread change — every handler in your project needs to be updated.

### `IRequestHandler<TRequest>` (void / unit handlers)

```csharp
// v2.0.1 — old
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

// v3.0.0 — new
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    ValueTask Handle(TRequest request, CancellationToken cancellationToken = default);
}
```

### `IRequestHandler<TRequest, TResponse>` (response handlers)

```csharp
// v2.0.1 — old
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

// v3.0.0 — new
public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    ValueTask<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}
```

The same change applies to the semantic aliases `ICommandHandler<T>`, `ICommandHandler<T, TResponse>`, and `IQueryHandler<T, TResponse>` — they inherit the updated `IRequestHandler<,>` contract.

---

## 2. Notification handler return type: `Task` → `ValueTask`

```csharp
// v2.0.1 — old
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}

// v3.0.0 — new
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    ValueTask Handle(TNotification notification, CancellationToken cancellationToken = default);
}
```

> `INotificationSubscriber<T>.OnNotification` is **unchanged** — it still returns `Task`.

---

## 3. Middleware return types: `Task` → `ValueTask`

### `IRequestMiddleware<TRequest, TResponse>`

```csharp
// v2.0.1 — old
ValueTask<TResponse> HandleAsync(
    TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
// was: Task<TResponse> HandleAsync(...)

// v3.0.0 — new
ValueTask<TResponse> HandleAsync(
    TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
```

### `IRequestMiddleware<TRequest>` (void variant)

```csharp
// v2.0.1 — old: Task HandleAsync(...)
// v3.0.0 — new:
ValueTask HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken);
```

### `INotificationMiddleware` and `INotificationMiddleware<TNotification>`

```csharp
// v2.0.1 — old: Task InvokeAsync<TNotification>(...)
// v3.0.0 — new:
ValueTask InvokeAsync<TNotification>(
    TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    where TNotification : INotification;
```

> `IStreamRequestMiddleware<TRequest, TResponse>.HandleAsync` is **unchanged** — it returns `IAsyncEnumerable<TResponse>`.

---

## 4. `IMediator` return types: `Task` → `ValueTask`

```csharp
// v2.0.1 — old
Task Send(IRequest request, CancellationToken cancellationToken = default);
Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
    where TNotification : INotification;

// v3.0.0 — new
ValueTask Send(IRequest request, CancellationToken cancellationToken = default);
ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
    where TNotification : INotification;
```

All call sites that `await mediator.Send(...)` or `await mediator.Publish(...)` continue to compile without changes. Sites that capture the return value as `Task<T>` must be updated to `ValueTask<T>`.

---

## 5. Dispatch is now entirely source-generated — all runtime reflection stripped out

In v2.0.1, `AddMediator()` was a set of hand-written extension methods inside the `Blazing.Mediator` library, and all dispatch ultimately relied on runtime reflection to resolve handlers. In v3.0.0, every byte of reflection-based dispatch has been replaced with compile-time source generation. `AddMediator()` is now emitted by `Blazing.Mediator.SourceGenerators` per consuming project, producing a fully typed, zero-allocation dispatch table at build time.

**There is no reflection path left.** If `AddMediator()` is not called (or `Blazing.Mediator.SourceGenerators` is not referenced), calling `IMediator.Send`, `IMediator.Publish`, or `IMediator.SendStream` throws `InvalidOperationException`:

```
Source generator dispatcher not found. Ensure AddMediator() is called and source generators are active.
```

**`Blazing.Mediator.SourceGenerators` is a required dependency** alongside `Blazing.Mediator`. Add it to every project that calls `AddMediator()`:

```xml
<PackageReference Include="Blazing.Mediator.SourceGenerators" />
```

In short: **the source generator is required**. Projects that cannot use source generators are not compatible with v3.0.0.

---

## 6. `#if USE_SOURCE_GENERATORS` preprocessor blocks removed

v2.0.1 required consuming projects to define `USE_SOURCE_GENERATORS` to activate the generated dispatch path:

```xml
<!-- v2.0.1 — old: required in consuming .csproj -->
<DefineConstants>USE_SOURCE_GENERATORS</DefineConstants>
```

v3.0.0 eliminates this entirely. The new `MediatorDispatcherBase` abstract class bridges the pre-compiled library and the source-generated `ContainerMetadata`. Remove all `#if USE_SOURCE_GENERATORS` / `#endif` guards from your code. The source generator path activates automatically when `Blazing.Mediator.SourceGenerators` is referenced.

---

## 7. `MediatorConfiguration` extends new `MediatorConfigurationSection`

`MediatorConfiguration` now inherits from `MediatorConfigurationSection` and implements `IEnvironmentConfigurationOptions<MediatorConfiguration>`. The static factory methods (`Development()`, `Production()`, `Disabled()`, `Minimal()`) are unchanged. The constructor is unchanged.

The `EnableStatisticsTracking` bool property is **deprecated** — use `StatisticsOptions` instead:

```csharp
// v2.0.1 — deprecated (still compiles, but produces Obsolete warning)
config.EnableStatisticsTracking = true;

// v3.0.0 — preferred
config.WithStatisticsTracking(options => { ... });
```

---

## 8. New required service lifetime: Singleton (was Scoped)

In v2.0.1 it was common to register `IMediator` as Scoped due to its pipeline builder dependencies. In v3.0.0 the default and recommended lifetime for `IMediator` (and its backing `ContainerMetadata`) is **Singleton**, configured via `MediatorOptions.ServiceLifetime`. This eliminates per-request DI resolution overhead and is what produces the zero-allocation fast path.

If you have explicit Scoped overrides in your registration, review whether they are still needed.

---

## 9. `INotificationPublisher` replaces hard-coded sequential dispatch

In v2.0.1 notification dispatch was always sequential and hard-coded inside `Mediator.cs`. In v3.0.0 it is pluggable via `INotificationPublisher`. The default behaviour (sequential) is identical — no change is required unless you need concurrent dispatch:

```csharp
// v3.0.0 — opt-in concurrent dispatch (new capability)
var config = new MediatorConfiguration();
config.WithConcurrentNotificationPublisher();
services.AddMediator(config);
```

---

## 10. New type: `MediatorDispatcherBase`

Source generators now emit `internal sealed class ContainerMetadata : MediatorDispatcherBase`. If you wrote code that reflected over or referenced the generated type directly, update those references. Normal application code is not affected.

---

## Not breaking (unchanged)

| Interface / Class                                         | Status                                                                                     |
| --------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `INotificationSubscriber<T>.OnNotification`               | Unchanged — returns `Task`                                                                 |
| `INotificationSubscriber.OnNotification`                  | Unchanged — returns `Task`                                                                 |
| `IStreamRequestHandler<T,R>.Handle`                       | Unchanged — returns `IAsyncEnumerable<R>`                                                  |
| `IStreamRequestMiddleware<T,R>.HandleAsync`               | Unchanged — returns `IAsyncEnumerable<R>`                                                  |
| `IRequest`, `IRequest<T>`                                 | Unchanged                                                                                  |
| `ICommand`, `ICommand<T>`                                 | Unchanged                                                                                  |
| `IQuery<T>`                                               | Unchanged                                                                                  |
| `INotification`                                           | Unchanged                                                                                  |
| `IStreamRequest<T>`                                       | Unchanged                                                                                  |
| `IConditionalMiddleware`                                  | Unchanged (inherits updated `IRequestMiddleware`) — only the `ValueTask` change propagates |
| `IConditionalNotificationMiddleware`                      | Unchanged (inherits updated `INotificationMiddleware`)                                     |
| `IConditionalStreamRequestMiddleware`                     | Unchanged                                                                                  |
| `[Order]` attribute                                       | Unchanged — same namespace `Blazing.Mediator.Abstractions.Middleware`                      |
| `MediatorStatistics`                                      | Unchanged                                                                                  |
| `TelemetryOptions`, `LoggingOptions`, `StatisticsOptions` | Extended with `Validate()` / `Clone()` — backward-compatible                               |
| `IMediator.Subscribe` / `Unsubscribe`                     | Unchanged                                                                                  |
| `IMediator.SendStream<T>`                                 | Unchanged — returns `IAsyncEnumerable<T>`                                                  |
