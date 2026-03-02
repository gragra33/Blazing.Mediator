# Blazing.Mediator — Cross-Library Benchmark Comparison

Performance comparison between **MediatR** (industry baseline), **Blazing.Mediator v2.0.1** (pre-optimisation, reflection-based), and **Blazing.Mediator v3.0.0** (source-generated, zero-alloc fast path).

All measurements taken on the same machine with all optional features disabled (no telemetry, no logging, no statistics middleware). Session 34 used the `ComparisonBenchmarks.Old` project (old v2.0.1 master). Session 33 used `ComparisonBenchmarks` (new v3.0.0 source-generated path).

**Environment:** BenchmarkDotNet v0.15.6 · .NET 10.0.3 · AMD Ryzen 7 3700X @ 3.59 GHz · 64 GB RAM

---

## Request / Send

| Library                                        |         Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | -----------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      2.65 ns |            0.03× |     96.6% faster |         — |
| martinothamar/Mediator — Interface             |     11.13 ns |            0.14× |     85.6% faster |         — |
| MediatR 12.5 _(baseline)_                      |     77.38 ns |            1.00× |                — |     128 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **20.42 ns** |        **0.30×** | **73.6% faster** |   **0 B** |
| Blazing.Mediator v2.0.1 (reflection)           |  2,002.81 ns |           25.98× |    2,488% slower |   2,160 B |

**v3.0.0 vs v2.0.1:** **98× faster** · **98.98% reduction** · **−2,160 B per call**

---

## Notification / Publish

| Library                                        |         Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | -----------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      4.26 ns |            0.04× |     96.2% faster |         — |
| martinothamar/Mediator — Interface             |     12.62 ns |            0.11× |     88.8% faster |         — |
| MediatR 12.5 _(baseline)_                      |    112.24 ns |            1.00× |                — |     288 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **31.29 ns** |        **0.29×** | **72.1% faster** |   **0 B** |
| Blazing.Mediator v2.0.1 (reflection)           |  2,184.08 ns |           19.52× |    1,846% slower |   2,136 B |

**v3.0.0 vs v2.0.1:** **70× faster** · **98.57% reduction** · **−2,136 B per call**

---

## Streaming / SendStream

| Library                                        |          Mean | Ratio to MediatR |       vs MediatR | Allocated |
| ---------------------------------------------- | ------------: | ---------------: | ---------------: | --------: |
| martinothamar/Mediator — Concrete              |      81.06 ns |            0.26× |     74.0% faster |      96 B |
| martinothamar/Mediator — Interface             |      88.93 ns |            0.29× |     71.4% faster |      96 B |
| MediatR 12.5 _(baseline)_                      |     311.41 ns |            1.00× |                — |     488 B |
| **Blazing.Mediator v3.0.0** (source-generated) | **103.65 ns** |        **0.32×** | **66.7% faster** |  **96 B** |
| Blazing.Mediator v2.0.1 (reflection)           |   3,025.45 ns |            9.74× |      872% slower |   2,768 B |

**v3.0.0 vs v2.0.1:** **29× faster** · **96.57% reduction** · **−2,672 B per call**

---

## Summary

| Dispatch path    | v2.0.1 (reflection) | v3.0.0 (source-gen) | Speed gain | % reduction |   Alloc gain |
| ---------------- | ------------------: | ------------------: | ---------: | ----------: | -----------: |
| **Request**      |  2,003 ns / 2,160 B |     **20 ns / 0 B** |    **98×** | **−98.98%** | **−2,160 B** |
| **Notification** |  2,184 ns / 2,136 B |     **31 ns / 0 B** |    **70×** | **−98.57%** | **−2,136 B** |
| **Stream**       |  3,025 ns / 2,768 B |   **104 ns / 96 B** |    **29×** | **−96.57%** | **−2,672 B** |

Blazing.Mediator v3.0.0 is **3–4× faster than MediatR** and allocates **zero bytes** on the Request and Notification hot paths.

---

## Benchmark Source

| Project               | Path                                                                                                                                         |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| v3.0.0 results        | [`benchmarks/ComparisonBenchmarks/results/2026-03-02_00-10-10/`](../../benchmarks/ComparisonBenchmarks/results/2026-03-02_00-10-10/)         |
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
