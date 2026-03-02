using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using Blazing.Mediator.Configuration;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Benchmarks;

[Config(typeof(ReflectionVsSourceGenConfig))]
[DotTraceDiagnoser]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Benchmarks
{
    private IMediator _mediator = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private IMediator _mediatorSubscriberOnly = null!;
    private IMediator _mediatorSubscriberWithTelemetry = null!;
    
    // Mediators with different middleware counts
    private IMediator _mediator0Middleware = null!;
    private IMediator _mediator1Middleware = null!;
    private IMediator _mediator5Middleware = null!;
    private IMediator _mediator10Middleware = null!;
    
    private readonly Ping _request = new() { Message = "Hello World" };
    private readonly Pinged _notification = new();
    private readonly BenchmarkStreamRequest _streamRequest = new() { Count = 10 };
    private PingedSubscriber _subscriber = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup mediator without telemetry (baseline) - with handlers
        var services = new ServiceCollection();
        services.AddSingleton(TextWriter.Null);
        services.AddScoped<IRequestHandler<Ping>, PingHandler>();
        services.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        var mediatorConfig = new MediatorConfiguration();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();

        // Setup mediator with minimal telemetry - with handlers
        var servicesWithTelemetry = new ServiceCollection();
        servicesWithTelemetry.AddSingleton(TextWriter.Null);
        servicesWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>();
        servicesWithTelemetry.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        var mediatorConfigWithTelemetry = new MediatorConfiguration();
        mediatorConfigWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesWithTelemetry.AddMediator(mediatorConfigWithTelemetry);

        var providerWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _mediatorWithTelemetry = providerWithTelemetry.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests (no notification handlers)
        var servicesSubscriberOnly = new ServiceCollection();
        servicesSubscriberOnly.AddSingleton(TextWriter.Null);
        servicesSubscriberOnly.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        var mediatorConfigSubscriberOnly = new MediatorConfiguration();
        mediatorConfigSubscriberOnly.WithoutTelemetry();
        servicesSubscriberOnly.AddMediator(mediatorConfigSubscriberOnly);

        var providerSubscriberOnly = servicesSubscriberOnly.BuildServiceProvider();
        _mediatorSubscriberOnly = providerSubscriberOnly.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests with telemetry
        var servicesSubscriberWithTelemetry = new ServiceCollection();
        servicesSubscriberWithTelemetry.AddSingleton(TextWriter.Null);
        servicesSubscriberWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        var mediatorConfigSubscriberWithTelemetry = new MediatorConfiguration();
        mediatorConfigSubscriberWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesSubscriberWithTelemetry.AddMediator(mediatorConfigSubscriberWithTelemetry);

        var providerSubscriberWithTelemetry = servicesSubscriberWithTelemetry.BuildServiceProvider();
        _mediatorSubscriberWithTelemetry = providerSubscriberWithTelemetry.GetRequiredService<IMediator>();

        // Setup subscriber for subscriber-only notification benchmarks
        _subscriber = new PingedSubscriber();
        _mediatorSubscriberOnly.Subscribe(_subscriber);
        _mediatorSubscriberWithTelemetry.Subscribe(_subscriber);

        // Setup mediators with different middleware counts
        _mediator0Middleware = CreateMediatorWithMiddleware(0);
        _mediator1Middleware = CreateMediatorWithMiddleware(1);
        _mediator5Middleware = CreateMediatorWithMiddleware(5);
        _mediator10Middleware = CreateMediatorWithMiddleware(10);
    }

    private static IMediator CreateMediatorWithMiddleware(int middlewareCount)
    {
        var services = new ServiceCollection();
        services.AddSingleton(TextWriter.Null);
        
        // Register handlers
        services.AddScoped<IRequestHandler<Ping>, PingHandler>();
        services.AddScoped<INotificationHandler<Pinged>, PingedHandler>();
        services.AddScoped<IStreamRequestHandler<BenchmarkStreamRequest, string>, BenchmarkStreamHandler>();

        // Add mediator with specified middleware count
        var mediatorConfig = new MediatorConfiguration();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);

        // Add the specified number of no-op middleware layers
        for (int i = 0; i < middlewareCount; i++)
        {
            services.AddScoped(typeof(IRequestMiddleware<,>), typeof(NoOpMiddleware<,>));
        }

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    #region Original Benchmarks

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask SendRequests()
    {
        return _mediator.Send(_request);
    }

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask SendRequestsWithTelemetry()
    {
        return _mediatorWithTelemetry.Send(_request);
    }

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask PublishToHandlers()
    {
        return _mediator.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask PublishToHandlersWithTelem()
    {
        return _mediatorWithTelemetry.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask PublishToSubscribers()
    {
        return _mediatorSubscriberOnly.Publish(_notification);
    }

    [Benchmark]
    [BenchmarkCategory("Original")]
    public ValueTask PublishToSubscribersTelem()
    {
        return _mediatorSubscriberWithTelemetry.Publish(_notification);
    }

    #endregion

    #region Send Benchmarks with Middleware

    [Benchmark(Baseline = true, Description = "Send (0 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public async Task Send_0Middleware()
    {
        await _mediator0Middleware.Send(_request);
    }

    [Benchmark(Description = "Send (1 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public async Task Send_1Middleware()
    {
        await _mediator1Middleware.Send(_request);
    }

    [Benchmark(Description = "Send (5 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public async Task Send_5Middleware()
    {
        await _mediator5Middleware.Send(_request);
    }

    [Benchmark(Description = "Send (10 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public async Task Send_10Middleware()
    {
        await _mediator10Middleware.Send(_request);
    }

    #endregion

    #region SendStream Benchmarks with Middleware

    [Benchmark(Baseline = true, Description = "SendStream (0 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_0Middleware()
    {
        await foreach (var item in _mediator0Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (1 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_1Middleware()
    {
        await foreach (var item in _mediator1Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (5 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_5Middleware()
    {
        await foreach (var item in _mediator5Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (10 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_10Middleware()
    {
        await foreach (var item in _mediator10Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    #endregion

    #region Publish Benchmarks with Middleware

    [Benchmark(Baseline = true, Description = "Publish (0 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public async Task Publish_0Middleware()
    {
        await _mediator0Middleware.Publish(_notification);
    }

    [Benchmark(Description = "Publish (1 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public async Task Publish_1Middleware()
    {
        await _mediator1Middleware.Publish(_notification);
    }

    [Benchmark(Description = "Publish (5 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public async Task Publish_5Middleware()
    {
        await _mediator5Middleware.Publish(_notification);
    }

    [Benchmark(Description = "Publish (10 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public async Task Publish_10Middleware()
    {
        await _mediator10Middleware.Publish(_notification);
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Lightweight no-op middleware that just passes through the pipeline.
/// Used to measure pure pipeline overhead without any processing logic.
/// </summary>
public class NoOpMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Minimal overhead - just pass through
        return next();
    }
}

/// <summary>
/// Test stream request for benchmarking.
/// </summary>
public class BenchmarkStreamRequest : IStreamRequest<string>
{
    public int Count { get; set; }
}

/// <summary>
/// Handler for BenchmarkStreamRequest.
/// </summary>
public class BenchmarkStreamHandler : IStreamRequestHandler<BenchmarkStreamRequest, string>
{
    public async IAsyncEnumerable<string> Handle(
        BenchmarkStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < request.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield(); // Minimal async work
            yield return $"Item-{i}";
        }
    }
}

#endregion
