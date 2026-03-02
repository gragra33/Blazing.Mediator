# Blazing.Mediator — Cross-Library Benchmark Comparison

Performance comparison between **MediatR** (industry baseline), **Blazing.Mediator v2.0.1** (reflection-based), and **Blazing.Mediator v3.0.0** (source-generated, zero-alloc).

All measurements taken on the same machine with all optional features disabled (no telemetry, no logging, no statistics middleware).

**Environment:** BenchmarkDotNet v0.15.6 · .NET 10.0.3 · AMD Ryzen 7 3700X @ 3.59 GHz · 64 GB RAM

---

## Request / Send

| Library                                        |         Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | -----------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      2.35 ns |            0.03× |     96.7% faster |         — |
| martinothamar/Mediator — Interface             |     10.66 ns |            0.15× |     85.2% faster |         — |
| MediatR 12.5 _(baseline)_                      |     72.13 ns |            1.00× |                — |     128 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **17.49 ns** |        **0.24×** | **75.8% faster** |   **0 B** |
| Blazing.Mediator v2.0.1 (reflection)           |  2,002.81 ns |           25.98× |    2,488% slower |   2,160 B |

**v3.0.0 vs v2.0.1:** **114× faster** · **99.13% reduction** · **−2,160 B per call**

---

## Notification / Publish

| Library                                        |         Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | -----------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      4.04 ns |            0.04× |     96.4% faster |         — |
| martinothamar/Mediator — Interface             |     11.41 ns |            0.10× |     89.9% faster |         — |
| MediatR 12.5 _(baseline)_                      |    112.98 ns |            1.00× |                — |     288 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **31.35 ns** |        **0.28×** | **72.3% faster** |   **0 B** |
| Blazing.Mediator v2.0.1 (reflection)           |  2,184.08 ns |           19.52× |    1,846% slower |   2,136 B |

**v3.0.0 vs v2.0.1:** **70× faster** · **98.57% reduction** · **−2,136 B per call**

---

## Streaming / SendStream

| Library                                        |          Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | ------------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      75.87 ns |            0.25× |     75.1% faster |      96 B |
| martinothamar/Mediator — Interface             |      82.96 ns |            0.27× |     72.8% faster |      96 B |
| MediatR 12.5 _(baseline)_                      |     304.92 ns |            1.00× |                — |     488 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **100.89 ns** |        **0.33×** | **66.9% faster** |  **96 B** |
| Blazing.Mediator v2.0.1 (reflection)           |   3,025.45 ns |            9.74× |      872% slower |   2,768 B |

**v3.0.0 vs v2.0.1:** **30× faster** · **96.67% reduction** · **−2,672 B per call**

---

## Summary

| Dispatch path    | v2.0.1 (reflection) | v3.0.0 (source-gen) | Speed gain | % reduction |   Alloc gain |
| ---------------- | ------------------: | ------------------: | ---------: | ----------: | -----------: |
| **Request**      |  2,003 ns / 2,160 B |     **17 ns / 0 B** |   **114×** | **−99.13%** | **−2,160 B** |
| **Notification** |  2,184 ns / 2,136 B |     **31 ns / 0 B** |    **70×** | **−98.57%** | **−2,136 B** |
| **Stream**       |  3,025 ns / 2,768 B |   **101 ns / 96 B** |    **30×** | **−96.67%** | **−2,672 B** |

Blazing.Mediator v3.0.0 is **3–4× faster than MediatR** and allocates **zero bytes** on the Request and Notification hot paths.

---

## Benchmark Source

| Project               | Path                                                                                                                                         |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| v3.0.0 results        | [`benchmarks/ComparisonBenchmarks/results/2026-03-02_14-16-36/`](../../benchmarks/ComparisonBenchmarks/results/2026-03-02_14-16-36/)         |
| v2.0.1 results        | [`benchmarks/ComparisonBenchmarks.Old/results/2026-03-02_11-21-42/`](../../benchmarks/ComparisonBenchmarks.Old/results/2026-03-02_11-21-42/) |
| v3.0.0 benchmark code | [`benchmarks/ComparisonBenchmarks/ComparisonBenchmarks.cs`](../../benchmarks/ComparisonBenchmarks/ComparisonBenchmarks.cs)                   |
| v2.0.1 benchmark code | [`benchmarks/ComparisonBenchmarks.Old/ComparisonBenchmarks.cs`](../../benchmarks/ComparisonBenchmarks.Old/ComparisonBenchmarks.cs)           |

To reproduce the v3.0.0 results:

```powershell
cd benchmarks/ComparisonBenchmarks
.\run-comparison-benchmark.ps1
```

To reproduce the v2.0.1 results:

```powershell
cd benchmarks/ComparisonBenchmarks.Old
.\run-comparison-old-benchmark.ps1
```
