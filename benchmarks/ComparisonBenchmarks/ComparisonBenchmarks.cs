using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace ComparisonBenchmarks;

// ── Shared response value object ───────────────────────────────────────────────
/// <summary>Shared response returned by all three library dispatchers.</summary>
public sealed record Response(Guid Id);

// ── martinothamar/Mediator  +  MediatR message types ──────────────────────────
//  These two libraries are combined because their handler signatures differ
//  only in return type (ValueTask vs Task), which allows explicit-interface
//  coexistence on a single handler class — exactly as done in martinothamar's
//  own ComparisonBenchmarks (#6).

/// <summary>Request for martinothamar/Mediator and MediatR.</summary>
public sealed record Request(Guid Id)
    : global::Mediator.IRequest<Response>,
      global::MediatR.IRequest<Response>;

/// <summary>Notification for martinothamar/Mediator and MediatR.</summary>
public sealed record Notification(Guid Id)
    : global::Mediator.INotification,
      global::MediatR.INotification;

/// <summary>Streaming request for martinothamar/Mediator and MediatR.</summary>
public sealed record StreamRequest(Guid Id)
    : global::Mediator.IStreamRequest<Response>,
      global::MediatR.IStreamRequest<Response>;

// ── Blazing.Mediator message types ─────────────────────────────────────────────
//  Dedicated types for Blazing.Mediator so that its handler class can use
//  regular (non-explicit) public Handle() methods.  This also avoids the
//  source-generator TypeCatalog seeing martinothamar's interfaces mixed in,
//  which would produce unresolvable type names in the generated code.

/// <summary>Request for Blazing.Mediator.</summary>
public sealed record BlazeRequest(Guid Id)
    : global::Blazing.Mediator.IRequest<Response>;

/// <summary>Notification for Blazing.Mediator.</summary>
public sealed record BlazeNotification(Guid Id)
    : global::Blazing.Mediator.INotification;

/// <summary>Streaming request for Blazing.Mediator.</summary>
public sealed record BlazeStreamRequest(Guid Id)
    : global::Blazing.Mediator.IStreamRequest<Response>;

// ── Handler for martinothamar/Mediator + MediatR ───────────────────────────────
/// <summary>
/// Implements both martinothamar/Mediator and MediatR handler interfaces using
/// the same explicit-implementation pattern used in martinothamar's own
/// ComparisonBenchmarks (#6 benchmark).
/// </summary>
public sealed class Handler
    : global::Mediator.IRequestHandler<Request, Response>,
      global::MediatR.IRequestHandler<Request, Response>,
      global::Mediator.IStreamRequestHandler<StreamRequest, Response>,
      global::MediatR.IStreamRequestHandler<StreamRequest, Response>,
      global::Mediator.INotificationHandler<Notification>,
      global::MediatR.INotificationHandler<Notification>
{
    private static readonly Response _response = new(Guid.NewGuid());
    private static readonly Task<Response> _taskResponse = Task.FromResult(_response);

    // martinothamar/Mediator returns ValueTask<Response>
    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new(_response);

    // MediatR returns Task<Response> — explicit to avoid signature collision
    Task<Response> global::MediatR.IRequestHandler<Request, Response>.Handle(
        Request request, CancellationToken cancellationToken) =>
        _taskResponse;

    IAsyncEnumerable<Response> global::Mediator.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    IAsyncEnumerable<Response> global::MediatR.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    // martinothamar/Mediator returns ValueTask
    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) =>
        default;

    // MediatR returns Task — explicit to avoid signature collision
    Task global::MediatR.INotificationHandler<Notification>.Handle(
        Notification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;

#pragma warning disable CS1998 // No await in async IAsyncEnumerable — intentional
    private static async IAsyncEnumerable<Response> Enumerate(
        [EnumeratorCancellation] CancellationToken _ = default)
    {
        yield return _response;
        yield return _response;
        yield return _response;
    }
#pragma warning restore CS1998
}

// ── Handler for Blazing.Mediator ───────────────────────────────────────────────
/// <summary>
/// Separate handler class for Blazing.Mediator so all Handle() methods can be
/// public (non-explicit), which is required for the Blazing.Mediator source
/// generator's dispatch emitter to call <c>handler.Handle(...)</c> directly.
/// </summary>
public sealed class BlazeHandler
    : global::Blazing.Mediator.IRequestHandler<BlazeRequest, Response>,
      global::Blazing.Mediator.IStreamRequestHandler<BlazeStreamRequest, Response>,
      global::Blazing.Mediator.INotificationHandler<BlazeNotification>
{
    private static readonly Response _response = new(Guid.NewGuid());

    public ValueTask<Response> Handle(BlazeRequest request, CancellationToken cancellationToken) =>
        new(_response);

    public IAsyncEnumerable<Response> Handle(
        BlazeStreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    public ValueTask Handle(BlazeNotification notification, CancellationToken cancellationToken) =>
        default;

#pragma warning disable CS1998
    private static async IAsyncEnumerable<Response> Enumerate(
        [EnumeratorCancellation] CancellationToken _ = default)
    {
        yield return _response;
        yield return _response;
        yield return _response;
    }
#pragma warning restore CS1998
}

// ── Benchmark ──────────────────────────────────────────────────────────────────

/// <summary>
/// Side-by-side performance comparison of three .NET mediator libraries for the
/// most common operations: <c>Send</c> (request/query), <c>Publish</c>
/// (notification), and <c>CreateStream</c> / <c>SendStream</c> (streaming).
///
/// <b>Library registrations:</b>
/// <list type="bullet">
///   <item>martinothamar/Mediator – Singleton, source-generated dispatch via
///         <c>Mediator.Mediator</c> and <c>Mediator.IMediator</c>.</item>
///   <item>MediatR – Singleton, reflection-based dispatch.</item>
///   <item>Blazing.Mediator – Scoped (pre-resolved from a long-lived scope),
///         <b>source-generated dispatch</b> via <see cref="global::Blazing.Mediator.MediatorDispatcherBase"/>.
///         <c>ContainerMetadata</c> is Singleton — handlers and middleware chains
///         are pre-resolved from the root <see cref="IServiceProvider"/> at startup.
///         All optional features (telemetry, statistics, logging) are disabled
///         so dispatch overhead is measured in isolation.</item>
/// </list>
///
/// This run captures the <b>source-generator post-optimisation</b> benchmark
/// after the overhaul documented in <c>sourcegen-update-plan.md</c>.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ComparisonBenchmarks
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private IServiceProvider _sp = null!;
    private IServiceScope _blazingScope = null!;

    // martinothamar/Mediator – Singleton; resolved from root provider
    private global::Mediator.Mediator _mediatorConcrete = null!;
    private global::Mediator.IMediator _mediatorInterface = null!;

    // MediatR – Singleton
    private global::MediatR.IMediator _mediatR = null!;

    // Blazing.Mediator – Scoped; pre-resolved from a long-lived scope
    private global::Blazing.Mediator.IMediator _blazingMediator = null!;

    // Messages for martinothamar/Mediator + MediatR (shared types)
    private Request _request = null!;
    private StreamRequest _streamRequest = null!;
    private Notification _notification = null!;

    // Messages for Blazing.Mediator (dedicated types)
    private BlazeRequest _blazeRequest = null!;
    private BlazeStreamRequest _blazeStreamRequest = null!;
    private BlazeNotification _blazeNotification = null!;

    // ── GlobalSetup ───────────────────────────────────────────────────────────

    [GlobalSetup]
    public void Setup()
    {
        _request = new Request(Guid.NewGuid());
        _streamRequest = new StreamRequest(Guid.NewGuid());
        _notification = new Notification(Guid.NewGuid());

        _blazeRequest = new BlazeRequest(Guid.NewGuid());
        _blazeStreamRequest = new BlazeStreamRequest(Guid.NewGuid());
        _blazeNotification = new BlazeNotification(Guid.NewGuid());

        var services = new ServiceCollection();

        // ── martinothamar/Mediator ──────────────────────────────────────────
        // The source generator emits AddMediator() into the
        // Microsoft.Extensions.DependencyInjection namespace; calling it via
        // 'services.AddMediator()' is unambiguous because the Blazing.Mediator
        // extension method lives in the Blazing.Mediator namespace (called below
        // via its fully-qualified type name to avoid compile-time ambiguity).
        services.AddMediator(); // → MediatorDependencyInjectionExtensions.AddMediator (Singleton)

        // ── MediatR ────────────────────────────────────────────────────────
        services.AddMediatR(opts =>
        {
            opts.Lifetime = ServiceLifetime.Singleton;
            opts.RegisterServicesFromAssembly(typeof(Handler).Assembly);
        });

        // ── Blazing.Mediator ───────────────────────────────────────────────
        // All components are registered manually rather than calling the generated
        // AddMediator() extension method. martinothamar/Mediator's IncrementalGenerator
        // scans ALL method bodies for "AddMediator" invocations and raises MSG0007
        // ("could not parse MediatorOptions-based configuration") on any unrecognised
        // call shape, which would prevent it from generating its own registration code.
        //
        // Handler registrations — mirrors exactly what the generated AddMediator() emits.
        services.AddTransient<global::ComparisonBenchmarks.BlazeHandler>();
        services.AddTransient<
            global::Blazing.Mediator.IRequestHandler<global::ComparisonBenchmarks.BlazeRequest, global::ComparisonBenchmarks.Response>,
            global::ComparisonBenchmarks.BlazeHandler>();
        services.AddTransient<
            global::Blazing.Mediator.INotificationHandler<global::ComparisonBenchmarks.BlazeNotification>,
            global::ComparisonBenchmarks.BlazeHandler>();
        // Container probe — registered twice so GetServices<ContainerProbe0>() returns ContainerProbe0[]
        // (Microsoft.Extensions.DependencyInjection returns a typed array when ≥2 registrations exist).
        // ServicesUnderlyingTypeIsArray enables zero-copy Unsafe.As<T[]> casts in notification dispatch.
        services.AddTransient<global::Blazing.Mediator.Generated.ContainerProbe0>();
        services.AddTransient<global::Blazing.Mediator.Generated.ContainerProbe0>();
        // Type catalog — Singleton; all data is compile-time constant, zero allocation on first access.
        services.AddSingleton<global::Blazing.Mediator.Statistics.IMediatorTypeCatalog>(
            global::Blazing.Mediator.Generated.MediatorTypeCatalog.Instance);
        // INotificationPublisher — required by NotificationHandlerWrapper_*.Init() to pre-resolve
        // the publisher chain once at scope construction time.
        services.AddSingleton<global::Blazing.Mediator.INotificationPublisher,
            global::Blazing.Mediator.Notifications.SequentialNotificationPublisher>();
        // ContainerMetadata — Scoped so each request scope initialises its own wrapper instances.
        // The constructor calls Init(sp) on every wrapper, pre-resolving handlers and middleware
        // from the scope's IServiceProvider exactly once. Subsequent dispatches within the same
        // scope pay only a single field read + pre-baked delegate invoke — zero DI, zero alloc.
        services.AddScoped<global::Blazing.Mediator.Generated.ContainerMetadata>();
        services.AddScoped<global::Blazing.Mediator.MediatorDispatcherBase>(
            static sp => sp.GetRequiredService<global::Blazing.Mediator.Generated.ContainerMetadata>());
        // IMediator — Scoped; resolved from a long-lived scope so the benchmark measures pure
        // dispatch overhead with zero per-call DI work.
        services.AddScoped<global::Blazing.Mediator.IMediator, global::Blazing.Mediator.Mediator>();

        _sp = services.BuildServiceProvider();

        // martinothamar/Mediator: both concrete class and interface (Singleton)
        _mediatorConcrete = _sp.GetRequiredService<global::Mediator.Mediator>();
        _mediatorInterface = _sp.GetRequiredService<global::Mediator.IMediator>();

        // MediatR (Singleton)
        _mediatR = _sp.GetRequiredService<global::MediatR.IMediator>();

        // Blazing.Mediator: create one long-lived scope and resolve IMediator from it.
        // IMediator is registered Scoped (above). ContainerMetadata is also Scoped —
        // its constructor pre-resolves all handlers once for this scope's lifetime.
        _blazingScope = _sp.CreateScope();
        _blazingMediator = _blazingScope.ServiceProvider
            .GetRequiredService<global::Blazing.Mediator.IMediator>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _blazingScope.Dispose();
        (_sp as IDisposable)?.Dispose();
    }

    // ── Request / Send ────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Request")]
    public Task<Response> Request_MediatR() =>
        _mediatR.Send(_request, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> Request_Mediator_Concrete() =>
        _mediatorConcrete.Send(_request, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> Request_Mediator_Interface() =>
        _mediatorInterface.Send(_request, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Request")]
    public ValueTask<Response> Request_BlazingMediator() =>
        _blazingMediator.Send(_blazeRequest, CancellationToken.None);

    // ── Notification / Publish ────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Notification")]
    public Task Notification_MediatR() =>
        _mediatR.Publish(_notification, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask Notification_Mediator_Concrete() =>
        _mediatorConcrete.Publish(_notification, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask Notification_Mediator_Interface() =>
        _mediatorInterface.Publish(_notification, CancellationToken.None);

    [Benchmark]
    [BenchmarkCategory("Notification")]
    public ValueTask Notification_BlazingMediator() =>
        _blazingMediator.Publish(_blazeNotification, CancellationToken.None);

    // ── Streaming ─────────────────────────────────────────────────────────────

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Streaming")]
    public async ValueTask Stream_MediatR()
    {
        await foreach (var _ in _mediatR.CreateStream(_streamRequest, CancellationToken.None)) { }
    }

    [Benchmark]
    [BenchmarkCategory("Streaming")]
    public async ValueTask Stream_Mediator_Concrete()
    {
        await foreach (var _ in _mediatorConcrete.CreateStream(_streamRequest, CancellationToken.None)) { }
    }

    [Benchmark]
    [BenchmarkCategory("Streaming")]
    public async ValueTask Stream_Mediator_Interface()
    {
        await foreach (var _ in _mediatorInterface.CreateStream(_streamRequest, CancellationToken.None)) { }
    }

    [Benchmark]
    [BenchmarkCategory("Streaming")]
    public async ValueTask Stream_BlazingMediator()
    {
        await foreach (var _ in _blazingMediator.SendStream(_blazeStreamRequest, CancellationToken.None)) { }
    }
}
