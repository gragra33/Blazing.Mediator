using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Tracing middleware for streaming requests with enhanced OpenTelemetry integration.
/// </summary>
public class StreamingTracingMiddleware<TRequest, TResponse>(
    ILogger<StreamingTracingMiddleware<TRequest, TResponse>> logger)
    : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private static readonly ActivitySource ActivitySource = new("OpenTelemetryExample.StreamingMiddleware");

    public int Order => -100; // Execute early in the pipeline

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var responseType = typeof(TResponse).Name;

        using var activity = ActivitySource.StartActivity($"StreamingMiddleware.{requestType}");
        activity?.SetTag("middleware.type", "StreamingTracing");
        activity?.SetTag("request.type", requestType);
        activity?.SetTag("response.type", responseType);
        activity?.SetTag("middleware.order", Order);

        var startTime = DateTime.UtcNow;
        var streamId = Guid.NewGuid().ToString("N")[..8];

        activity?.SetTag("stream.id", streamId);
        activity?.SetTag("stream.start_time", startTime.ToString("O"));

        logger.LogInformation(
            "🔄 [STREAMING TRACE] Starting stream {RequestType} -> {ResponseType} [ID: {StreamId}]",
            requestType, responseType, streamId);

        // Get the stream first, then enumerate it
        IAsyncEnumerable<TResponse> stream;
        try
        {
            stream = next();
        }
        catch (Exception ex)
        {
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            logger.LogError(ex,
                "❌ [STREAMING TRACE] Failed to start stream {RequestType} -> {ResponseType} [ID: {StreamId}] Error: {ErrorMessage}",
                requestType, responseType, streamId, ex.Message);

            throw;
        }

        // Now enumerate the stream without try-catch around yield
        await foreach (var item in EnumerateWithTelemetry(stream, activity, streamId, requestType, responseType, startTime, cancellationToken))
        {
            yield return item;
        }
    }

    private async IAsyncEnumerable<TResponse> EnumerateWithTelemetry(
        IAsyncEnumerable<TResponse> stream,
        Activity? activity,
        string streamId,
        string requestType,
        string responseType,
        DateTime startTime,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var itemCount = 0;
        var items = new List<TResponse>();

        try
        {
            await foreach (var item in stream.WithCancellation(cancellationToken))
            {
                itemCount++;

                // Add activity event for each item
                activity?.AddEvent(new ActivityEvent("stream.item.processed", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["item.timestamp"] = DateTime.UtcNow.ToString("O"),
                    ["stream.id"] = streamId
                }));

                // Log every 10th item to avoid spam
                if (itemCount % 10 == 0 || itemCount <= 5)
                {
                    logger.LogDebug(
                        "📦 [STREAMING TRACE] Item {ItemCount} processed for stream {StreamId} ({RequestType})",
                        itemCount, streamId, requestType);
                }

                items.Add(item);
            }

            // Success path
            var duration = DateTime.UtcNow - startTime;
            activity?.SetTag("stream.items_count", itemCount);
            activity?.SetTag("stream.duration_ms", duration.TotalMilliseconds);
            activity?.SetTag("stream.throughput_items_per_sec", itemCount / Math.Max(duration.TotalSeconds, 0.001));
            activity?.SetStatus(ActivityStatusCode.Ok, $"Stream completed successfully with {itemCount} items");

            logger.LogInformation(
                "[STREAMING TRACE] Completed stream {RequestType} -> {ResponseType} [ID: {StreamId}] " +
                "Items: {ItemCount}, Duration: {Duration}ms, Throughput: {Throughput:F2} items/sec",
                requestType, responseType, streamId, itemCount, duration.TotalMilliseconds,
                itemCount / Math.Max(duration.TotalSeconds, 0.001));
        }
        catch (Exception ex)
        {
            var streamException = ex;
            var duration = DateTime.UtcNow - startTime;

            activity?.SetTag("stream.items_count", itemCount);
            activity?.SetTag("stream.duration_ms", duration.TotalMilliseconds);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            logger.LogError(ex,
                "❌ [STREAMING TRACE] Failed stream {RequestType} -> {ResponseType} [ID: {StreamId}] " +
                "Items processed: {ItemCount}, Duration: {Duration}ms, Error: {ErrorMessage}",
                requestType, responseType, streamId, itemCount, duration.TotalMilliseconds, ex.Message);

            throw;
        }

        // Yield the collected items outside of the try-catch
        foreach (var item in items)
        {
            yield return item;
        }
    }
}