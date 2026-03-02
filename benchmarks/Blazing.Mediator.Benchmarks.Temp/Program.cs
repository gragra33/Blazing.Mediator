using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Blazing.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks.Temp;

/// <summary>
/// Simplified benchmark to validate reflection vs source generation.
/// Run with: dotnet run -c Release  (for reflection baseline)
/// Run with: dotnet run -c SourceGen (for source generation)
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("BLAZING.MEDIATOR VALIDATION BENCHMARK");
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine();

#if USE_SOURCE_GENERATORS
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ USE_SOURCE_GENERATORS is DEFINED");
        Console.WriteLine("  This build should use generated dispatch code");
        Console.ResetColor();
#else
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("✗ USE_SOURCE_GENERATORS is NOT defined");
        Console.WriteLine("  This build will use reflection-based dispatch");
        Console.ResetColor();
#endif
        
        Console.WriteLine();
        Console.WriteLine("Build Configuration: " + 
#if DEBUG
            "DEBUG"
#else
            "RELEASE"
#endif
        );
        Console.WriteLine();
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine();

        var summary = BenchmarkRunner.Run<SimpleSendBenchmark>();
        
        Console.WriteLine();
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("BENCHMARK COMPLETE");
        Console.WriteLine("=".PadRight(60, '='));
    }
}

/// <summary>
/// Simple request for testing
/// </summary>
public record TestRequest(string Message) : IRequest<string>;

/// <summary>
/// Simple handler for testing
/// </summary>
public class TestRequestHandler : IRequestHandler<TestRequest, string>
{
    public async ValueTask<string> Handle(TestRequest request, CancellationToken cancellationToken)
    {
        return $"Handled: {request.Message}";
    }
}

/// <summary>
/// Benchmark configuration - single job to test current build
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[Config(typeof(NoOptimizationValidatorConfig))]
public class SimpleSendBenchmark
{
    private IMediator _mediator = null!;
    private TestRequest _request = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Register mediator with handler discovery
        services.AddMediator();

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _request = new TestRequest("Test");

        // Warm up
        _ = _mediator.Send(_request).GetAwaiter().GetResult();
        
        Console.WriteLine();
        Console.WriteLine("Setup complete. Mediator initialized.");
        Console.WriteLine($"Request Type: {_request.GetType().Name}");
        Console.WriteLine($"Handler Type: {typeof(TestRequestHandler).Name}");
        Console.WriteLine();
    }

    [Benchmark]
    public async Task<string> Send_TestRequest()
    {
        return await _mediator.Send(_request);
    }

    [GlobalCleanup]
    public static void Cleanup()
    {
        Console.WriteLine();
        Console.WriteLine("Benchmark iterations complete.");
    }
}

/// <summary>
/// Configuration to disable optimization validator
/// </summary>
public class NoOptimizationValidatorConfig : ManualConfig
{
    public NoOptimizationValidatorConfig()
    {
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}
