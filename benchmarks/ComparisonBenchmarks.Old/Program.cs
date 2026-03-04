using BenchmarkDotNet.Running;

// Run all benchmarks
BenchmarkSwitcher.FromAssembly(typeof(ComparisonBenchmarksOld.ComparisonBenchmarksOld).Assembly).Run(args);
