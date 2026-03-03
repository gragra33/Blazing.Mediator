# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-03-02 14:26:35
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
| Method                          | Categories   | Mean       | Error     | StdDev     | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------------- |-----------:|----------:|-----------:|------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator_Concrete  | Notification |   4.040 ns | 0.1119 ns |  0.1807 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  11.407 ns | 0.2571 ns |  0.3848 ns |  0.10 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  31.345 ns | 0.6536 ns |  1.4068 ns |  0.28 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification | 112.984 ns | 2.2931 ns |  6.1603 ns |  1.00 |    0.08 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   2.345 ns | 0.1065 ns |  0.1809 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |  10.663 ns | 0.2665 ns |  0.3736 ns |  0.15 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  17.494 ns | 0.4108 ns |  0.8012 ns |  0.24 |    0.01 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |  72.132 ns | 1.4788 ns |  2.9532 ns |  1.00 |    0.06 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |  75.874 ns | 1.5594 ns |  3.4556 ns |  0.25 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |  82.960 ns | 1.7099 ns |  2.3970 ns |  0.27 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    | 100.893 ns | 2.0401 ns |  3.5191 ns |  0.33 |    0.02 |    3 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 304.919 ns | 6.0816 ns | 15.2576 ns |  1.00 |    0.07 |    4 | 0.0582 |     488 B |        1.00 |


