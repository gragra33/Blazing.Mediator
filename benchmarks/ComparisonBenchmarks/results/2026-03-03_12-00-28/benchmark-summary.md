# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-03-03 12:13:44
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
| Notification_Mediator_Concrete  | Notification |   3.941 ns | 0.1076 ns |  0.2293 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  11.585 ns | 0.2610 ns |  0.5091 ns |  0.11 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  30.680 ns | 0.6267 ns |  1.6290 ns |  0.30 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification | 103.831 ns | 2.3154 ns |  6.7906 ns |  1.00 |    0.09 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   2.369 ns | 0.1078 ns |  0.1740 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |  10.999 ns | 0.2761 ns |  0.5513 ns |  0.15 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  19.125 ns | 0.4431 ns |  1.0269 ns |  0.26 |    0.02 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |  72.639 ns | 1.4630 ns |  3.9302 ns |  1.00 |    0.08 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Stream_Mediator_Interface       | Streaming    |  83.081 ns | 1.6574 ns |  4.4240 ns |  0.27 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Concrete        | Streaming    |  87.616 ns | 1.7903 ns |  4.5892 ns |  0.28 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    |  92.641 ns | 1.8788 ns |  4.7136 ns |  0.30 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 313.579 ns | 6.1830 ns | 11.7638 ns |  1.00 |    0.05 |    3 | 0.0582 |     488 B |        1.00 |


