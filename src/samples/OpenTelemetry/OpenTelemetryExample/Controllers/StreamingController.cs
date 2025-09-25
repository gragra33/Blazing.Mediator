using Blazing.Mediator;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;
using System.Text.Json;

namespace OpenTelemetryExample.Controllers;

/// <summary>
/// Controller for streaming operations demonstrating OpenTelemetry integration with streaming.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamingController(IMediator mediator, ILogger<StreamingController> logger) : ControllerBase
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Controller";
    private const string ControllerName = $"{AppSourceName}.{nameof(StreamingController)}";

    /// <summary>
    /// Stream data with generic string content for Blazor client compatibility.
    /// Returns IAsyncEnumerable for true streaming support.
    /// </summary>
    /// <param name="count">Number of items to stream (default: 50)</param>
    /// <param name="delayMs">Delay between items in milliseconds (default: 100)</param>
    /// <returns>Streaming JSON response as IAsyncEnumerable</returns>
    [HttpGet("stream-data")]
    public async IAsyncEnumerable<StreamResponseDto<string>> StreamData(
        [FromQuery] int count = 50,
        [FromQuery] int delayMs = 100)
    {
        using var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.StreamData", ActivityKind.Server);

        activity?.SetTag("controller.method", "StreamData");
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);

        logger.LogInformation("🌊 Starting generic data streaming: count={Count}, delay={DelayMs}ms",
            count, delayMs);

        // FIX: Use mediator with StreamUsersQuery instead of generating data directly
        var query = new StreamUsersQuery
        {
            Count = count,
            DelayMs = delayMs,
            SearchTerm = null,
            IncludeInactive = true
        };

        var itemCount = 0;
        var batchId = Guid.NewGuid().ToString("N")[..8];

        await foreach (var user in mediator.SendStream(query).ConfigureAwait(false))
        {
            itemCount++;

            var streamResponse = new StreamResponseDto<string>
            {
                Data = $"User: {user.Name} ({user.Email}) - Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC",
                Metadata = new StreamMetadataDto
                {
                    ItemNumber = itemCount,
                    Timestamp = DateTimeOffset.UtcNow,
                    BatchId = batchId,
                    IsLast = itemCount == count // Will be updated if we get fewer items
                }
            };

            activity?.AddEvent(new ActivityEvent($"controller.stream_data.item.{itemCount}", default, new ActivityTagsCollection
            {
                ["item.number"] = itemCount,
                ["user.id"] = user.Id,
                ["user.name"] = user.Name,
                ["data"] = streamResponse.Data,
                ["metadata.batch_id"] = streamResponse.Metadata.BatchId
            }));

            // Check if client disconnected
            if (HttpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("🔌 Client disconnected from stream-data at item {ItemCount}", itemCount);
                break;
            }

            yield return streamResponse;
        }

        activity?.SetTag("stream.items_streamed", itemCount);
        logger.LogInformation("Completed stream-data: {ItemCount} items streamed", itemCount);
    }

    /// <summary>
    /// Stream users with basic streaming response.
    /// </summary>
    /// <param name="count">Number of users to stream (default: 10)</param>
    /// <param name="delayMs">Delay between items in milliseconds (default: 500)</param>
    /// <param name="searchTerm">Optional search term to filter users</param>
    /// <param name="includeInactive">Whether to include inactive users</param>
    /// <returns>Stream of users</returns>
    [HttpGet("users")]
    public async IAsyncEnumerable<UserDto> StreamUsers(
        [FromQuery] int count = 10,
        [FromQuery] int delayMs = 500,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeInactive = false)
    {
        using var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.StreamUsers", ActivityKind.Server);

        activity?.SetTag("controller.method", "StreamUsers");
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);
        activity?.SetTag("include_inactive", includeInactive);

        logger.LogInformation("🌊 Starting user streaming: count={Count}, delay={DelayMs}ms, search={SearchTerm}",
            count, delayMs, searchTerm);

        var query = new StreamUsersQuery
        {
            Count = count,
            DelayMs = delayMs,
            SearchTerm = searchTerm,
            IncludeInactive = includeInactive
        };

        var itemCount = 0;
        await foreach (var user in mediator.SendStream(query).ConfigureAwait(false))
        {
            itemCount++;
            activity?.AddEvent(new ActivityEvent($"controller.stream.item.{itemCount}", default, new ActivityTagsCollection
            {
                ["item.number"] = itemCount,
                ["user.id"] = user.Id,
                ["user.name"] = user.Name
            }));

            yield return user;
        }

        activity?.SetTag("stream.items_streamed", itemCount);
        logger.LogInformation("Completed user streaming: {ItemCount} items streamed", itemCount);
    }

    /// <summary>
    /// Stream users with metadata in Server-Sent Events format.
    /// </summary>
    /// <param name="count">Number of users to stream (default: 10)</param>
    /// <param name="delayMs">Delay between items in milliseconds (default: 500)</param>
    /// <param name="searchTerm">Optional search term to filter users</param>
    /// <param name="includeInactive">Whether to include inactive users</param>
    /// <returns>Server-Sent Events stream</returns>
    [HttpGet("users/sse")]
    public async Task StreamUsersSSE(
        [FromQuery] int count = 10,
        [FromQuery] int delayMs = 500,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeInactive = false)
    {
        using var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.StreamUsersSSE", ActivityKind.Server);

        activity?.SetTag("controller.method", "StreamUsersSSE");
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);
        activity?.SetTag("include_inactive", includeInactive);

        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("Access-Control-Allow-Origin", "*");

        logger.LogInformation("📡 Starting SSE user streaming: count={Count}, delay={DelayMs}ms", count, delayMs);

        var query = new StreamUsersWithMetadataQuery
        {
            Count = count,
            DelayMs = delayMs,
            SearchTerm = searchTerm,
            IncludeInactive = includeInactive
        };

        var itemCount = 0;
        await foreach (var userResponse in mediator.SendStream(query).ConfigureAwait(false))
        {
            itemCount++;

            var jsonData = JsonSerializer.Serialize(userResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await Response.WriteAsync($"data: {jsonData}\n\n").ConfigureAwait(false);
            await Response.Body.FlushAsync().ConfigureAwait(false);

            activity?.AddEvent(new ActivityEvent($"controller.sse.item.{itemCount}", default, new ActivityTagsCollection
            {
                ["item.number"] = itemCount,
                ["user.id"] = userResponse.Data.Id,
                ["user.name"] = userResponse.Data.Name,
                ["metadata.batch_id"] = userResponse.Metadata.BatchId
            }));

            // Check if client disconnected
            if (HttpContext.RequestAborted.IsCancellationRequested)
            {
                logger.LogInformation("🔌 Client disconnected from SSE stream at item {ItemCount}", itemCount);
                break;
            }
        }

        activity?.SetTag("stream.items_streamed", itemCount);
        logger.LogInformation("Completed SSE user streaming: {ItemCount} items streamed", itemCount);
    }

    /// <summary>
    /// Stream users with metadata.
    /// </summary>
    /// <param name="count">Number of users to stream (default: 10)</param>
    /// <param name="delayMs">Delay between items in milliseconds (default: 500)</param>
    /// <param name="searchTerm">Optional search term to filter users</param>
    /// <param name="includeInactive">Whether to include inactive users</param>
    /// <returns>Stream of users with metadata</returns>
    [HttpGet("users/metadata")]
    public async IAsyncEnumerable<StreamResponseDto<UserDto>> StreamUsersWithMetadata(
        [FromQuery] int count = 10,
        [FromQuery] int delayMs = 500,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool includeInactive = false)
    {
        using var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.StreamUsersWithMetadata", ActivityKind.Server);

        activity?.SetTag("controller.method", "StreamUsersWithMetadata");
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);
        activity?.SetTag("include_inactive", includeInactive);

        logger.LogInformation("📦 Starting user streaming with metadata: count={Count}, delay={DelayMs}ms", count, delayMs);

        var query = new StreamUsersWithMetadataQuery
        {
            Count = count,
            DelayMs = delayMs,
            SearchTerm = searchTerm,
            IncludeInactive = includeInactive
        };

        var itemCount = 0;
        await foreach (var userResponse in mediator.SendStream(query).ConfigureAwait(false))
        {
            itemCount++;
            activity?.AddEvent(new ActivityEvent($"controller.metadata.item.{itemCount}", default, new ActivityTagsCollection
            {
                ["item.number"] = itemCount,
                ["user.id"] = userResponse.Data.Id,
                ["user.name"] = userResponse.Data.Name,
                ["metadata.batch_id"] = userResponse.Metadata.BatchId,
                ["metadata.is_last"] = userResponse.Metadata.IsLast
            }));

            yield return userResponse;
        }

        activity?.SetTag("stream.items_streamed", itemCount);
        logger.LogInformation("Completed user streaming with metadata: {ItemCount} items streamed", itemCount);
    }

    /// <summary>
    /// Health check endpoint for streaming functionality.
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult<object>> GetStreamingHealth()
    {
        using var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.GetStreamingHealth", ActivityKind.Server);

        activity?.SetTag("controller.method", "GetStreamingHealth");

        try
        {
            // Test basic streaming functionality
            var testQuery = new StreamUsersQuery { Count = 1, DelayMs = 0 };
            var testItems = new List<UserDto>();

            await foreach (var item in mediator.SendStream(testQuery).ConfigureAwait(false))
            {
                testItems.Add(item);
                break; // Only test one item
            }

            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Checks = new
                {
                    StreamingPipeline = "OK",
                    OpenTelemetryIntegration = Blazing.Mediator.Mediator.TelemetryEnabled ? "Enabled" : "Disabled",
                    MiddlewareCount = 3, // Number of streaming middleware
                    TestStreamItems = testItems.Count
                }
            };

            activity?.SetTag("health.status", "healthy");
            return Ok(health);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("health.status", "unhealthy");

            StreamingControllerLog.LogStreamingHealthCheckFailed(logger, ex);

            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }
}
