using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

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
public sealed class TelemetryMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly Histogram<double> _durationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.send.duration", unit: "ms",
            description: "Duration of mediator send operations");

    private static readonly Counter<long> _successCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.success",
            description: "Number of successful mediator send operations");

    private static readonly Counter<long> _failureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.failure",
            description: "Number of failed mediator send operations");

    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="TelemetryMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    public TelemetryMiddleware(TelemetryOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

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
public sealed class TelemetryMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private static readonly Histogram<double> _durationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.send.duration", unit: "ms",
            description: "Duration of mediator send operations");

    private static readonly Counter<long> _successCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.success",
            description: "Number of successful mediator send operations");

    private static readonly Counter<long> _failureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.send.failure",
            description: "Number of failed mediator send operations");

    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="TelemetryMiddleware{TRequest}"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    public TelemetryMiddleware(TelemetryOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

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
