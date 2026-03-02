using Blazing.Mediator.OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazing.Mediator;

public sealed partial class Mediator
{
    /// <summary>
    /// Sends a stream request through the middleware pipeline to its corresponding handler and returns an async enumerable.
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request to send</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns>An async enumerable of response items</returns>
    /// <exception cref="InvalidOperationException">Thrown when no handler is found for the request type</exception>
    public IAsyncEnumerable<TResponse> SendStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (GetDispatcher() is { } d)
            // Only wrap with telemetry when an OTel listener is actually attached;
            // CreateInstrumentedStream allocates async state machines up-front, so skipping
            // it when no one is listening is both correct and zero-overhead.
            return IsTelemetryEnabled && ActivitySource.HasListeners()
                ? CreateInstrumentedStream(request, d.SendStreamAsync(request, cancellationToken), cancellationToken)
                : d.SendStreamAsync(request, cancellationToken);

        ArgumentNullException.ThrowIfNull(request);
        var requestTypeName = request.GetType().Name;
        // Debug logging: SendStream operation started
        _logger?.SendStreamOperationStarted(requestTypeName);
        return SendStreamReflection(request, cancellationToken);
    }

    /// <summary>
    /// Reflection-based stream send implementation (preserved for fallback and comparison).
    /// </summary>
    private IAsyncEnumerable<TResponse> SendStreamReflection<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken)
    {
        Type requestType = request.GetType();
        var requestTypeName = requestType.Name;

        _statistics?.IncrementQuery(requestTypeName);

        Type handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        // Debug logging: Stream handler resolution
        _logger?.SendStreamHandlerResolution(PipelineUtilities.FormatTypeName(handlerType), requestTypeName);

        // Create final handler delegate that executes the actual stream handler
        IAsyncEnumerable<TResponse> FinalHandler()
        {
            // Check for multiple handler registrations
            IEnumerable<object?> handlers = _serviceProvider.GetServices(handlerType);
            object[] handlerArray = handlers.Where(h => h != null).ToArray()!;

            switch (handlerArray)
            {
                case { Length: 0 }:
                    _logger?.NoHandlerFoundWarning(requestTypeName);
                    throw new InvalidOperationException(string.Format(_streamHandlerNotFoundFormat, requestTypeName));
                case { Length: > 1 }:
                var handlerNames = string.Join(", ", handlerArray.Select(h => h.GetType().Name));
                    _logger?.MultipleHandlersFoundWarning(requestTypeName, handlerNames);
                    throw new InvalidOperationException(string.Format(_multipleStreamHandlersFoundFormat, requestTypeName));
            }

            object handler = handlerArray[0];

            var handlerTypeName = handler.GetType().Name;
            _logger?.SendStreamHandlerFound(handlerTypeName, requestTypeName);

            MethodInfo method = handlerType.GetMethod(_handleMethodName) ?? throw new InvalidOperationException(string.Format(_handleMethodNotFoundFormat, handlerType.Name));

            try
            {
                IAsyncEnumerable<TResponse>? result = (IAsyncEnumerable<TResponse>?)method.Invoke(handler, [request, cancellationToken]);
                return result ?? throw new InvalidOperationException(string.Format(_handlerReturnedNullFormat, requestTypeName));
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        };

        // Execute through middleware pipeline using reflection to call the generic method
        MethodInfo? executeMethod = _pipelineBuilder
            .GetType()
            .GetMethods()
            .FirstOrDefault(m =>
                m.Name == "ExecuteStreamPipeline" &&
                m.GetParameters().Length == 4 &&
                m.GetParameters()[2].ParameterType.IsGenericType &&
                m.IsGenericMethodDefinition);

        IAsyncEnumerable<TResponse> baseStream;
        if (executeMethod == null)
        {
            // Fallback to direct execution if pipeline method not found
            baseStream = FinalHandler();
        }
        else
        {
            MethodInfo genericExecuteMethod = executeMethod.MakeGenericMethod(requestType, typeof(TResponse));
            IAsyncEnumerable<TResponse>? pipelineResult = (IAsyncEnumerable<TResponse>?)genericExecuteMethod.Invoke(_pipelineBuilder, [request, _serviceProvider, (StreamRequestHandlerDelegate<TResponse>)FinalHandler, cancellationToken]);
            baseStream = pipelineResult ?? FinalHandler();
        }

        // Wrap the stream with telemetry instrumentation
        return CreateInstrumentedStream(request, baseStream, cancellationToken);
    }

    /// <summary>
    /// Creates an instrumented stream wrapper that provides comprehensive OpenTelemetry tracking for stream operations.
    /// Includes packet-level metrics for throughput, latency, and performance monitoring.
    /// </summary>
    /// <typeparam name="TResponse">The type of response items in the stream</typeparam>
    /// <param name="request">The stream request</param>
    /// <param name="baseStream">The underlying stream to instrument</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An instrumented stream that records comprehensive telemetry data</returns>
    private async IAsyncEnumerable<TResponse> CreateInstrumentedStream<TResponse>(
        IStreamRequest<TResponse> request,
        IAsyncEnumerable<TResponse> baseStream,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Create the stream context outside try-catch to avoid yield restrictions
        var streamContext = new StreamTelemetryContext<TResponse>(request, _telemetryOptions);

        await foreach (var item in InstrumentedStreamEnumeration(baseStream, streamContext, cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Internal enumeration method that handles the telemetry instrumentation.
    /// Separated to avoid yield return in try-catch restrictions.
    /// </summary>
    private async IAsyncEnumerable<TResponse> InstrumentedStreamEnumeration<TResponse>(
        IAsyncEnumerable<TResponse> baseStream,
        StreamTelemetryContext<TResponse> context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var activity = IsTelemetryEnabled ? ActivitySource.StartActivity($"{_mediatorSendStreamActivity}{context.RequestTypeName}") : null;

        // Ensure the activity is started and marked as active for proper telemetry propagation
        if (activity != null)
        {
            activity.SetStatus(ActivityStatusCode.Ok);
            // Set initial tags immediately to ensure they're captured
            activity.SetTag("request_name", context.RequestTypeName);
            activity.SetTag("request_type", "stream");
            activity.SetTag("response_type", context.ResponseTypeName);
            activity.SetTag("mediator.operation", "SendStream");
            activity.SetTag("otel.library.name", "Blazing.Mediator");
            activity.SetTag("otel.library.version", typeof(Mediator).Assembly.GetName().Version?.ToString() ?? "1.0.0");

            // Add streaming-specific semantic convention tags
            activity.SetTag("stream.type", "response");
            activity.SetTag("stream.packet_level_telemetry", IsPacketLevelTelemetryEnabled);
            activity.SetTag("stream.batch_size", GetPacketTelemetryBatchSize);
        }

        var streamStopwatch = Stopwatch.StartNew();
        var lastItemTime = streamStopwatch.ElapsedMilliseconds;
        Exception? exception = null;

        // Initialize telemetry context
        context.Initialize(activity, _serviceProvider, _pipelineBuilder, _statistics);

        IAsyncEnumerator<TResponse>? enumerator = null;
        try
        {
            enumerator = baseStream.GetAsyncEnumerator(cancellationToken);

            // Add an initial event to mark the start of streaming
            activity?.AddEvent(new ActivityEvent("stream_started", DateTimeOffset.UtcNow, new ActivityTagsCollection
            {
                ["request_name"] = context.RequestTypeName,
                ["response_type"] = context.ResponseTypeName,
                ["activity_id"] = activity.Id ?? "unknown",
                ["stream.operation"] = "start"
            }));

            // Enumerate items with telemetry tracking
            while (true)
            {
                bool hasNext;
                var packetStopwatch = Stopwatch.StartNew();

                try
                {
                    hasNext = await enumerator.MoveNextAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exception = ex;
                    context.RecordError(activity, ex);
                    throw;
                }

                if (!hasNext) break;

                packetStopwatch.Stop();
                var currentTime = streamStopwatch.ElapsedMilliseconds;
                var item = enumerator.Current;

                // Record packet-level metrics with enhanced telemetry
                context.RecordPacket(currentTime, lastItemTime, item, packetStopwatch.Elapsed.TotalMilliseconds, cancellationToken);
                lastItemTime = currentTime;

                yield return item;
            }

            // Stream completed successfully
            context.RecordSuccess(activity);

            // Add completion event with comprehensive summary
            activity?.AddEvent(new ActivityEvent("stream_completed", DateTimeOffset.UtcNow, new ActivityTagsCollection
            {
                ["items_processed"] = context.ItemCount,
                ["duration_ms"] = streamStopwatch.ElapsedMilliseconds,
                ["activity_id"] = activity.Id ?? "unknown",
                ["stream.operation"] = "complete",
                ["stream.throughput_items_per_sec"] = context.ItemCount / Math.Max(streamStopwatch.Elapsed.TotalSeconds, 0.001),
                ["stream.average_inter_packet_time_ms"] = context.AverageInterPacketTime
            }));
        }
        finally
        {
            if (enumerator != null)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }

            streamStopwatch.Stop();
            context.RecordFinalMetrics(activity, streamStopwatch.Elapsed, exception);
        }
    }
}