# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-02-28 11:12:21
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
| Method                          | Categories   | Mean         | Error      | StdDev      | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------------- |-------------:|-----------:|------------:|------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator_Concrete  | Notification |     4.076 ns |  0.1142 ns |   0.1908 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |    12.027 ns |  0.2689 ns |   0.3302 ns |  0.10 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_MediatR            | Notification |   120.066 ns |  2.4489 ns |   6.6208 ns |  1.00 |    0.08 |    3 | 0.0343 |     288 B |        1.00 |
| Notification_BlazingMediator    | Notification | 2,704.838 ns | 56.1212 ns | 164.5938 ns | 22.59 |    1.82 |    4 | 0.3357 |    2832 B |        9.83 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |     3.088 ns |  0.1228 ns |   0.1148 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |    14.355 ns |  0.3581 ns |   0.5360 ns |  0.16 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |    26.694 ns |  0.5948 ns |   0.8719 ns |  0.30 |    0.01 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |    89.470 ns |  1.8548 ns |   2.6002 ns |  1.00 |    0.04 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |    95.929 ns |  1.8948 ns |   2.3270 ns |  0.25 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |   105.844 ns |  2.1629 ns |   3.2374 ns |  0.28 |    0.01 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    |   377.263 ns |  7.5411 ns |  16.3937 ns |  1.00 |    0.06 |    3 | 0.0582 |     488 B |        1.00 |
| Stream_BlazingMediator          | Streaming    | 1,462.710 ns | 29.2739 ns |  39.0799 ns |  3.88 |    0.19 |    4 | 0.1640 |    1376 B |        2.82 |


