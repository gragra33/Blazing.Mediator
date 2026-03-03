# Mediator Comparison Benchmark — Source-Generator Post-Optimisation

Generated : 2026-03-01 22:05:33
Project : ComparisonBenchmarks
Config : Release / net10.0
Session : 30 — Post-Session-29 Scoped lifetime benchmark (Request regression detected)

> **Note:** Blazing.Mediator is running in **source-generated dispatch mode**.
> `ContainerMetadata`, `MediatorDispatcherBase`, and `IMediator` are all registered as **Scoped**
> (changed from Singleton in Session 29 to fix captive-dependency crash in ASP.NET Core).
> `ContainerMetadata` constructor calls `Init(sp)` on every handler wrapper using the scope's
> `IServiceProvider`, pre-resolving handlers and middleware chains once per scope.
> `Mediator.GetDispatcher()` resolves `MediatorDispatcherBase` on first call and caches it
> via `FastLazyValue` — subsequent dispatches pay only one `Volatile.Read`.
> All optional features are disabled (no telemetry, no statistics, no logging).
> Blazing.Mediator `IMediator` is Scoped, pre-resolved from a long-lived benchmark scope.
>
> ⚠️ **Request regression vs Session 17**: 28.48 ns / 48 B (was 23.48 ns / 0 B).
> Likely caused by Scoped `MediatorDispatcherBase` resolution overhead. Needs investigation.

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
| Notification_Mediator_Concrete  | Notification |   3.785 ns | 0.1070 ns |  0.2136 ns |   3.712 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  11.654 ns | 0.2508 ns |  0.5862 ns |  11.566 ns |  0.12 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  32.301 ns | 0.6808 ns |  1.6827 ns |  31.752 ns |  0.32 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification | 100.867 ns | 1.8532 ns |  4.9145 ns | 100.370 ns |  1.00 |    0.07 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |            |            |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   2.385 ns | 0.1077 ns |  0.2247 ns |   2.312 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |  10.243 ns | 0.2539 ns |  0.2924 ns |  10.328 ns |  0.14 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  28.481 ns | 0.5986 ns |  1.4795 ns |  27.963 ns |  0.40 |    0.03 |    3 | 0.0057 |      48 B |        0.38 |
| Request_MediatR                 | Request      |  71.403 ns | 1.4778 ns |  3.5406 ns |  70.850 ns |  1.00 |    0.07 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |            |            |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |  76.668 ns | 1.5642 ns |  2.6982 ns |  76.865 ns |  0.26 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |  77.880 ns | 1.5991 ns |  3.7062 ns |  77.645 ns |  0.27 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    |  90.809 ns | 1.8444 ns |  3.5091 ns |  90.045 ns |  0.31 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 292.901 ns | 5.8548 ns | 14.2513 ns | 288.876 ns |  1.00 |    0.07 |    3 | 0.0582 |     488 B |        1.00 |
