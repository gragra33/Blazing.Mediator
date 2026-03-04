using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace ComparisonBenchmarksOld;

// ── Shared response value object ───────────────────────────────────────────────
/// <summary>Shared response returned by all three library dispatchers.</summary>
public sealed record Response(Guid Id);

// ── martinothamar/Mediator  +  MediatR message types ──────────────────────────
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

// ── Old Blazing.Mediator (master v2.0.1) message types ────────────────────────
//  Dedicated types so the source-generator for martinothamar/Mediator does not
//  see mixed interfaces, which would produce unresolvable type names in its
//  generated registration code.

/// <summary>Request for old Blazing.Mediator (reflection-based dispatch).</summary>
public sealed record BlazeRequest(Guid Id)
    : global::Blazing.Mediator.IRequest<Response>;

/// <summary>Notification for old Blazing.Mediator.</summary>
public sealed record BlazeNotification(Guid Id)
    : global::Blazing.Mediator.INotification;

/// <summary>Streaming request for old Blazing.Mediator.</summary>
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

    public ValueTask<Response> Handle(Request request, CancellationToken cancellationToken) =>
        new(_response);

    Task<Response> global::MediatR.IRequestHandler<Request, Response>.Handle(
        Request request, CancellationToken cancellationToken) =>
        _taskResponse;

    IAsyncEnumerable<Response> global::Mediator.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    IAsyncEnumerable<Response> global::MediatR.IStreamRequestHandler<StreamRequest, Response>.Handle(
        StreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    public ValueTask Handle(Notification notification, CancellationToken cancellationToken) =>
        default;

    Task global::MediatR.INotificationHandler<Notification>.Handle(
        Notification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;

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

// ── Handler for old Blazing.Mediator ──────────────────────────────────────────
/// <summary>
/// Implements old Blazing.Mediator v2.0.1 handler interfaces.
/// <para>
/// The old library uses <c>Task</c> return types (not <c>ValueTask</c>), as
/// <c>ValueTask</c> was introduced in the source-generator overhaul.
/// Handlers are resolved per-call via <see cref="IServiceProvider.GetServices"/> on
/// the open-generic <c>IRequestHandler&lt;,&gt;</c> type + reflection
/// <c>MethodInfo.Invoke</c> — the same path measured in the pre-optimisation baseline.
/// </para>
/// </summary>
public sealed class BlazeHandler
    : global::Blazing.Mediator.IRequestHandler<BlazeRequest, Response>,
      global::Blazing.Mediator.IStreamRequestHandler<BlazeStreamRequest, Response>,
      global::Blazing.Mediator.INotificationHandler<BlazeNotification>
{
    private static readonly Response _response = new(Guid.NewGuid());
    private static readonly Task<Response> _taskResponse = Task.FromResult(_response);

    // Old Blazing.Mediator IRequestHandler returns Task<T> (not ValueTask<T>)
    public Task<Response> Handle(BlazeRequest request, CancellationToken cancellationToken) =>
        _taskResponse;

    public IAsyncEnumerable<Response> Handle(
        BlazeStreamRequest request, CancellationToken cancellationToken) =>
        Enumerate();

    // Old INotificationHandler returns Task (not ValueTask)
    public Task Handle(BlazeNotification notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;

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
/// Side-by-side comparison of the <b>pre-optimisation</b> Blazing.Mediator v2.0.1
/// (master branch, reflection-based dispatch) against martinothamar/Mediator and MediatR.
///
/// <para>
/// This benchmark isolates the old library's dispatch overhead and serves as the permanent
/// historical baseline for measuring the gains achieved by the source-generator overhaul.
/// Compare results from this project against <c>ComparisonBenchmarks</c> (optimised version).
/// </para>
///
/// <b>Library registrations:</b>
/// <list type="bullet">
///   <item>martinothamar/Mediator – Singleton, source-generated dispatch.</item>
///   <item>MediatR – Singleton, reflection-based dispatch.</item>
///   <item>Old Blazing.Mediator (v2.0.1) – Scoped (pre-resolved from a long-lived scope),
///         <b>reflection-based dispatch</b>. <c>IRequestHandler</c> is resolved via
///         <see cref="IServiceProvider.GetServices"/> on the open-generic handler type, then
///         dispatched via <see cref="System.Reflection.MethodInfo.Invoke"/> on every call.
///         All optional features (telemetry, statistics, logging) are disabled so only raw
///         dispatch overhead is measured.</item>
/// </list>
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[RankColumn]
public class ComparisonBenchmarksOld
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private IServiceProvider _sp = null!;
    private IServiceScope _blazingScope = null!;

    // martinothamar/Mediator – Singleton; resolved from root provider
    private global::Mediator.Mediator _mediatorConcrete = null!;
    private global::Mediator.IMediator _mediatorInterface = null!;

    // MediatR – Singleton
    private global::MediatR.IMediator _mediatR = null!;

    // Old Blazing.Mediator – Scoped; pre-resolved from a long-lived scope
    private global::Blazing.Mediator.IMediator _blazingMediator = null!;

    // Messages for martinothamar/Mediator + MediatR (shared types)
    private Request _request = null!;
    private StreamRequest _streamRequest = null!;
    private Notification _notification = null!;

    // Messages for old Blazing.Mediator (dedicated types)
    private BlazeRequest _blazeRequest = null!;
    private BlazeStreamRequest _blazeStreamRequest = null!;
    private BlazeNotification _blazeNotification = null!;

    // ── GlobalSetup ───────────────────────────────────────────────────────────

    [GlobalSetup]
    public void Setup()
    {
        _request       = new Request(Guid.NewGuid());
        _streamRequest = new StreamRequest(Guid.NewGuid());
        _notification  = new Notification(Guid.NewGuid());

        _blazeRequest       = new BlazeRequest(Guid.NewGuid());
        _blazeStreamRequest = new BlazeStreamRequest(Guid.NewGuid());
        _blazeNotification  = new BlazeNotification(Guid.NewGuid());

        var services = new ServiceCollection();

        // ── martinothamar/Mediator ──────────────────────────────────────────
        // The source generator emits AddMediator() into the
        // Microsoft.Extensions.DependencyInjection namespace.
        services.AddMediator();

        // ── MediatR ────────────────────────────────────────────────────────
        services.AddMediatR(opts =>
        {
            opts.Lifetime = ServiceLifetime.Singleton;
            opts.RegisterServicesFromAssembly(typeof(Handler).Assembly);
        });

        // ── Old Blazing.Mediator (master v2.0.1) ────────────────────────────
        // All components are registered manually — calling AddMediator(…) from the
        // Blazing.Mediator namespace would trigger martinothamar/Mediator's
        // IncrementalGenerator MSG0007 warning (it scans ALL method bodies for
        // "AddMediator" invocations) and would prevent it from generating its own
        // registration code.
        //
        // MediatorConfiguration singleton — holds the (empty) pipeline builders.
        var blazingConfig = new global::Blazing.Mediator.Configuration.MediatorConfiguration();
        services.AddSingleton(blazingConfig);

        // Pipeline builders — Scoped, returned from the config singleton.
        services.AddScoped<global::Blazing.Mediator.Pipeline.IMiddlewarePipelineBuilder>(
            static sp => sp.GetRequiredService<global::Blazing.Mediator.Configuration.MediatorConfiguration>().PipelineBuilder);
        services.AddScoped<global::Blazing.Mediator.Pipeline.INotificationPipelineBuilder>(
            static sp => sp.GetRequiredService<global::Blazing.Mediator.Configuration.MediatorConfiguration>().NotificationPipelineBuilder);

        // IMediator — Scoped; pre-resolved from a long-lived scope.
        // TelemetryOptions.Disabled() ensures IsTelemetryEnabled = false inside the old
        // Mediator, matching the "all optional features disabled" configuration used in
        // the pre-optimisation baseline documented in sourcegen-update-plan.md.
        // statistics: null and logger: null disable all other optional features.
        services.AddScoped<global::Blazing.Mediator.IMediator>(
            static sp => new global::Blazing.Mediator.Mediator(
                sp,
                sp.GetRequiredService<global::Blazing.Mediator.Pipeline.IMiddlewarePipelineBuilder>(),
                sp.GetRequiredService<global::Blazing.Mediator.Pipeline.INotificationPipelineBuilder>(),
                statistics: null,
                telemetryOptions: global::Blazing.Mediator.Configuration.TelemetryOptions.Disabled(),
                logger: null));

        // Handler registrations — mirrors exactly what AddMediator(typeof(BlazeHandler)) would emit.
        services.AddTransient<global::ComparisonBenchmarksOld.BlazeHandler>();
        services.AddTransient<
            global::Blazing.Mediator.IRequestHandler<global::ComparisonBenchmarksOld.BlazeRequest, global::ComparisonBenchmarksOld.Response>,
            global::ComparisonBenchmarksOld.BlazeHandler>();
        services.AddTransient<
            global::Blazing.Mediator.INotificationHandler<global::ComparisonBenchmarksOld.BlazeNotification>,
            global::ComparisonBenchmarksOld.BlazeHandler>();
        services.AddTransient<
            global::Blazing.Mediator.IStreamRequestHandler<global::ComparisonBenchmarksOld.BlazeStreamRequest, global::ComparisonBenchmarksOld.Response>,
            global::ComparisonBenchmarksOld.BlazeHandler>();

        _sp = services.BuildServiceProvider();

        // martinothamar/Mediator (Singleton)
        _mediatorConcrete  = _sp.GetRequiredService<global::Mediator.Mediator>();
        _mediatorInterface = _sp.GetRequiredService<global::Mediator.IMediator>();

        // MediatR (Singleton)
        _mediatR = _sp.GetRequiredService<global::MediatR.IMediator>();

        // Old Blazing.Mediator: create one long-lived scope and resolve IMediator from it.
        // IMediator and pipeline builders are Scoped — the scope owns their lifetime.
        _blazingScope   = _sp.CreateScope();
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
    public Task<Response> Request_OldBlazingMediator() =>
        _blazingMediator.Send<Response>(_blazeRequest, CancellationToken.None);

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
    public Task Notification_OldBlazingMediator() =>
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
    public async ValueTask Stream_OldBlazingMediator()
    {
        await foreach (var _ in _blazingMediator.SendStream<Response>(_blazeStreamRequest, CancellationToken.None)) { }
    }
}
