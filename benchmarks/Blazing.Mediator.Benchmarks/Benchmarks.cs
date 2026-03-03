using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

[Config(typeof(ReflectionVsSourceGenConfig))]
[DotTraceDiagnoser]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Benchmarks
{
    private readonly Pinged _notification = new();

    private readonly Ping _request = new() { Message = "Hello World" };
    private readonly BenchmarkStreamRequest _streamRequest = new() { Count = 10 };
    private IMediator _mediator = null!;

    // Mediators with different middleware counts
    private IMediator _mediator0Middleware = null!;
    private IMediator _mediator10Middleware = null!;
    private IMediator _mediator1Middleware = null!;
    private IMediator _mediator5Middleware = null!;
    private IMediator _mediatorSubscriberOnly = null!;
    private IMediator _mediatorSubscriberWithTelemetry = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private PingedSubscriber _subscriber = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup mediator without telemetry (baseline) - with handlers
        ServiceCollection services = new();
        services.AddSingleton(TextWriter.Null);
        services.AddScoped<IRequestHandler<Ping>, PingHandler>();
        services.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        MediatorConfiguration mediatorConfig = new();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);

        ServiceProvider provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();

        // Setup mediator with minimal telemetry - with handlers
        ServiceCollection servicesWithTelemetry = new();
        servicesWithTelemetry.AddSingleton(TextWriter.Null);
        servicesWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>();
        servicesWithTelemetry.AddScoped<INotificationHandler<Pinged>, PingedHandler>();

        MediatorConfiguration mediatorConfigWithTelemetry = new();
        mediatorConfigWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesWithTelemetry.AddMediator(mediatorConfigWithTelemetry);

        ServiceProvider providerWithTelemetry = servicesWithTelemetry.BuildServiceProvider();
        _mediatorWithTelemetry = providerWithTelemetry.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests (no notification handlers)
        ServiceCollection servicesSubscriberOnly = new();
        servicesSubscriberOnly.AddSingleton(TextWriter.Null);
        servicesSubscriberOnly.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        MediatorConfiguration mediatorConfigSubscriberOnly = new();
        mediatorConfigSubscriberOnly.WithoutTelemetry();
        servicesSubscriberOnly.AddMediator(mediatorConfigSubscriberOnly);

        ServiceProvider providerSubscriberOnly = servicesSubscriberOnly.BuildServiceProvider();
        _mediatorSubscriberOnly = providerSubscriberOnly.GetRequiredService<IMediator>();

        // Setup mediator for subscriber-only tests with telemetry
        ServiceCollection servicesSubscriberWithTelemetry = new();
        servicesSubscriberWithTelemetry.AddSingleton(TextWriter.Null);
        servicesSubscriberWithTelemetry.AddScoped<IRequestHandler<Ping>, PingHandler>(); // Only Ping handler

        MediatorConfiguration mediatorConfigSubscriberWithTelemetry = new();
        mediatorConfigSubscriberWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesSubscriberWithTelemetry.AddMediator(mediatorConfigSubscriberWithTelemetry);

        ServiceProvider providerSubscriberWithTelemetry = servicesSubscriberWithTelemetry.BuildServiceProvider();
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
        ServiceCollection services = new();
        services.AddSingleton(TextWriter.Null);

        // Register handlers
        services.AddScoped<IRequestHandler<Ping>, PingHandler>();
        services.AddScoped<INotificationHandler<Pinged>, PingedHandler>();
        services.AddScoped<IStreamRequestHandler<BenchmarkStreamRequest, string>, BenchmarkStreamHandler>();

        // Add mediator with specified middleware count
        MediatorConfiguration mediatorConfig = new();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);

        // Add the specified number of no-op middleware layers
        for (int i = 0; i < middlewareCount; i++)
            services.AddScoped(typeof(IRequestMiddleware<,>), typeof(NoOpMiddleware<,>));

        ServiceProvider provider = services.BuildServiceProvider();
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
        await foreach (string item in _mediator0Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (1 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_1Middleware()
    {
        await foreach (string item in _mediator1Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (5 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_5Middleware()
    {
        await foreach (string item in _mediator5Middleware.SendStream(_streamRequest))
        {
            // Process item
        }
    }

    [Benchmark(Description = "SendStream (10 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async Task SendStream_10Middleware()
    {
        await foreach (string item in _mediator10Middleware.SendStream(_streamRequest))
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
///     Lightweight no-op middleware that just passes through the pipeline.
///     Used to measure pure pipeline overhead without any processing logic.
/// </summary>
public class NoOpMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public int Order => 0;

    public ValueTask<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Minimal overhead - just pass through
        return next();
    }
}

/// <summary>
///     Test stream request for benchmarking.
/// </summary>
public class BenchmarkStreamRequest : IStreamRequest<string>
{
    public int Count { get; set; }
}

/// <summary>
///     Handler for BenchmarkStreamRequest.
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