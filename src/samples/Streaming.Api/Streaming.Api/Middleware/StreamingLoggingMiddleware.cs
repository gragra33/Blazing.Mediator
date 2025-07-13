using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Text.Json;

namespace Streaming.Api.Middleware;

/// <summary>
/// Example streaming middleware for logging and monitoring streaming requests
/// </summary>
/// <typeparam name="TRequest">The stream request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class StreamingLoggingMiddleware<TRequest, TResponse> : IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    private readonly ILogger<StreamingLoggingMiddleware<TRequest, TResponse>> _logger;

    public StreamingLoggingMiddleware(ILogger<StreamingLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 0; // Execute first

    public async IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request, 
        StreamRequestHandlerDelegate<TResponse> next, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;
        var itemCount = 0;

        _logger.LogInformation("🚀 STREAM REQUEST: {RequestType} started at {StartTime}",
            requestType, startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        try
        {
            // Serialize and log request details (be careful with sensitive data in production)
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            _logger.LogInformation("📋 STREAM REQUEST DATA: {RequestData}", requestJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not serialize stream request: {Error}", ex.Message);
        }

        var streamStartTime = DateTime.UtcNow;
        _logger.LogInformation("🌊 Starting stream processing at {StreamStartTime}", streamStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

        IAsyncEnumerable<TResponse> stream;
        try
        {
            stream = next();
        }
        catch (Exception ex)
        {
            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogError(ex, "❌ STREAM REQUEST: {RequestType} failed to start at {EndTime}. " +
                                "Duration: {Duration}ms",
                requestType, endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), 
                duration.TotalMilliseconds);
            throw;
        }

        await foreach (var item in stream.WithCancellation(cancellationToken))
        {
            itemCount++;
            
            // Log every 100 items to avoid log spam
            if (itemCount % 100 == 0)
            {
                _logger.LogInformation("📦 Streamed {ItemCount} items so far", itemCount);
            }

            yield return item;
        }

        var completionTime = DateTime.UtcNow;
        var totalDuration = completionTime - startTime;
        var streamDuration = completionTime - streamStartTime;

        _logger.LogInformation("✅ STREAM REQUEST: {RequestType} completed at {EndTime}. " +
                               "Total duration: {Duration}ms, Stream duration: {StreamDuration}ms, Items: {ItemCount}",
            requestType, completionTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), 
            totalDuration.TotalMilliseconds, streamDuration.TotalMilliseconds, itemCount);
    }
}
