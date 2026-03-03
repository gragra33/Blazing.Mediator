using BenchmarkDotNet.Running;

// Run all benchmarks
BenchmarkSwitcher.FromAssembly(typeof(ComparisonBenchmarks.ComparisonBenchmarks).Assembly).Run(args);
