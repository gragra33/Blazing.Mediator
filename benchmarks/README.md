# Blazing.Mediator Benchmarks

This directory contains performance benchmarks for comparing reflection-based dispatch vs source generation.

## Quick Start

### Windows (PowerShell)
```powershell
.\run-sourcegen-benchmark.ps1
```

### Linux/Mac (Bash)
```bash
chmod +x run-sourcegen-benchmark.sh
./run-sourcegen-benchmark.sh
```

## What These Scripts Do

The benchmark scripts automate the process of properly comparing reflection vs source generation:

1. **Build Blazing.Mediator in Release mode** (uses reflection)
2. **Run benchmarks** to establish baseline performance
3. **Build Blazing.Mediator in SourceGen mode** (uses source generation)
4. **Run benchmarks** again to measure improvements
5. **Display results** showing the performance difference

## Understanding the Results

Look for these improvements with source generation:

| Metric | Expected Improvement |
|--------|---------------------|
| **Execution Time** | 15-35% faster |
| **Memory Allocation** | 30-50% less |
| **Cold Start** | 3-5x faster |
| **Type Resolution** | 40-60% faster |

### Sample Expected Output

```
| Method                     | Job             | Mean      | Allocated |
|--------------------------- |---------------- |----------:|----------:|
| SendRequests               | Reflection-NET9 |  2.579 us |   2.38 KB |  ? Baseline
| SendRequests               | SourceGen-NET9  |  1.850 us |   1.65 KB |  ? 28% faster, 31% less GC
```

## Manual Benchmark Process

If you prefer to run benchmarks manually:

### 1. Build Reflection Version
```bash
dotnet build src\Blazing.Mediator\Blazing.Mediator.csproj -c Release
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0
```

### 2. Build Source Generation Version
```bash
dotnet build src\Blazing.Mediator\Blazing.Mediator.csproj -c SourceGen
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0
```

## How It Works

### Source Generation Configuration

The `Blazing.Mediator.csproj` has two configurations:

```xml
<!-- Release: Uses reflection (baseline) -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <!-- USE_SOURCE_GENERATORS is NOT defined -->
</PropertyGroup>

<!-- SourceGen: Uses source generation (optimized) -->
<PropertyGroup Condition="'$(Configuration)' == 'SourceGen'">
    <DefineConstants>$(DefineConstants);USE_SOURCE_GENERATORS</DefineConstants>
</PropertyGroup>
```

When `USE_SOURCE_GENERATORS` is defined:
- Mediator uses generated dispatch tables instead of reflection
- Handler resolution is compile-time instead of runtime
- Type lookups use cached constants instead of string manipulation

### Benchmark Configuration

The `BenchmarkConfig.cs` defines two jobs:
- **Reflection-NET9**: Baseline using reflection
- **SourceGen-NET9**: Optimized using source generation

Both jobs run the same benchmark code, but against different builds of Blazing.Mediator.

## Troubleshooting

### "No performance difference"

**Problem**: Both jobs show identical performance.

**Solution**: Make sure you're building Blazing.Mediator in the correct configuration:
```bash
# This is WRONG - both use reflection
dotnet build -c Release
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release

# This is CORRECT - properly builds SourceGen version
dotnet build src\Blazing.Mediator\Blazing.Mediator.csproj -c SourceGen
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release
```

### "Generated files not found"

**Problem**: Source generator doesn't produce output.

**Solution**: Check that:
1. SourceGen configuration defines `USE_SOURCE_GENERATORS`
2. Blazing.Mediator references the SourceGenerators project
3. Generated files appear in `obj\SourceGen\generated\`

```bash
# Verify generated files
ls src\Blazing.Mediator\obj\SourceGen\**\generated\**\*.g.cs
```

### "Cannot find Blazing.Mediator.dll"

**Problem**: Project reference issues.

**Solution**: The benchmark project uses a ProjectReference which automatically uses whichever configuration of Blazing.Mediator was last built. Make sure to build Blazing.Mediator first before running benchmarks.

## Advanced Options

### Run Specific Benchmarks

```bash
# Run only one specific benchmark class
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --filter "*Benchmarks*"

# Run benchmarks matching a pattern
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --filter "*Send*"
```

### Export Results

```bash
# Export to CSV
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --exporters csv

# Export to JSON
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --exporters json
```

### Memory Profiling

```bash
# Run with detailed memory diagnoser
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --memory

# Run with event trace profiler (Windows only)
dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0 -- --profiler ETW
```

## Documentation

For more detailed information, see:
- **[Source Generator Status](../docs/source-gen/SOURCE_GENERATOR_STATUS.md)** - Implementation details
- **[How to Benchmark SourceGen](../docs/source-gen/HOW_TO_BENCHMARK_SOURCEGEN.md)** - Detailed benchmark guide
- **[Executive Summary](../docs/source-gen/EXECUTIVE_SUMMARY.md)** - Quick overview

## Contributing

When adding new benchmarks:
1. Ensure they work with both configurations
2. Add appropriate `[BenchmarkCategory]` attributes
3. Document expected performance characteristics
4. Test with both Release and SourceGen builds

## CI/CD Integration

To integrate these benchmarks into CI/CD:

```yaml
# Example GitHub Actions workflow
- name: Run Reflection Benchmarks
  run: |
    dotnet build src/Blazing.Mediator/Blazing.Mediator.csproj -c Release
    dotnet run --project benchmarks/Blazing.Mediator.Benchmarks -c Release --framework net10.0

- name: Run SourceGen Benchmarks
  run: |
    dotnet build src/Blazing.Mediator/Blazing.Mediator.csproj -c SourceGen
    dotnet run --project benchmarks/Blazing.Mediator.Benchmarks -c Release --framework net10.0
```
