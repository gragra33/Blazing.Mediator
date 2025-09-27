# Pipeline Performance Benchmarks

This directory contains comprehensive benchmarks designed to validate the performance improvements made to `MiddlewarePipelineBuilder` and `NotificationPipelineBuilder` in the Blazing.Mediator library.

## 🎯 Performance Goals

Our optimization target: **<50ns execution time** for critical pipeline operations.

## 🚀 Key Optimizations Benchmarked

### 1. Assembly Scanning Elimination ⚡
- **Before**: 30+ second delays due to `AppDomain.CurrentDomain.GetAssemblies()` calls
- **After**: Millisecond execution using fast fallback types
- **Benchmark**: `MiddlewareBuilder_AssemblyScanning_Optimized`

### 2. Sorting Algorithm Optimization 🔄
- **Before**: O(n²) `FindIndex()` operations during middleware sorting  
- **After**: O(1) pre-calculated dictionary lookups
- **Benchmark**: `MiddlewareBuilder_OptimizedSorting`

### 3. LINQ Elimination 📊
- **Before**: Expensive LINQ operations with intermediate collections
- **After**: Optimized for loops with cached results
- **Benchmark**: `InterfaceChecking_OptimizedLoops`

### 4. I/O Operation Removal 🚫
- **Before**: Synchronous `Console.WriteLine` calls in hot paths
- **After**: Clean execution without blocking I/O
- **Impact**: Eliminated synchronous I/O bottlenecks

## 📁 Benchmark Files

### `PipelinePerformanceBenchmarks.cs`
Comprehensive benchmarks covering:
- End-to-end pipeline execution
- Query and command processing
- Middleware analysis performance
- Memory allocation optimization

### `PerformanceBottleneckBenchmarks.cs`
Targeted benchmarks for specific optimizations:
- Assembly scanning vs fast fallback types
- O(n²) vs O(1) sorting algorithms
- LINQ vs optimized loops comparison
- Type creation performance

### `NanosecondPrecisionBenchmarks.cs`
Ultra-high precision benchmarks:
- <50ns execution target validation
- Individual operation timing
- Critical hot path measurements
- Memory allocation tracking

## 🏃 Running Benchmarks

### Windows (PowerShell)
```powershell
.\run-pipeline-benchmarks.ps1
```

### Manual Execution
```bash
# Build in Release mode (essential for accurate benchmarks)
dotnet build -c Release

# Run specific benchmark suite
dotnet run -c Release -- --filter "*PipelinePerformanceBenchmarks*" --exporters github markdown --memory

# Run all pipeline benchmarks  
dotnet run -c Release -- --filter "*Pipeline*" --exporters github markdown --memory

# Run with additional diagnostics
dotnet run -c Release -- --filter "*NanosecondPrecisionBenchmarks*" --memory --threads --diagnostics
```

## 📊 Interpreting Results

### Key Metrics to Watch

1. **Execution Time**: Look for sub-nanosecond improvements in hot paths
2. **Memory Allocation**: Should show reduction in Gen0 collections
3. **Throughput**: Higher operations/second after optimizations
4. **Consistency**: Lower standard deviation indicates stable performance

### Expected Improvements

| Operation | Before | After | Improvement |
|-----------|---------|-------|-------------|
| Assembly Scanning | 30+ seconds | <10ms | ~99.97% |
| Middleware Sorting | O(n²) | O(1) | Exponential |
| Interface Checking | LINQ overhead | Direct loops | 60-80% |
| Pipeline Execution | Slower | <50ns target | Significant |

### Baseline Comparisons

Some benchmarks include `[Baseline = true]` annotations to show direct before/after comparisons:
- `InterfaceChecking_OriginalLinq` (baseline) vs `InterfaceChecking_OptimizedLoops`
- Memory allocations: Before vs After optimization

## 🔍 Troubleshooting

### Build Issues
```bash
# Clean and rebuild if you encounter issues
dotnet clean
dotnet restore  
dotnet build -c Release
```

### Benchmark Accuracy
- Ensure no other intensive processes are running
- Run multiple times for consistency
- Use Release configuration (never Debug)
- Close unnecessary applications to reduce noise

### Platform Considerations
- Windows: Uses `InliningDiagnoser` for detailed analysis
- Cross-platform: Some Windows-specific diagnostics may be unavailable
- .NET 9: Optimized for latest runtime improvements

## 📈 Continuous Performance Monitoring

These benchmarks can be integrated into CI/CD pipelines to ensure performance doesn't regress:

```bash
# CI/CD benchmark execution
dotnet run -c Release -- --filter "*Pipeline*" --exporters json --memory --job short
```

## 🎛️ Configuration Options

### Benchmark Configurations

- **`PipelinePerformanceConfig`**: Standard throughput testing
- **`BottleneckConfig`**: Targeted bottleneck analysis  
- **`NanosecondPrecisionConfig`**: Ultra-high precision timing

### Custom Execution
```bash
# Run with specific job configuration
dotnet run -c Release -- --job short --memory --threads

# Export to multiple formats
dotnet run -c Release -- --exporters console github json xml --memory

# Filter by category  
dotnet run -c Release -- --filter "*Assembly*" --memory
```

## 🏆 Success Criteria

The optimizations are successful if benchmarks show:

✅ **Assembly scanning**: <10ms execution time  
✅ **Sorting operations**: Near-constant time regardless of middleware count  
✅ **Memory allocations**: Reduced Gen0 garbage collection  
✅ **Pipeline execution**: Consistent sub-50ns performance for critical paths  
✅ **Throughput**: Higher operations/second across all scenarios

## 📝 Notes

- Benchmarks use minimal test middleware to focus on pipeline performance
- Generic middleware is tested to validate constraint optimization
- Multiple middleware counts stress-test sorting algorithms
- Memory diagnosers track allocation improvements
- Results may vary by hardware but relative improvements should be consistent