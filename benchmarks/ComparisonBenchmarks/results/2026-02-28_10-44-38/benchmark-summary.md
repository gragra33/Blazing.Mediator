# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : 2026-02-28 10:45:17
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
| Method                          | Categories   | Mean | Error | Ratio | RatioSD | Rank | Alloc Ratio |
|-------------------------------- |------------- |-----:|------:|------:|--------:|-----:|------------:|
| Notification_MediatR            | Notification |   NA |    NA |     ? |       ? |    ? |           ? |
| Notification_Mediator_Concrete  | Notification |   NA |    NA |     ? |       ? |    ? |           ? |
| Notification_Mediator_Interface | Notification |   NA |    NA |     ? |       ? |    ? |           ? |
| Notification_BlazingMediator    | Notification |   NA |    NA |     ? |       ? |    ? |           ? |
|                                 |              |      |       |       |         |      |             |
| Request_MediatR                 | Request      |   NA |    NA |     ? |       ? |    ? |           ? |
| Request_Mediator_Concrete       | Request      |   NA |    NA |     ? |       ? |    ? |           ? |
| Request_Mediator_Interface      | Request      |   NA |    NA |     ? |       ? |    ? |           ? |
| Request_BlazingMediator         | Request      |   NA |    NA |     ? |       ? |    ? |           ? |
|                                 |              |      |       |       |         |      |             |
| Stream_MediatR                  | Streaming    |   NA |    NA |     ? |       ? |    ? |           ? |
| Stream_Mediator_Concrete        | Streaming    |   NA |    NA |     ? |       ? |    ? |           ? |
| Stream_Mediator_Interface       | Streaming    |   NA |    NA |     ? |       ? |    ? |           ? |
| Stream_BlazingMediator          | Streaming    |   NA |    NA |     ? |       ? |    ? |           ? |

Benchmarks with issues:
  ComparisonBenchmarks.Notification_MediatR: DefaultJob
  ComparisonBenchmarks.Notification_Mediator_Concrete: DefaultJob
  ComparisonBenchmarks.Notification_Mediator_Interface: DefaultJob
  ComparisonBenchmarks.Notification_BlazingMediator: DefaultJob
  ComparisonBenchmarks.Request_MediatR: DefaultJob
  ComparisonBenchmarks.Request_Mediator_Concrete: DefaultJob
  ComparisonBenchmarks.Request_Mediator_Interface: DefaultJob
  ComparisonBenchmarks.Request_BlazingMediator: DefaultJob
  ComparisonBenchmarks.Stream_MediatR: DefaultJob
  ComparisonBenchmarks.Stream_Mediator_Concrete: DefaultJob
  ComparisonBenchmarks.Stream_Mediator_Interface: DefaultJob
  ComparisonBenchmarks.Stream_BlazingMediator: DefaultJob


