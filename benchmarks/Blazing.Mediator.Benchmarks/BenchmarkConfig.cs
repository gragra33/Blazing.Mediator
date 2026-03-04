using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
///     Configuration for Blazing.Mediator source-generator-based performance benchmarks.
///     Runs on .NET 10 (x64) with memory diagnostics and inlining analysis on Windows.
///     To run:
///     dotnet run --project benchmarks\Blazing.Mediator.Benchmarks -c Release --framework net10.0
/// </summary>
public class ReflectionVsSourceGenConfig : ManualConfig
{
    public ReflectionVsSourceGenConfig()
    {
        // Single job targeting the host process runtime (.NET 10 when launched with --framework net10.0)
        AddJob(Job.Default
            .WithId("SourceGen-NET10")
            .WithPlatform(Platform.X64)
            .AsBaseline());

        // Memory diagnostics to measure allocation profile
        AddDiagnoser(MemoryDiagnoser.Default);

        // Inlining analysis on Windows to verify hot-path optimisations
        if (OperatingSystem.IsWindows())
            AddDiagnoser(new InliningDiagnoser(true, ["Blazing.Mediator"]));

        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}