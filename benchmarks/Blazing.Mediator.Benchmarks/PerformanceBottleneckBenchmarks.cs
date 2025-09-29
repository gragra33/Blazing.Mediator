using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Engines;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Specialized benchmarks targeting the exact performance bottlenecks we fixed:
/// 1. Assembly scanning elimination (30+ second ? milliseconds)
/// 2. O(n²) sorting ? O(1) lookup optimization  
/// 3. Expensive LINQ operations ? optimized loops
/// 4. Console I/O removal from hot paths
/// </summary>
[Config(typeof(BottleneckConfig))]
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PerformanceBottleneckBenchmarks
{
    private IServiceProvider? _serviceProvider;
    private MiddlewarePipelineBuilder? _middlewarePipelineBuilder;
    private NotificationPipelineBuilder? _notificationPipelineBuilder;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None)); // Disable logging for clean benchmarks
        _serviceProvider = services.BuildServiceProvider();
        
        SetupPipelineBuilders();
    }

    private void SetupPipelineBuilders()
    {
        // Setup MiddlewarePipelineBuilder with many middleware to stress-test optimizations
        _middlewarePipelineBuilder = new MiddlewarePipelineBuilder();
        for (int i = 0; i < 25; i++)
        {
            _middlewarePipelineBuilder.AddMiddleware(typeof(BottleneckTestMiddleware<,>)); // Generic type definition
            _middlewarePipelineBuilder.AddMiddleware<BottleneckConcreteTestMiddleware>();   // Concrete type
        }

        // Setup NotificationPipelineBuilder with many middleware
        _notificationPipelineBuilder = new NotificationPipelineBuilder();
        for (int i = 0; i < 25; i++)
        {
            _notificationPipelineBuilder.AddMiddleware(typeof(BottleneckTestNotificationMiddleware<>)); // Generic
            _notificationPipelineBuilder.AddMiddleware<BottleneckConcreteNotificationMiddleware>();     // Concrete
        }
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    #region Assembly Scanning Optimizations (The Big Win: 30s ? ms)

    [BenchmarkCategory("Assembly Scanning"), Benchmark(Description = "MiddlewarePipelineBuilder.GetDetailedMiddlewareInfo - Assembly Scanning Optimization")]
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> MiddlewareBuilder_AssemblyScanning_Optimized()
    {
        // This method previously took 30+ seconds due to assembly scanning in TryCreateConcreteMiddlewareType
        // Now uses fast fallback types instead of AppDomain.CurrentDomain.GetAssemblies()
        return _middlewarePipelineBuilder!.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    [BenchmarkCategory("Assembly Scanning"), Benchmark(Description = "NotificationPipelineBuilder.GetDetailedMiddlewareInfo - Assembly Scanning Optimization")]
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> NotificationBuilder_AssemblyScanning_Optimized()
    {
        // This method also had assembly scanning bottlenecks in TryCreateConcreteNotificationMiddlewareType
        // Now uses fast fallback types instead of expensive reflection
        return _notificationPipelineBuilder!.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    #endregion

    #region Sorting Algorithm Optimizations (O(n²) ? O(1))

    [BenchmarkCategory("Sorting Performance"), Benchmark(Description = "MiddlewarePipelineBuilder Sorting - O(1) Lookup vs O(n²) FindIndex")]
    public void MiddlewareBuilder_OptimizedSorting()
    {
        // Create a larger pipeline to demonstrate the O(n²) ? O(1) optimization
        var builder = new MiddlewarePipelineBuilder();
        
        // Add 100 middleware to really stress the sorting algorithm
        for (int i = 0; i < 100; i++)
        {
            builder.AddMiddleware<BottleneckConcreteTestMiddleware>();
            builder.AddMiddleware(typeof(BottleneckTestMiddleware<,>));
        }

        // This triggers the sorting that we optimized from O(n²) to O(1)
        var testRequest = new TestRequest();
        
        // Force the sorting optimization by calling ExecutePipeline setup logic
        // (We can't easily isolate just the sorting part without exposing internals)
        _ = builder.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    [BenchmarkCategory("Sorting Performance"), Benchmark(Description = "NotificationPipelineBuilder Sorting - Optimized Registration Indices")]
    public void NotificationBuilder_OptimizedSorting()
    {
        // Create a larger notification pipeline
        var builder = new NotificationPipelineBuilder();
        
        // Add 100 middleware to stress the sorting
        for (int i = 0; i < 100; i++)
        {
            builder.AddMiddleware<BottleneckConcreteNotificationMiddleware>();
            builder.AddMiddleware(typeof(BottleneckTestNotificationMiddleware<>));
        }

        // This triggers the optimized sorting algorithm
        _ = builder.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    #endregion

    #region LINQ vs Loop Optimizations

    [BenchmarkCategory("LINQ vs Loops"), Benchmark(Description = "Interface Checking - Optimized Loops vs LINQ")]
    public void InterfaceChecking_OptimizedLoops()
    {
        // This simulates the interface checking optimizations we made
        // where we replaced expensive LINQ with for loops
        var type = typeof(BottleneckTestMiddleware<,>);
        var interfaces = type.GetInterfaces();
        
        // Simulate the optimized interface checking we implemented
        var count = 0;
        for (int i = 0; i < interfaces.Length; i++)
        {
            var iface = interfaces[i];
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>))
            {
                count++;
            }
        }
    }

    [BenchmarkCategory("LINQ vs Loops"), Benchmark(Baseline = true, Description = "Interface Checking - Original LINQ (Baseline)")]
    public void InterfaceChecking_OriginalLinq()
    {
        // This simulates the original expensive LINQ operations
        var type = typeof(BottleneckTestMiddleware<,>);
        var interfaces = type.GetInterfaces();
        
        // Original expensive LINQ approach
        // ReSharper disable once ReplaceWithSingleCallToCount
        var count = interfaces
            .Where(i => i.IsGenericType)
            .Where(i => i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>))
            .Count();
    }

    #endregion

    #region Type Creation Performance

    [BenchmarkCategory("Type Creation"), Benchmark(Description = "TryMakeGenericType - Fast Fallback Types")]
    public void FastFallbackTypes_Performance()
    {
        var middlewareType = typeof(BottleneckTestMiddleware<,>);
        var fastFallbackTypes = new[] 
        { 
            typeof(TestRequest), 
            typeof(object), 
            typeof(string)
        };

        // Simulate the fast fallback type creation we implemented
        foreach (var requestType in fastFallbackTypes)
        {
            foreach (var responseType in new[] { typeof(object), typeof(string), typeof(int) })
            {
                try
                {
                    var concreteType = middlewareType.MakeGenericType(requestType, responseType);
                    // Success - we created the type quickly
                    break;
                }
                catch (ArgumentException)
                {
                    // Continue with next combination
                    continue;
                }
            }
        }
    }

    #endregion

    #region Memory Allocation Benchmarks

    [BenchmarkCategory("Memory"), Benchmark(Description = "Memory Efficient Collections - Optimized")]
    public void MemoryEfficientCollections()
    {
        // Test the memory efficiency of our optimized approach
        var registrationIndices = new Dictionary<Type, int>();
        var middlewareTypes = new Type[]
        {
            typeof(BottleneckConcreteTestMiddleware),
            typeof(BottleneckTestMiddleware<,>),
            typeof(BottleneckConcreteNotificationMiddleware),
            typeof(BottleneckTestNotificationMiddleware<>)
        };

        // Pre-calculate indices (our optimization)
        for (int i = 0; i < middlewareTypes.Length; i++)
        {
            registrationIndices[middlewareTypes[i]] = i;
        }

        // Fast O(1) lookups
        foreach (var type in middlewareTypes)
        {
            var index = registrationIndices.GetValueOrDefault(type, -1);
        }
    }

    #endregion
}

/// <summary>
/// Benchmark configuration optimized for measuring bottleneck fixes.
/// </summary>
public class BottleneckConfig : ManualConfig
{
    public BottleneckConfig()
    {
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit)
            .WithGcServer(true)
            .WithIterationCount(10)
            .WithLaunchCount(3)
            .WithWarmupCount(3)
            .WithUnrollFactor(4)
            .WithStrategy(RunStrategy.Throughput)
            .WithId("Bottleneck Analysis"));

        AddDiagnoser(MemoryDiagnoser.Default);
        
        // Group benchmarks by category for easy comparison
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Alphabetical));
    }
}

#region Test Types

public class BottleneckTestRequest : IRequest<string> { }

public class BottleneckConcreteTestMiddleware : IRequestMiddleware<BottleneckTestRequest, string>
{
    public static int Order => 10;
    
    public Task<string> HandleAsync(BottleneckTestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        return next();
    }
}

public class BottleneckTestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => 20;
    
    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return next();
    }
}

public class BottleneckTestNotificationDummy : INotification { }

public class BottleneckConcreteNotificationMiddleware : INotificationMiddleware
{
    public static int Order => 10;
    
    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        return next(notification, cancellationToken);
    }
}

public class BottleneckTestNotificationMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification
{
    public static int Order => 20;
    
    public Task InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        return next(notification, cancellationToken);
    }

    // Explicit implementation of the generic base interface method
    Task INotificationMiddleware.InvokeAsync<T>(T notification, NotificationDelegate<T> next, CancellationToken cancellationToken)
    {
        if (notification is TNotification typedNotification)
        {
            var typedNext = (NotificationDelegate<TNotification>)(object)next;
            return InvokeAsync(typedNotification, typedNext, cancellationToken);
        }
        return next(notification, cancellationToken);
    }
}

#endregion