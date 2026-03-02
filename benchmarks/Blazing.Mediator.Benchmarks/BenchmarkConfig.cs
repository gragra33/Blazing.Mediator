using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Configuration for comparing reflection-based vs source generator-based mediator performance.
/// 
/// IMPORTANT: To properly compare reflection vs source generation, you need to run benchmarks
/// with DIFFERENT builds of Blazing.Mediator:
/// 
/// 1. Release build (reflection-based):
///    dotnet build src\Blazing.Mediator\Blazing.Mediator.csproj -c Release
///    dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0
/// 
/// 2. SourceGen build (source generation enabled):
///    dotnet build src\Blazing.Mediator\Blazing.Mediator.csproj -c SourceGen
///    dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0
///    (Note: Benchmark still uses Release, but references SourceGen build via project reference)
/// 
/// The benchmark project will automatically reference the correct build based on which
/// Blazing.Mediator configuration was last built.
/// 
/// Expected performance improvements with source generation:
/// - 15-35% faster Send() operations
/// - 30-50% less memory allocation
/// - 3-5x faster cold start performance
/// 
/// See docs/source-gen/HOW_TO_BENCHMARK_SOURCEGEN.md for detailed instructions.
/// </summary>
public class ReflectionVsSourceGenConfig : ManualConfig
{
    public ReflectionVsSourceGenConfig()
    {
        // Baseline: Reflection-based dispatch
        // Run after building Blazing.Mediator in Release configuration
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithId("Reflection-NET9")
            .WithPlatform(Platform.X64)
            .AsBaseline());

        // Comparison: Source Generator-based dispatch  
        // Run after building Blazing.Mediator in SourceGen configuration
        // NOTE: The environment variable here is for documentation only.
        // The actual source generation is controlled by which configuration
        // Blazing.Mediator was built with (Release vs SourceGen).
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithId("SourceGen-NET9")
            .WithPlatform(Platform.X64)
            .WithEnvironmentVariable("USE_SOURCE_GENERATORS", "true"));

        // Add memory diagnostics to measure allocation improvements
        AddDiagnoser(MemoryDiagnoser.Default);
        
        // Disable optimization validator warnings
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}
