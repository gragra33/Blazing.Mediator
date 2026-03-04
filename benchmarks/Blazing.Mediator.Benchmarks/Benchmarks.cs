using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Blazing.Mediator.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class Benchmarks
{
    private readonly Pinged _notification = new();
    private readonly Ping _request = new() { Message = "Hello World" };
    private readonly BenchmarkStreamRequest _streamRequest = new() { Count = 10 };

    private IMediator _mediator = null!;
    private IMediator _mediatorWithTelemetry = null!;
    private IMediator _mediatorSubscriberOnly = null!;
    private IMediator _mediatorSubscriberWithTelemetry = null!;

    // Mediators with different middleware counts
    private IMediator _mediator0Middleware = null!;
    private IMediator _mediator1Middleware = null!;
    private IMediator _mediator5Middleware = null!;
    private IMediator _mediator10Middleware = null!;

    private PingedSubscriber _subscriber = null!;

    // Service providers and scopes — disposed in GlobalCleanup
    private readonly List<ServiceProvider> _providers = new();
    private readonly List<IServiceScope> _scopes = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Baseline: no telemetry
        ServiceCollection services = new();
        services.AddSingleton(TextWriter.Null);
        MediatorConfiguration mediatorConfig = new();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);
        _mediator = ResolveFromScope(services);

        // With minimal telemetry
        ServiceCollection servicesWithTelemetry = new();
        servicesWithTelemetry.AddSingleton(TextWriter.Null);
        MediatorConfiguration mediatorConfigWithTelemetry = new();
        mediatorConfigWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesWithTelemetry.AddMediator(mediatorConfigWithTelemetry);
        _mediatorWithTelemetry = ResolveFromScope(servicesWithTelemetry);

        // Subscriber-only (no telemetry)
        ServiceCollection servicesSubscriberOnly = new();
        servicesSubscriberOnly.AddSingleton(TextWriter.Null);
        MediatorConfiguration mediatorConfigSubscriberOnly = new();
        mediatorConfigSubscriberOnly.WithoutTelemetry();
        servicesSubscriberOnly.AddMediator(mediatorConfigSubscriberOnly);
        _mediatorSubscriberOnly = ResolveFromScope(servicesSubscriberOnly);

        // Subscriber-only with telemetry
        ServiceCollection servicesSubscriberWithTelemetry = new();
        servicesSubscriberWithTelemetry.AddSingleton(TextWriter.Null);
        MediatorConfiguration mediatorConfigSubscriberWithTelemetry = new();
        mediatorConfigSubscriberWithTelemetry.WithTelemetry(TelemetryOptions.Minimal());
        servicesSubscriberWithTelemetry.AddMediator(mediatorConfigSubscriberWithTelemetry);
        _mediatorSubscriberWithTelemetry = ResolveFromScope(servicesSubscriberWithTelemetry);

        // Subscribe once; both mediators share the same subscriber instance
        _subscriber = new PingedSubscriber();
        _mediatorSubscriberOnly.Subscribe(_subscriber);
        _mediatorSubscriberWithTelemetry.Subscribe(_subscriber);

        // Mediators with different middleware counts
        _mediator0Middleware = CreateMediatorWithMiddleware(0);
        _mediator1Middleware = CreateMediatorWithMiddleware(1);
        _mediator5Middleware = CreateMediatorWithMiddleware(5);
        _mediator10Middleware = CreateMediatorWithMiddleware(10);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        foreach (IServiceScope scope in _scopes) scope.Dispose();
        foreach (ServiceProvider provider in _providers) provider.Dispose();
        _scopes.Clear();
        _providers.Clear();
    }

    // Builds a ServiceProvider, creates a long-lived scope, resolves IMediator from it,
    // and tracks both for disposal in GlobalCleanup.
    private IMediator ResolveFromScope(ServiceCollection services)
    {
        ServiceProvider provider = services.BuildServiceProvider();
        _providers.Add(provider);
        IServiceScope scope = provider.CreateScope();
        _scopes.Add(scope);
        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    private IMediator CreateMediatorWithMiddleware(int middlewareCount)
    {
        ServiceCollection services = new();
        services.AddSingleton(TextWriter.Null);
        MediatorConfiguration mediatorConfig = new();
        mediatorConfig.WithoutTelemetry();
        services.AddMediator(mediatorConfig);
        for (int i = 0; i < middlewareCount; i++)
            services.AddScoped(typeof(IRequestMiddleware<,>), typeof(NoOpMiddleware<,>));
        return ResolveFromScope(services);
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
    public ValueTask Send_0Middleware() => _mediator0Middleware.Send(_request);

    [Benchmark(Description = "Send (1 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public ValueTask Send_1Middleware() => _mediator1Middleware.Send(_request);

    [Benchmark(Description = "Send (5 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public ValueTask Send_5Middleware() => _mediator5Middleware.Send(_request);

    [Benchmark(Description = "Send (10 MW)")]
    [BenchmarkCategory("Send_Middleware")]
    public ValueTask Send_10Middleware() => _mediator10Middleware.Send(_request);

    #endregion

    #region SendStream Benchmarks with Middleware

    [Benchmark(Baseline = true, Description = "SendStream (0 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async ValueTask SendStream_0Middleware()
    {
        await foreach (string _ in _mediator0Middleware.SendStream(_streamRequest)) { }
    }

    [Benchmark(Description = "SendStream (1 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async ValueTask SendStream_1Middleware()
    {
        await foreach (string _ in _mediator1Middleware.SendStream(_streamRequest)) { }
    }

    [Benchmark(Description = "SendStream (5 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async ValueTask SendStream_5Middleware()
    {
        await foreach (string _ in _mediator5Middleware.SendStream(_streamRequest)) { }
    }

    [Benchmark(Description = "SendStream (10 MW)")]
    [BenchmarkCategory("SendStream_Middleware")]
    public async ValueTask SendStream_10Middleware()
    {
        await foreach (string _ in _mediator10Middleware.SendStream(_streamRequest)) { }
    }

    #endregion

    #region Publish Benchmarks with Middleware

    [Benchmark(Baseline = true, Description = "Publish (0 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public ValueTask Publish_0Middleware() => _mediator0Middleware.Publish(_notification);

    [Benchmark(Description = "Publish (1 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public ValueTask Publish_1Middleware() => _mediator1Middleware.Publish(_notification);

    [Benchmark(Description = "Publish (5 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public ValueTask Publish_5Middleware() => _mediator5Middleware.Publish(_notification);

    [Benchmark(Description = "Publish (10 MW)")]
    [BenchmarkCategory("Publish_Middleware")]
    public ValueTask Publish_10Middleware() => _mediator10Middleware.Publish(_notification);

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
///     Pre-allocated string items avoid per-item heap allocation.
///     No Task.Yield so stream completes synchronously — measures dispatch overhead only.
/// </summary>
public class BenchmarkStreamHandler : IStreamRequestHandler<BenchmarkStreamRequest, string>
{
    // Pre-allocate enough items to cover the largest benchmark (Count=10)
    private static readonly string[] _items =
        Enumerable.Range(0, 1000).Select(i => $"Item-{i}").ToArray();

#pragma warning disable CS1998 // No await — intentional; measures dispatch, not async overhead
    public async IAsyncEnumerable<string> Handle(
        BenchmarkStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < request.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return _items[i];
        }
    }
#pragma warning restore CS1998
}

#endregion