using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
///     Benchmarks startup and registration performance.
///     Target: 10x improvement (50ms -> 5ms for 100 handlers)
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[MarkdownExporter]
public class StartupPerformanceBenchmark
{
    [Benchmark(Baseline = true, Description = "Startup with Reflection (100 handlers)")]
    public IServiceProvider Startup_Reflection_100Handlers()
    {
        ServiceCollection services = new();

        services.AddMediator();

        // Register 100 mock handlers for realistic load
        for (int i = 0; i < 100; i++)
        {
            Type queryType = typeof(MockQuery<>).MakeGenericType(typeof(int));
            Type handlerType = typeof(MockQueryHandler<>).MakeGenericType(typeof(int));
            Type handlerInterface = typeof(IRequestHandler<,>).MakeGenericType(queryType, typeof(int));

            services.AddScoped(handlerInterface, handlerType);
        }

        return services.BuildServiceProvider();
    }

    [Benchmark(Description = "Startup - DI Resolution Only")]
    public IServiceProvider Startup_DIResolution()
    {
        ServiceCollection services = new();

        // Minimal setup - just DI container
        services.AddScoped<IMediator, Mediator>();

        return services.BuildServiceProvider();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // Clean up service providers to avoid memory leaks
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

#region Mock Types for Startup Testing

public record MockQuery<T>(T Value) : IRequest<T>;

public class MockQueryHandler<T> : IRequestHandler<MockQuery<T>, T>
{
    public async ValueTask<T> Handle(MockQuery<T> request, CancellationToken cancellationToken)
    {
        return request.Value;
    }
}

#endregion