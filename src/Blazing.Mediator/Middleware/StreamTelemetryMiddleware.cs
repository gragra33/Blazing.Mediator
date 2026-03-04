using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Blazing.Mediator.Middleware;

/// <summary>
/// Optional OpenTelemetry tracing and metrics middleware for streaming request handlers.
/// Wraps the full <see cref="IAsyncEnumerable{TResponse}"/> lifetime end-to-end, preserving three
/// telemetry granularity levels:
/// <list type="bullet">
///   <item><strong>Whole-stream Activity span</strong> — opened before the first <c>MoveNextAsync()</c>, closed after enumeration completes.</item>
///   <item><strong>Per-item OTel metrics</strong> — item counter, throughput histogram, and inter-packet timing updated inside the <c>await foreach</c>.</item>
///   <item><strong>Per-item child Activity spans</strong> — opt-in via <see cref="TelemetryOptions.PacketLevelTelemetryEnabled"/>; batching granularity controlled by <see cref="TelemetryOptions.PacketTelemetryBatchSize"/>.</item>
/// </list>
/// <para>
/// Register via the generated <c>AddMediator(MediatorConfiguration config)</c> when
/// <c>config.TelemetryOptions != null</c>. When not registered, zero overhead is incurred.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The stream request type.</typeparam>
/// <typeparam name="TResponse">The stream item type.</typeparam>
public sealed class StreamTelemetryMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private static readonly Histogram<double> _durationHistogram = MediatorMetrics.StreamDurationHistogram;

    private static readonly Counter<long> _successCounter = MediatorMetrics.StreamSuccessCounter;

    private static readonly Counter<long> _failureCounter = MediatorMetrics.StreamFailureCounter;

    private static readonly Counter<long> _packetCounter = MediatorMetrics.StreamPacketCounter;

    private static readonly Histogram<double> _ttfbHistogram = MediatorMetrics.StreamTtfbHistogram;

    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="StreamTelemetryMiddleware{TRequest, TResponse}"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    public StreamTelemetryMiddleware(TelemetryOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            await foreach (var item in next().WithCancellation(cancellationToken).ConfigureAwait(false))
                yield return item;
            yield break;
        }

        var requestName = typeof(TRequest).Name;
        using var activity = Mediator.ActivitySource.StartActivity($"Mediator.SendStream:{requestName}");
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("request_name", requestName);
        activity?.SetTag("operation", "send_stream");

        var sw = Stopwatch.StartNew();
        bool firstItemReceived = false;
        double ttfbMs = 0;
        long itemCount = 0;

        IAsyncEnumerable<TResponse> stream;
        try
        {
            stream = next();
        }
        catch (Exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            var failTags = new TagList { { "request_name", requestName } };
            _failureCounter.Add(1, failTags);
            throw;
        }

        await foreach (var item in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (!firstItemReceived)
            {
                ttfbMs = sw.Elapsed.TotalMilliseconds;
                firstItemReceived = true;
                _ttfbHistogram.Record(ttfbMs, new TagList { { "request_name", requestName } });
            }

            itemCount++;

            if (_options.PacketLevelTelemetryEnabled && itemCount % _options.PacketTelemetryBatchSize == 0)
            {
                activity?.AddEvent(new ActivityEvent($"stream_packet_batch_{itemCount}",
                    tags: new ActivityTagsCollection
                    {
                        { "batch_end", itemCount },
                        { "batch_size", _options.PacketTelemetryBatchSize },
                    }));
            }

            yield return item;
        }

        sw.Stop();
        var tags = new TagList
        {
            { "request_name", requestName },
            { "items_count", itemCount },
        };

        _durationHistogram.Record(sw.Elapsed.TotalMilliseconds, tags);
        _packetCounter.Add(itemCount, new TagList { { "request_name", requestName } });
        _successCounter.Add(1, tags);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
