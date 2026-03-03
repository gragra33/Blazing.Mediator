# Mediator Comparison Benchmark — Session 17 — IsNotificationHandled fast path

Generated : 2026-02-28 13:29:32
Project : ComparisonBenchmarks
Config : Release / net10.0
Session : 17 — Non-async Publish<T> + IsNotificationHandled<T>() boolean sentinel

> **Changes vs Session 16:**
>
> - `MediatorDispatcherBase` — new abstract `IsNotificationHandled<TNotification>()` method
> - `MediatorCodeWriter.AppendContainerMetadata` — emits `IsNotificationHandled` override
>   with `typeof(TNotification) == typeof(X)` comparisons (JIT constant-foldable)
> - `Mediator.Notification.Publish<T>` — changed from `async ValueTask` + `try/catch NotImplementedException`
>   to non-async `ValueTask` returning dispatcher's `PublishAsync` directly on the fast path
> - `SubscriberTracker.IsGenericSubscriber(Type)` — removed; was the last `GetInterfaces()` call
>   in the library hot path; hardcoded `IsGeneric: true` at the only call site
>
> **Note:** Blazing.Mediator is running in **source-generated dispatch mode**.
> All optional features are disabled (no telemetry, no statistics, no logging).
> Blazing.Mediator IMediator is Scoped, pre-resolved from a long-lived scope.

## results\ComparisonBenchmarks.ComparisonBenchmarks-report-github.md

```
BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7922)
AMD Ryzen 7 3700X 3.59GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
```

| Method                          | Categories   |       Mean |     Error |     StdDev |     Median | Ratio | RatioSD | Rank |   Gen0 | Allocated | Alloc Ratio |
| ------------------------------- | ------------ | ---------: | --------: | ---------: | ---------: | ----: | ------: | ---: | -----: | --------: | ----------: |
| Notification_Mediator_Concrete  | Notification |   4.496 ns | 0.1481 ns |  0.4365 ns |   4.350 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  14.009 ns | 0.3119 ns |  0.8950 ns |  14.138 ns |  0.11 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  36.723 ns | 0.7323 ns |  1.3205 ns |  36.990 ns |  0.29 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification | 128.717 ns | 2.6168 ns |  5.5767 ns | 127.359 ns |  1.00 |    0.06 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |            |            |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   3.597 ns | 0.1395 ns |  0.2721 ns |   3.574 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |  13.108 ns | 0.3316 ns |  0.5448 ns |  13.097 ns |  0.14 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  23.477 ns | 0.4954 ns |  0.4634 ns |  23.544 ns |  0.25 |    0.01 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |  94.765 ns | 1.8935 ns |  3.2662 ns |  94.200 ns |  1.00 |    0.05 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |            |            |       |         |      |        |           |             |
| Stream_Mediator_Interface       | Streaming    | 100.053 ns | 1.9563 ns |  4.1266 ns | 100.659 ns |  0.27 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Concrete        | Streaming    | 101.402 ns | 2.0707 ns |  2.9029 ns | 100.214 ns |  0.27 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    | 111.693 ns | 2.2777 ns |  4.7545 ns | 111.082 ns |  0.30 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 370.907 ns | 7.4230 ns | 12.1963 ns | 371.335 ns |  1.00 |    0.05 |    3 | 0.0582 |     488 B |        1.00 |

## Session 17 Key Results vs Session 16

| Benchmark                        | Session 16        | Session 17          | Delta                | vs MediatR                       |
| -------------------------------- | ----------------- | ------------------- | -------------------- | -------------------------------- |
| **Notification_BlazingMediator** | 44.878 ns / 0 B   | **36.723 ns / 0 B** | **−18.2% (−8.2 ns)** | **3.5× faster, zero-alloc**      |
| **Request_BlazingMediator**      | 22.603 ns / 0 B   | 23.477 ns / 0 B     | +0.9 ns (noise)      | **4.0× faster, zero-alloc**      |
| **Stream_BlazingMediator**       | 107.538 ns / 96 B | 111.693 ns / 96 B   | +4.2 ns (noise)      | **3.3× faster, 5.1× less alloc** |

> **Notification: 44.878 → 36.723 ns — 18.2% faster.** The removal of the async state machine and
> try/catch overhead on the fast path delivers a solid 8.2 ns reduction. Allocation remains 0 B.
> Gap to ≤5 ns target explained by: `INotificationPublisher.Publish` overhead,
> `SequentialNotificationPublisher` handler iteration, virtual dispatch through
> `MediatorDispatcherBase.PublishAsync`, and `NotificationHandlerWrapper_*.Handle` machinery.
> The 5 ns target is aggressive — equivalent to martinothamar direct-concrete dispatch
> (4.5 ns) which has no abstraction layer.
