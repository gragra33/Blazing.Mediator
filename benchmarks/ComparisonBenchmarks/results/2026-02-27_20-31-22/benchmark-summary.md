# Mediator Comparison Benchmark — Pre-Optimisation Baseline
Generated : 2026-02-27 20:41:11
Project   : ComparisonBenchmarks
Config    : Release / net10.0

> **Note:** Blazing.Mediator is running in reflection-based dispatch mode
> (current architecture, pre-source-generator overhaul).  All optional
> features are disabled (no telemetry, no statistics, no logging).
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
| Notification_Mediator_Concrete  | Notification |     3.965 ns |  0.1105 ns |   0.1654 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Notification_Mediator_Interface | Notification |    12.046 ns |  0.2719 ns |   0.5675 ns |  0.10 |    0.01 |    2 |      - |         - |        0.00 |
| Notification_MediatR            | Notification |   121.131 ns |  3.1343 ns |   9.1923 ns |  1.01 |    0.11 |    3 | 0.0343 |     288 B |        1.00 |
| Notification_BlazingMediator    | Notification | 2,564.625 ns | 50.6430 ns | 114.3097 ns | 21.29 |    1.83 |    4 | 0.3357 |    2832 B |        9.83 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Request_Mediator_Concrete       | Request      |     2.407 ns |  0.1068 ns |   0.1926 ns |  0.03 |    0.00 |    1 |      - |         - |        0.00 |
| Request_Mediator_Interface      | Request      |    10.025 ns |  0.2633 ns |   0.4542 ns |  0.14 |    0.01 |    2 |      - |         - |        0.00 |
| Request_MediatR                 | Request      |    74.089 ns |  1.7112 ns |   4.9645 ns |  1.00 |    0.09 |    3 | 0.0153 |     128 B |        1.00 |
| Request_BlazingMediator         | Request      | 2,656.682 ns | 52.7701 ns | 105.3877 ns | 36.01 |    2.75 |    4 | 0.3624 |    3032 B |       23.69 |
|                                 |              |              |            |             |       |         |      |        |           |             |
| Stream_Mediator_Concrete        | Streaming    |    86.232 ns |  1.7718 ns |   3.6591 ns |  0.28 |    0.02 |    1 | 0.0114 |      96 B |        0.20 |
| Stream_Mediator_Interface       | Streaming    |    95.136 ns |  1.9358 ns |   2.8974 ns |  0.31 |    0.02 |    2 | 0.0114 |      96 B |        0.20 |
| Stream_MediatR                  | Streaming    |   303.626 ns |  6.0845 ns |  11.7229 ns |  1.00 |    0.05 |    3 | 0.0582 |     488 B |        1.00 |
| Stream_BlazingMediator          | Streaming    | 3,168.348 ns | 62.3817 ns | 128.8289 ns | 10.45 |    0.58 |    4 | 0.3777 |    3160 B |        6.48 |


