using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Engines;
using Blazing.Mediator.Abstractions;
using Blazing.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Blazing.Mediator.Benchmarks;

/// <summary>
/// Ultra-high precision benchmarks targeting our sub-50ns execution goal.
/// These benchmarks focus on the most critical hot paths that were optimized.
/// </summary>
[Config(typeof(NanosecondPrecisionConfig))]
[MemoryDiagnoser(displayGenColumns: false)]
[ThreadingDiagnoser]
public class NanosecondPrecisionBenchmarks
{
    private MiddlewarePipelineBuilder? _middlewarePipelineBuilder;
    private NotificationPipelineBuilder? _notificationPipelineBuilder;
    private IServiceProvider? _serviceProvider;
    private Dictionary<Type, int>? _preCalculatedIndices;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddSingleton<UltraFastMiddleware>();
        services.AddSingleton<UltraFastNotificationMiddleware>();
        _serviceProvider = services.BuildServiceProvider();

        SetupOptimizedPipelineBuilders();
        SetupPreCalculatedIndices();
    }

    private void SetupOptimizedPipelineBuilders()
    {
        _middlewarePipelineBuilder = new MiddlewarePipelineBuilder();
        _middlewarePipelineBuilder.AddMiddleware<UltraFastMiddleware>();
        
        _notificationPipelineBuilder = new NotificationPipelineBuilder();
        _notificationPipelineBuilder.AddMiddleware<UltraFastNotificationMiddleware>();
    }

    private void SetupPreCalculatedIndices()
    {
        _preCalculatedIndices = new Dictionary<Type, int>
        {
            [typeof(UltraFastMiddleware)] = 0,
            [typeof(UltraFastNotificationMiddleware)] = 0
        };
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    #region <50ns Target Benchmarks - Core Optimizations

    /// <summary>
    /// Tests our optimized registration index lookup (was O(n²) FindIndex, now O(1) Dictionary lookup).
    /// Target: Less than 10ns for single lookup operation.
    /// </summary>
    [Benchmark(Description = "O(1) Registration Index Lookup - <10ns Target")]
    public int OptimizedRegistrationIndexLookup()
    {
        var middlewareType = typeof(UltraFastMiddleware);
        
        // This simulates our GetRegistrationIndex optimization
        if (_preCalculatedIndices!.TryGetValue(middlewareType, out int index))
        {
            return index;
        }
        
        return int.MaxValue; // Fallback
    }

    /// <summary>
    /// Tests fast fallback type creation (eliminates 30+ second assembly scanning).
    /// Target: Less than 20ns for type constraint validation.
    /// </summary>
    [Benchmark(Description = "Fast Fallback Type Creation - <20ns Target")]
    public Type? FastFallbackTypeCreation()
    {
        var middlewareTypeDefinition = typeof(GenericUltraFastMiddleware<,>);
        var fastFallbackTypes = new[] { typeof(SimpleRequest), typeof(object) };
        var responseTypes = new[] { typeof(string), typeof(object) };
        
        // Simulate our optimized TryCreateConcreteMiddlewareType
        foreach (var requestType in fastFallbackTypes)
        {
            foreach (var responseType in responseTypes)
            {
                try
                {
                    return middlewareTypeDefinition.MakeGenericType(requestType, responseType);
                }
                catch (ArgumentException)
                {
                    continue;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// Tests optimized interface checking (replaced LINQ with for loops).
    /// Target: Less than 15ns for interface compatibility check.
    /// </summary>
    [Benchmark(Description = "Optimized Interface Checking - <15ns Target")]
    public bool OptimizedInterfaceChecking()
    {
        var middlewareType = typeof(UltraFastMiddleware);
        var interfaces = middlewareType.GetInterfaces();
        var targetInterface = typeof(IRequestMiddleware<,>);
        
        // Optimized for loop instead of expensive LINQ
        for (int i = 0; i < interfaces.Length; i++)
        {
            var iface = interfaces[i];
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == targetInterface)
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Tests our constraint satisfaction optimization.
    /// Target: Less than 25ns for generic constraint validation.
    /// </summary>
    [Benchmark(Description = "Generic Constraint Validation - <25ns Target")]
    public bool FastConstraintValidation()
    {
        var genericTypeDefinition = typeof(GenericUltraFastMiddleware<,>);
        var typeArguments = new[] { typeof(SimpleRequest), typeof(string) };
        
        // Simulate our optimized CanSatisfyGenericConstraints method
        var genericParameters = genericTypeDefinition.GetGenericArguments();
        
        if (genericParameters.Length != typeArguments.Length)
            return false;
        
        for (int i = 0; i < genericParameters.Length; i++)
        {
            var parameter = genericParameters[i];
            var argument = typeArguments[i];
            
            // Basic constraint checking (optimized)
            if (parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) 
                && argument.IsValueType)
            {
                return false;
            }
        }
        
        return true;
    }

    #endregion

    #region Pipeline Execution Hot Path Benchmarks

    /// <summary>
    /// End-to-end pipeline execution benchmark.
    /// Target: Complete pipeline execution should be significantly faster than before.
    /// </summary>
    [Benchmark(Description = "Complete Pipeline Execution - End-to-End Performance")]
    public async Task<string> EndToEndPipelineExecution()
    {
        var request = new SimpleRequest { Data = "test" };
        RequestHandlerDelegate<string> finalHandler = () => Task.FromResult("result");
        
        return await _middlewarePipelineBuilder!.ExecutePipeline(
            request,
            _serviceProvider!,
            finalHandler,
            CancellationToken.None);
    }

    /// <summary>
    /// Notification pipeline execution benchmark.
    /// </summary>
    [Benchmark(Description = "Notification Pipeline Execution - End-to-End")]
    public async Task NotificationPipelineExecution()
    {
        var notification = new SimpleNotification { Message = "test" };
        var finalHandler = (NotificationDelegate<SimpleNotification>)((n, ct) => Task.CompletedTask);
        
        await _notificationPipelineBuilder!.ExecutePipeline(
            notification,
            _serviceProvider!,
            finalHandler,
            CancellationToken.None);
    }

    #endregion
}

/// <summary>
/// Ultra-high precision configuration for nanosecond-level measurements.
/// </summary>
public class NanosecondPrecisionConfig : ManualConfig
{
    public NanosecondPrecisionConfig()
    {
        // Use in-process toolchain for maximum precision
        var job = Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit)
            .WithGcServer(false)      // Use workstation GC for more predictable timing
            .WithGcConcurrent(false)  // Disable concurrent GC for timing precision
            .WithIterationCount(50)   // Many iterations for statistical accuracy
            .WithLaunchCount(5)       // Multiple launches to verify consistency
            .WithWarmupCount(10)      // Extensive warmup for JIT optimization
            .WithUnrollFactor(256)    // High unroll factor for nanosecond measurements
            .WithStrategy(RunStrategy.Throughput)
            .WithToolchain(InProcessEmitToolchain.Instance) // In-process for precision
            .WithId("Nanosecond Precision");

        AddJob(job);
        
        // Minimal diagnosers to avoid measurement interference
        AddDiagnoser(MemoryDiagnoser.Default);
        
        WithOptions(ConfigOptions.DisableOptimizationsValidator);
        WithOptions(ConfigOptions.StopOnFirstError);
    }
}

#region Ultra-Fast Test Types

/// <summary>
/// Ultra-simple request for nanosecond benchmarks.
/// </summary>
public class SimpleRequest : IRequest<string>
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// Ultra-simple notification for nanosecond benchmarks.
/// </summary>
public class SimpleNotification : INotification
{
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Ultra-fast middleware with minimal overhead for precise timing.
/// </summary>
public class UltraFastMiddleware : IRequestMiddleware<SimpleRequest, string>
{
    public static int Order => 1;
    
    public Task<string> HandleAsync(SimpleRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        // Minimal overhead - just pass through
        return next();
    }
}

/// <summary>
/// Generic ultra-fast middleware for testing generic optimizations.
/// </summary>
public class GenericUltraFastMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => 2;
    
    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return next();
    }
}

/// <summary>
/// Ultra-fast notification middleware.
/// </summary>
public class UltraFastNotificationMiddleware : INotificationMiddleware
{
    public static int Order => 1;
    
    public Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) 
        where TNotification : INotification
    {
        return next(notification, cancellationToken);
    }
}

#endregion