# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-02-28 10:55:17
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
| Notification_Mediator_Concrete  | Notification |     4.681 ns |  0.1299 ns |   0.1546 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |    13.412 ns |  0.2564 ns |   0.2273 ns |  0.10 |    0.00 |    2 |      - |         - |        0.00 |
| Notification_MediatR            | Notification |   132.239 ns |  2.6245 ns |   5.3015 ns |  1.00 |    0.06 |    3 | 0.0343 |     288 B |        1.00 |
| Notification_BlazingMediator    | Notification | 3,055.217 ns | 60.5610 ns | 130.3641 ns | 23.14 |    1.33 |    4 | 0.3357 |    2832 B |        9.83 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |     3.294 ns |  0.1291 ns |   0.2120 ns |  0.04 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |    15.149 ns |  0.3451 ns |   0.5953 ns |  0.18 |    0.01 |    2 |      - |         - |        0.00 |
| Request_BlazingMediator         | Request      |    24.829 ns |  0.5617 ns |   0.9385 ns |  0.30 |    0.02 |    3 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |    84.153 ns |  1.7465 ns |   3.8335 ns |  1.00 |    0.06 |    4 | 0.0153 |     128 B |        1.00 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Stream_BlazingMediator          | Streaming    |           NA |         NA |          NA |     ? |       ? |    ? |     NA |        NA |           ? |
| Stream_Mediator_Concrete        | Streaming    |    96.219 ns |  1.9484 ns |   2.0009 ns |  0.26 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |    99.975 ns |  2.0299 ns |   3.2780 ns |  0.27 |    0.01 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    |   365.856 ns |  6.8772 ns |  12.5753 ns |  1.00 |    0.05 |    2 | 0.0582 |     488 B |        1.00 |

Benchmarks with issues:
  ComparisonBenchmarks.Stream_BlazingMediator: DefaultJob


