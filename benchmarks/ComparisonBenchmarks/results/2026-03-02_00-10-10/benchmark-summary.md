# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-03-02 00:21:01
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
| Notification_Mediator_Concrete  | Notification |   3.953 ns | 0.1106 ns |  0.1878 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |  11.241 ns | 0.2584 ns |  0.4457 ns |  0.10 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_BlazingMediator    | Notification |  31.290 ns | 0.6592 ns |  1.1014 ns |  0.29 |    0.02 |    3 |      - |         - |        0.00 |
| Notification_MediatR            | Notification | 107.584 ns | 2.2703 ns |  6.6583 ns |  1.00 |    0.09 |    4 | 0.0343 |     288 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |   2.337 ns | 0.1046 ns |  0.1432 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |  11.277 ns | 0.2823 ns |  0.6428 ns |  0.16 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |  20.418 ns | 0.4776 ns |  0.8365 ns |  0.30 |    0.02 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |  68.736 ns | 1.4350 ns |  3.6786 ns |  1.00 |    0.07 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |            |           |            |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |  81.759 ns | 1.6857 ns |  3.5188 ns |  0.25 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |  95.744 ns | 1.9359 ns |  2.2293 ns |  0.30 |    0.01 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_BlazingMediator          | Streaming    | 103.646 ns | 2.1057 ns |  4.9635 ns |  0.32 |    0.02 |    3 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    | 322.503 ns | 6.4276 ns | 10.7391 ns |  1.00 |    0.05 |    4 | 0.0582 |     488 B |        1.00 |


