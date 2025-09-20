using System.Diagnostics;
using System.Runtime.CompilerServices;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Performance monitoring middleware for streaming requests.
/// </summary>
public class StreamingPerformanceMiddleware<TRequest, TResponse>(
    ILogger<StreamingPerformanceMiddleware<TRequest, TResponse>> logger)
    : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private static readonly ActivitySource ActivitySource = new("OpenTelemetryExample.StreamingPerformance");

    public int Order => -50; // Execute after tracing but before business logic

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"StreamingPerformance.{requestType}");
        
        var startTime = DateTime.UtcNow;
        var itemCount = 0;
        var lastItemTime = startTime;
        var interItemTimes = new List<double>();
        
        activity?.SetTag("middleware.type", "StreamingPerformance");
        activity?.SetTag("request.type", requestType);

        await foreach (var item in next().WithCancellation(cancellationToken))
        {
            var currentTime = DateTime.UtcNow;
            itemCount++;
            
            if (itemCount > 1)
            {
                var interItemDelay = (currentTime - lastItemTime).TotalMilliseconds;
                interItemTimes.Add(interItemDelay);
                
                // Track inter-item timing in activity
                activity?.AddEvent(new ActivityEvent($"performance.item.{itemCount}", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["inter_item_delay_ms"] = interItemDelay,
                    ["timestamp"] = currentTime.ToString("O")
                }));
            }
            
            lastItemTime = currentTime;
            yield return item;
        }
        
        var totalDuration = DateTime.UtcNow - startTime;
        
        // Calculate performance metrics
        var avgInterItemTime = interItemTimes.Count > 0 ? interItemTimes.Average() : 0;
        var maxInterItemTime = interItemTimes.Count > 0 ? interItemTimes.Max() : 0;
        var minInterItemTime = interItemTimes.Count > 0 ? interItemTimes.Min() : 0;
        var throughput = itemCount / Math.Max(totalDuration.TotalSeconds, 0.001);
        
        activity?.SetTag("performance.items_count", itemCount);
        activity?.SetTag("performance.total_duration_ms", totalDuration.TotalMilliseconds);
        activity?.SetTag("performance.throughput_items_per_sec", throughput);
        activity?.SetTag("performance.avg_inter_item_delay_ms", avgInterItemTime);
        activity?.SetTag("performance.max_inter_item_delay_ms", maxInterItemTime);
        activity?.SetTag("performance.min_inter_item_delay_ms", minInterItemTime);
        
        logger.LogInformation(
            "📊 [STREAMING PERF] {RequestType} - Items: {ItemCount}, Duration: {Duration}ms, " +
            "Throughput: {Throughput:F2} items/sec, Avg Inter-Item: {AvgDelay:F2}ms",
            requestType, itemCount, totalDuration.TotalMilliseconds, throughput, avgInterItemTime);
    }
}