# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-03-01 23:02:10
Project   : ComparisonBenchmarks
Config    : Release / net10.0

> **Note:** Blazing.Mediator is running in **source-generated dispatch mode**.
> ContainerMetadata is a Singleton whose Init(sp) call pre-resolves handlers
> and middleware chains from the root IServiceProvider at startup.
> Mediator.GetDispatcher() resolves MediatorDispatcherBase on first call and
> caches it via FastLazyValue — subsequent dispatches pay only one Volatile.Read.
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
| Method                          | Categories   | Mean       | Error     | StdDev    | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------------- |-----------:|----------:|----------:|------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator_Concrete  | Notification |   3.788 ns | 0.1078 ns | 0.1709 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  13.620 ns | 0.3039 ns | 0.6921 ns |  0.14 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  32.447 ns | 0.6759 ns | 1.2860 ns |  0.33 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification |  98.614 ns | 1.9942 ns | 5.5591 ns |  1.00 |    0.08 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |           |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   2.442 ns | 0.1078 ns | 0.1802 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |   9.920 ns | 0.2298 ns | 0.2554 ns |  0.13 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  28.398 ns | 0.6273 ns | 1.4160 ns |  0.38 |    0.03 |    3 | 0.0057 |      48 B |        0.38 |
| Request_MediatR                 | Request      |  74.738 ns | 1.5562 ns | 3.4806 ns |  1.00 |    0.06 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |           |       |         |      |        |           |             |
| Stream_Mediator_Interface       | Streaming    |  77.445 ns | 1.5634 ns | 1.9200 ns |  0.28 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Concrete        | Streaming    |  80.161 ns | 1.6360 ns | 2.3980 ns |  0.29 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    |  90.797 ns | 1.8406 ns | 2.7549 ns |  0.32 |    0.01 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 280.355 ns | 5.6079 ns | 7.4864 ns |  1.00 |    0.04 |    3 | 0.0582 |     488 B |        1.00 |


