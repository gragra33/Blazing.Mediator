# Mediator Comparison Benchmark — Pre-Optimisation Baseline (Old Blazing.Mediator v2.0.1)
Generated : 2026-03-02 11:35:22
Project   : ComparisonBenchmarks.Old
Config    : Release / net10.0

> **Note:** Old Blazing.Mediator v2.0.1 (master branch) is running in **reflection-based dispatch mode**.
> There is NO source generator. On every Send/Publish/SendStream call the Mediator resolves
> IRequestHandler<T,R> via IServiceProvider.GetServices() on an open-generic type, then dispatches
> via MethodInfo.Invoke(). All optional features are disabled (no telemetry, no statistics, no logging).
> Old Blazing.Mediator IMediator is Scoped, pre-resolved from a long-lived scope.

## ComparisonBenchmarksOld.ComparisonBenchmarksOld-report-github.md

```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7922)
AMD Ryzen 7 3700X 3.59GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.3 (10.0.3, 10.0.326.7603), X64 RyuJIT x86-64-v3


```
| Method                          | Categories   | Mean         | Error      | StdDev      | Median       | Ratio | RatioSD | Rank | Gen0   | Allocated | Alloc Ratio |
|-------------------------------- |------------- |-------------:|-----------:|------------:|-------------:|------:|--------:|-----:|-------:|----------:|------------:|
| Notification_Mediator_Concrete  | Notification |     4.255 ns |  0.2375 ns |   0.7003 ns |     3.967 ns |  0.04 |    0.01 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |    12.617 ns |  0.2435 ns |   0.2278 ns |    12.536 ns |  0.11 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_MediatR            | Notification |   112.236 ns |  2.2717 ns |   6.2569 ns |   111.625 ns |  1.00 |    0.08 |    3 | 0.0343 |     288 B |        1.00 |
| Notification_OldBlazingMediator | Notification | 2,184.082 ns | 59.3320 ns | 171.1864 ns | 2,157.197 ns | 19.52 |    1.87 |    4 | 0.2518 |    2136 B |        7.42 |
|                                 |              |              |            |             |              |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |     2.645 ns |  0.1105 ns |   0.0923 ns |     2.624 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |    11.126 ns |  0.2854 ns |   0.4768 ns |    11.030 ns |  0.14 |    0.01 |    2 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |    77.377 ns |  1.6276 ns |   4.7220 ns |    76.721 ns |  1.00 |    0.08 |    3 | 0.0153 |     128 B |        1.00 |
| Request_OldBlazingMediator      | Request      | 2,002.809 ns | 39.8471 ns | 101.4235 ns | 1,984.104 ns | 25.98 |    2.01 |    4 | 0.2575 |    2160 B |       16.88 |
|                                 |              |              |            |             |              |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |    81.057 ns |  1.6705 ns |   4.5446 ns |    79.877 ns |  0.26 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |    88.931 ns |  1.7450 ns |   3.6808 ns |    88.127 ns |  0.29 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    |   311.410 ns |  6.2371 ns |  14.3307 ns |   307.919 ns |  1.00 |    0.06 |    3 | 0.0582 |     488 B |        1.00 |
| Stream_OldBlazingMediator       | Streaming    | 3,025.446 ns | 60.3949 ns | 148.1497 ns | 2,996.994 ns |  9.74 |    0.64 |    4 | 0.3281 |    2768 B |        5.67 |

