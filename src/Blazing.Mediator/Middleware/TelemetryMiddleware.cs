using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Blazing.Mediator.Pipeline;

namespace Blazing.Mediator.Middleware;

/// <summary>
/// Optional OpenTelemetry tracing and metrics middleware for request-with-response handlers.
/// Opened as a <c>Mediator.Send:{RequestName}</c> Activity and records duration, success, and failure
/// counters on <see cref="Mediator.Meter"/>.
/// <para>
/// Register via the generated <c>AddMediator(MediatorConfiguration config)</c> when
/// <c>config.TelemetryOptions != null</c>. When not registered, zero overhead is incurred.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The request type (query or command with response).</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
[Order(int.MinValue)]
public sealed class TelemetryMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Histogram<double> _durationHistogram =
        MediatorMetrics.SendDurationHistogram;

    private static readonly Counter<long> _successCounter =
        MediatorMetrics.SendSuccessCounter;

    private static readonly Counter<long> _failureCounter =
        MediatorMetrics.SendFailureCounter;

    private readonly TelemetryOptions _options;
    private readonly MiddlewarePipelineBuilder _pipelineBuilder;

    /// <summary>
    /// Initialises a new <see cref="TelemetryMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    /// <param name="pipelineBuilder">The pipeline builder used to resolve applicable middleware lists.</param>
    public TelemetryMiddleware(TelemetryOptions options, MiddlewarePipelineBuilder pipelineBuilder)
    {
        _options         = options         ?? throw new ArgumentNullException(nameof(options));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            return await next().ConfigureAwait(false);

        var requestName = typeof(TRequest).Name;
        var isQuery = request is IQuery<TResponse>;
        var requestType = isQuery ? "query" : "command";

        using var activity = Mediator.ActivitySource.StartActivity($"Mediator.Send:{requestName}");
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("request_name", requestName);
        activity?.SetTag("request_type", requestType);

        if (activity != null && _options.MiddlewareCaptureMode == Configuration.MiddlewareCaptureMode.Applicable)
        {
            var applicable = _pipelineBuilder.GetApplicableMiddleware<TRequest, TResponse>();
            activity.SetTag("request_middleware.pipeline",
                string.Join(",", applicable.Select(m => Mediator.SanitizeMiddlewareName(m.Type, _options.SensitiveDataPatterns))));
            activity.SetTag("request_middleware.count", applicable.Count);
            activity.SetTag("request_middleware.orders",
                string.Join(",", applicable.Select(m => m.Order)));
            activity.SetTag("request_middleware.capture_mode", "applicable");
        }

        // ── Executed mode: allocate tracking context before pipeline ──────────────────
        MiddlewareExecutionContext? execCtx = null;
        if (_options.MiddlewareCaptureMode == Configuration.MiddlewareCaptureMode.Executed)
            execCtx = MiddlewareExecutionContext.SetCurrent(new MiddlewareExecutionContext());

        var sw = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            return await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            // ── Executed mode: emit summary from collected context ─────────────────────
            if (activity != null && execCtx != null)
            {
                activity.SetTag("request_middleware.executed_pipeline",
                    string.Join(",", execCtx.Executed.Select(t => Mediator.SanitizeMiddlewareName(t, _options.SensitiveDataPatterns))));
                activity.SetTag("request_middleware.executed_count", execCtx.Executed.Count);
                activity.SetTag("request_middleware.skipped_pipeline",
                    string.Join(",", execCtx.Skipped.Select(t => Mediator.SanitizeMiddlewareName(t, _options.SensitiveDataPatterns))));
                activity.SetTag("request_middleware.skipped_count", execCtx.Skipped.Count);
                activity.SetTag("request_middleware.capture_mode", "executed");
                MiddlewareExecutionContext.ClearCurrent();
            }

            var tags = new TagList
            {
                { "request_name", requestName },
                { "request_type", requestType },
            };

            _durationHistogram.Record(sw.Elapsed.TotalMilliseconds, tags);

            if (exception == null)
            {
                _successCounter.Add(1, tags);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                _failureCounter.Add(1, tags);
            }
        }
    }
}

/// <summary>
/// Optional OpenTelemetry tracing and metrics middleware for void-command handlers.
/// Records the same activity and metrics as <see cref="TelemetryMiddleware{TRequest, TResponse}"/>
/// but for commands that return no value.
/// </summary>
/// <typeparam name="TRequest">The void command type.</typeparam>
[Order(int.MinValue)]
public sealed class TelemetryMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private static readonly Histogram<double> _durationHistogram =
        MediatorMetrics.SendDurationHistogram;

    private static readonly Counter<long> _successCounter =
        MediatorMetrics.SendSuccessCounter;

    private static readonly Counter<long> _failureCounter =
        MediatorMetrics.SendFailureCounter;

    private readonly TelemetryOptions _options;
    private readonly MiddlewarePipelineBuilder _pipelineBuilder;

    /// <summary>
    /// Initialises a new <see cref="TelemetryMiddleware{TRequest}"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    /// <param name="pipelineBuilder">The pipeline builder used to resolve applicable middleware lists.</param>
    public TelemetryMiddleware(TelemetryOptions options, MiddlewarePipelineBuilder pipelineBuilder)
    {
        _options         = options         ?? throw new ArgumentNullException(nameof(options));
        _pipelineBuilder = pipelineBuilder ?? throw new ArgumentNullException(nameof(pipelineBuilder));
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask HandleAsync(
        TRequest request,
        RequestHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            await next().ConfigureAwait(false);
            return;
        }

        var requestName = typeof(TRequest).Name;

        using var activity = Mediator.ActivitySource.StartActivity($"Mediator.Send:{requestName}");
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("request_name", requestName);
        activity?.SetTag("request_type", "command");

        if (activity != null && _options.MiddlewareCaptureMode == Configuration.MiddlewareCaptureMode.Applicable)
        {
            var applicable = _pipelineBuilder.GetApplicableMiddleware<TRequest>();
            activity.SetTag("request_middleware.pipeline",
                string.Join(",", applicable.Select(m => Mediator.SanitizeMiddlewareName(m.Type, _options.SensitiveDataPatterns))));
            activity.SetTag("request_middleware.count", applicable.Count);
            activity.SetTag("request_middleware.orders",
                string.Join(",", applicable.Select(m => m.Order)));
            activity.SetTag("request_middleware.capture_mode", "applicable");
        }

        // ── Executed mode: allocate tracking context before pipeline ──────────────────
        MiddlewareExecutionContext? execCtx = null;
        if (_options.MiddlewareCaptureMode == Configuration.MiddlewareCaptureMode.Executed)
            execCtx = MiddlewareExecutionContext.SetCurrent(new MiddlewareExecutionContext());

        var sw = Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await next().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            // ── Executed mode: emit summary from collected context ─────────────────────
            if (activity != null && execCtx != null)
            {
                activity.SetTag("request_middleware.executed_pipeline",
                    string.Join(",", execCtx.Executed.Select(t => Mediator.SanitizeMiddlewareName(t, _options.SensitiveDataPatterns))));
                activity.SetTag("request_middleware.executed_count", execCtx.Executed.Count);
                activity.SetTag("request_middleware.skipped_pipeline",
                    string.Join(",", execCtx.Skipped.Select(t => Mediator.SanitizeMiddlewareName(t, _options.SensitiveDataPatterns))));
                activity.SetTag("request_middleware.skipped_count", execCtx.Skipped.Count);
                activity.SetTag("request_middleware.capture_mode", "executed");
                MiddlewareExecutionContext.ClearCurrent();
            }

            var tags = new TagList
            {
                { "request_name", requestName },
                { "request_type", "command" },
            };

            _durationHistogram.Record(sw.Elapsed.TotalMilliseconds, tags);

            if (exception == null)
            {
                _successCounter.Add(1, tags);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                _failureCounter.Add(1, tags);
            }
        }
    }
}
