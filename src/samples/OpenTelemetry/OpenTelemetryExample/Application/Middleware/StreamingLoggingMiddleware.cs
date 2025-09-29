using Blazing.Mediator;
using Blazing.Mediator;
using System.Runtime.CompilerServices;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Logging middleware for streaming requests with detailed telemetry.
/// </summary>
public class StreamingLoggingMiddleware<TRequest, TResponse>(
    ILogger<StreamingLoggingMiddleware<TRequest, TResponse>> logger)
    : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public int Order => 0; // Execute in the middle of the pipeline

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var requestData = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        logger.LogInformation(
            "🚀 [STREAMING LOG] Starting {RequestType} with data: {RequestData}",
            requestType, requestData);

        var itemCount = 0;
        var startTime = DateTime.UtcNow;

        await foreach (var item in next().WithCancellation(cancellationToken))
        {
            itemCount++;

            // Log first few items and then every 25th item
            if (itemCount <= 3 || itemCount % 25 == 0)
            {
                logger.LogDebug(
                    "📤 [STREAMING LOG] Item {ItemCount} emitted for {RequestType}",
                    itemCount, requestType);
            }

            yield return item;
        }

        var duration = DateTime.UtcNow - startTime;
        logger.LogInformation(
            "🏁 [STREAMING LOG] Completed {RequestType} - {ItemCount} items in {Duration}ms",
            requestType, itemCount, duration.TotalMilliseconds);
    }
}