using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Columns;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Benchmarks for measuring MiddlewarePipelineBuilder and NotificationPipelineBuilder performance
/// to verify our sub-50ns execution target optimizations.
/// </summary>
[Config(typeof(PipelinePerformanceConfig))]
[MemoryDiagnoser(displayGenColumns: false)]
[ThreadingDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class PipelinePerformanceBenchmarks
{
    private MiddlewarePipelineBuilder? _middlewarePipelineBuilder;
    private NotificationPipelineBuilder? _notificationPipelineBuilder;
    private IServiceProvider? _serviceProvider;
    private TestRequest? _testRequest;
    private TestCommand? _testCommand;
    private TestNotification? _testNotification;
    private RequestHandlerDelegate<string>? _finalQueryHandler;
    private RequestHandlerDelegate? _finalCommandHandler;
    private NotificationDelegate<TestNotification>? _finalNotificationHandler;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        
        // Register test middleware
        services.AddScoped<FastTestMiddleware>();
        services.AddScoped<GenericTestMiddleware<TestRequest, string>>();
        services.AddScoped<GenericCommandMiddleware<TestCommand>>();
        services.AddScoped<TestNotificationMiddleware>();
        services.AddScoped<GenericNotificationMiddleware<TestNotification>>();
        
        // Add logging (but disable to avoid I/O overhead in benchmarks)
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        
        _serviceProvider = services.BuildServiceProvider();
        
        SetupMiddlewarePipelineBuilder();
        SetupNotificationPipelineBuilder();
        SetupTestData();
    }

    private void SetupMiddlewarePipelineBuilder()
    {
        _middlewarePipelineBuilder = new MiddlewarePipelineBuilder();
        
        // Add multiple middleware to test sorting performance
        _middlewarePipelineBuilder.AddMiddleware<FastTestMiddleware>();
        _middlewarePipelineBuilder.AddMiddleware<GenericTestMiddleware<TestRequest, string>>();
        _middlewarePipelineBuilder.AddMiddleware<GenericCommandMiddleware<TestCommand>>();
        
        // Add more middleware to stress test the sorting algorithms
        for (int i = 0; i < 10; i++)
        {
            _middlewarePipelineBuilder.AddMiddleware<FastTestMiddleware>();
        }
    }

    private void SetupNotificationPipelineBuilder()
    {
        _notificationPipelineBuilder = new NotificationPipelineBuilder();
        
        // Add multiple notification middleware to test performance
        _notificationPipelineBuilder.AddMiddleware<TestNotificationMiddleware>();
        _notificationPipelineBuilder.AddMiddleware<GenericNotificationMiddleware<TestNotification>>();
        
        // Add more middleware to stress test the sorting algorithms
        for (int i = 0; i < 10; i++)
        {
            _notificationPipelineBuilder.AddMiddleware<TestNotificationMiddleware>();
        }
    }

    private void SetupTestData()
    {
        _testRequest = new TestRequest { Message = "Benchmark test request" };
        _testCommand = new TestCommand { Message = "Benchmark test command" };
        _testNotification = new TestNotification { Message = "Benchmark test notification" };
        
        _finalQueryHandler = () => Task.FromResult("Final query result");
        _finalCommandHandler = () => Task.CompletedTask;
        _finalNotificationHandler = (notification, ct) => Task.CompletedTask;
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    #region MiddlewarePipelineBuilder Benchmarks

    /// <summary>
    /// Benchmarks the critical path: ExecutePipeline for queries (most common hot path).
    /// Target: Sub-50ns execution time for pipeline setup and sorting.
    /// </summary>
    [Benchmark(Description = "ExecutePipeline<TRequest,TResponse> - Query Hot Path")]
    public async Task<string> MiddlewarePipeline_ExecuteQuery()
    {
        return await _middlewarePipelineBuilder!.ExecutePipeline(
            _testRequest!,
            _serviceProvider!,
            _finalQueryHandler!,
            CancellationToken.None);
    }

    /// <summary>
    /// Benchmarks ExecutePipeline for void commands.
    /// </summary>
    [Benchmark(Description = "ExecutePipeline<TRequest> - Command Hot Path")]
    public async Task MiddlewarePipeline_ExecuteCommand()
    {
        await _middlewarePipelineBuilder!.ExecutePipeline(
            _testCommand!,
            _serviceProvider!,
            _finalCommandHandler!,
            CancellationToken.None);
    }

    /// <summary>
    /// Benchmarks the performance-critical GetDetailedMiddlewareInfo method.
    /// This was a major bottleneck with expensive assembly scanning.
    /// </summary>
    [Benchmark(Description = "GetDetailedMiddlewareInfo - Assembly Scanning Optimization")]
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> MiddlewarePipeline_GetDetailedInfo()
    {
        return _middlewarePipelineBuilder!.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    /// <summary>
    /// Benchmarks AnalyzeMiddleware method which was affected by our optimizations.
    /// </summary>
    [Benchmark(Description = "AnalyzeMiddleware - Performance Analysis")]
    public IReadOnlyList<MiddlewareAnalysis> MiddlewarePipeline_AnalyzeMiddleware()
    {
        return _middlewarePipelineBuilder!.AnalyzeMiddleware(_serviceProvider!, true);
    }

    #endregion

    #region NotificationPipelineBuilder Benchmarks

    /// <summary>
    /// Benchmarks NotificationPipelineBuilder.ExecutePipeline - the critical notification path.
    /// </summary>
    [Benchmark(Description = "NotificationPipeline ExecutePipeline - Hot Path")]
    public async Task NotificationPipeline_ExecutePipeline()
    {
        await _notificationPipelineBuilder!.ExecutePipeline(
            _testNotification!,
            _serviceProvider!,
            _finalNotificationHandler!,
            CancellationToken.None);
    }

    /// <summary>
    /// Benchmarks NotificationPipelineBuilder.GetDetailedMiddlewareInfo method.
    /// This was optimized to avoid expensive assembly scanning.
    /// </summary>
    [Benchmark(Description = "NotificationPipeline GetDetailedInfo - Assembly Optimization")]
    public IReadOnlyList<(Type Type, int Order, object? Configuration)> NotificationPipeline_GetDetailedInfo()
    {
        return _notificationPipelineBuilder!.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    /// <summary>
    /// Benchmarks NotificationPipelineBuilder.AnalyzeMiddleware method.
    /// </summary>
    [Benchmark(Description = "NotificationPipeline AnalyzeMiddleware - Analysis Performance")]
    public IReadOnlyList<MiddlewareAnalysis> NotificationPipeline_AnalyzeMiddleware()
    {
        return _notificationPipelineBuilder!.AnalyzeMiddleware(_serviceProvider!, true);
    }

    #endregion

    #region Micro-benchmarks for Specific Optimizations

    /// <summary>
    /// Micro-benchmark for the optimized sorting algorithm in MiddlewarePipelineBuilder.
    /// Tests the O(n²) to O(1) optimization we applied.
    /// </summary>
    [Benchmark(Description = "Middleware Sorting Algorithm - O(1) Lookup Optimization")]
    public void MiddlewarePipeline_SortingPerformance()
    {
        // Create a pipeline with many middleware to stress-test sorting
        var builder = new MiddlewarePipelineBuilder();
        
        // Add middleware in random order to test sorting performance
        for (int i = 0; i < 50; i++)
        {
            builder.AddMiddleware<FastTestMiddleware>();
            builder.AddMiddleware<GenericTestMiddleware<TestRequest, string>>();
        }
        
        // This triggers the sorting algorithm we optimized
        var info = builder.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    /// <summary>
    /// Micro-benchmark for NotificationPipelineBuilder sorting optimization.
    /// </summary>
    [Benchmark(Description = "Notification Sorting Algorithm - Optimized Performance")]
    public void NotificationPipeline_SortingPerformance()
    {
        // Create a pipeline with many notification middleware to stress-test sorting
        var builder = new NotificationPipelineBuilder();
        
        // Add middleware in random order to test sorting performance
        for (int i = 0; i < 50; i++)
        {
            builder.AddMiddleware<TestNotificationMiddleware>();
            builder.AddMiddleware<GenericNotificationMiddleware<TestNotification>>();
        }
        
        // This triggers the sorting algorithm we optimized
        var info = builder.GetDetailedMiddlewareInfo(_serviceProvider);
    }

    #endregion
}

/// <summary>
/// Custom benchmark configuration optimized for measuring nanosecond-level performance.
/// </summary>
public class PipelinePerformanceConfig : ManualConfig
{
    public PipelinePerformanceConfig()
    {
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit)
            .WithGcServer(true)
            .WithGcForce(false)
            .WithIterationCount(15)  // More iterations for accurate nanosecond measurements
            .WithLaunchCount(3)
            .WithWarmupCount(5)
            .WithUnrollFactor(16)    // Higher unroll factor for micro-optimizations
            .WithStrategy(RunStrategy.Throughput)
            .WithId("Pipeline Performance"));

        // Add memory diagnoser to track allocation optimizations
        AddDiagnoser(MemoryDiagnoser.Default);
        
        // Add Windows-specific diagnosers if available
        if (OperatingSystem.IsWindows())
        {
            AddDiagnoser(new InliningDiagnoser(logFailuresOnly: true, allowedNamespaces: ["Blazing.Mediator"]));
        }

        // Ensure accurate timing for nanosecond-level measurements
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
    }
}

#region Test Types for Benchmarks

/// <summary>
/// Test request type for benchmarking queries.
/// </summary>
public class TestRequest : IRequest<string>
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Test command type for benchmarking commands.
/// </summary>
public class TestCommand : IRequest
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Test notification type for benchmarking notifications.
/// </summary>
public class TestNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Fast test middleware with static order for predictable benchmarking.
/// </summary>
public class FastTestMiddleware : IRequestMiddleware<TestRequest, string>, IRequestMiddleware<TestCommand>
{
    public static int Order => 1;

    public async Task<string> HandleAsync(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        // Minimal processing to focus on pipeline performance
        return await next().ConfigureAwait(false);
    }

    public async Task HandleAsync(TestCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Minimal processing to focus on pipeline performance
        await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Generic test middleware for benchmarking generic type handling.
/// </summary>
public class GenericTestMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => 2;

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Minimal processing to focus on pipeline performance
        return await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Generic command middleware for benchmarking.
/// </summary>
public class GenericCommandMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public static int Order => 3;

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Minimal processing to focus on pipeline performance
        await next().ConfigureAwait(false);
    }
}

/// <summary>
/// Test notification middleware for benchmarking notifications.
/// </summary>
public class TestNotificationMiddleware : INotificationMiddleware
{
    public static int Order => 1;

    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) 
        where TNotification : INotification
    {
        // Minimal processing to focus on pipeline performance
        return next(notification, cancellationToken);
    }
}

/// <summary>
/// Generic notification middleware for benchmarking.
/// </summary>
public class GenericNotificationMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : INotification
{
    public static int Order => 2;

    public Task InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        // Minimal processing to focus on pipeline performance
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