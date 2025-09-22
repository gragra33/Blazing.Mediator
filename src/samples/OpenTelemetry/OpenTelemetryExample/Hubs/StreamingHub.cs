using Blazing.Mediator;
using Microsoft.AspNetCore.SignalR;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Hubs;

/// <summary>
/// SignalR hub for streaming users with OpenTelemetry integration.
/// </summary>
public class StreamingHub(IMediator mediator, ILogger<StreamingHub> logger) : Hub
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.SignalRHub";
    private const string HubName = $"{AppSourceName}.{nameof(StreamingHub)}";

    private static readonly Dictionary<string, CancellationTokenSource> _activeStreams = new();

    /// <summary>
    /// Start streaming with user data via mediator for consistent telemetry flow.
    /// </summary>
    public async Task StartStreaming(StreamingRequest request)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.StartStreaming");
        
        activity?.SetTag("signalr.method", "StartStreaming");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        activity?.SetTag("stream.count", request.Count);
        activity?.SetTag("stream.delay_ms", request.DelayMs);
        activity?.SetTag("stream.batch_size", request.BatchSize);

        var connectionId = Context.ConnectionId;
        logger.LogInformation("🔗 SignalR client {ConnectionId} started streaming", connectionId);

        // Cancel any existing stream for this connection
        if (_activeStreams.TryGetValue(connectionId, out var existingCts))
        {
            await existingCts.CancelAsync();
            existingCts.Dispose();
        }

        // Create new cancellation token source
        var cts = new CancellationTokenSource();
        _activeStreams[connectionId] = cts;

        try
        {
            // ✅ FIX: Use mediator with StreamUsersQuery instead of generating data directly
            var query = new StreamUsersQuery
            {
                Count = request.Count,
                DelayMs = request.DelayMs,
                SearchTerm = null,
                IncludeInactive = true
            };

            var itemCount = 0;
            var batchId = Guid.NewGuid().ToString("N")[..8];

            await foreach (var user in mediator.SendStream(query, cts.Token))
            {
                cts.Token.ThrowIfCancellationRequested();
                itemCount++;
                
                var streamResponse = new StreamResponseDto<string>
                {
                    Data = $"User: {user.Name} ({user.Email}) - Generated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC",
                    Metadata = new StreamMetadataDto
                    {
                        ItemNumber = itemCount,
                        Timestamp = DateTimeOffset.UtcNow,
                        BatchId = batchId,
                        IsLast = itemCount == request.Count // Will be updated if we get fewer items
                    }
                };
                
                activity?.AddEvent(new ActivityEvent($"signalr.stream.item.{itemCount}", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["user.id"] = user.Id,
                    ["user.name"] = user.Name,
                    ["data"] = streamResponse.Data,
                    ["metadata.batch_id"] = streamResponse.Metadata.BatchId,
                    ["connection.id"] = connectionId
                }));

                // Send to specific client
                await Clients.Caller.SendAsync("ReceiveStreamItem", streamResponse, cts.Token);

                // Check for batching (apply batching delay every N items)
                if (itemCount % request.BatchSize == 0 && request.DelayMs > 0)
                {
                    await Task.Delay(request.DelayMs, cts.Token);
                }
            }

            activity?.SetTag("stream.items_streamed", itemCount);
            activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully streamed {itemCount} items via SignalR");
            
            // Send completion signal
            await Clients.Caller.SendAsync("StreamCompleted", new { ItemCount = itemCount }, cts.Token);
            
            logger.LogInformation("✅ SignalR client {ConnectionId} completed streaming: {ItemCount} items", 
                connectionId, itemCount);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("🔌 SignalR streaming cancelled for client {ConnectionId}", connectionId);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            
            logger.LogError(ex, "❌ SignalR streaming failed for client {ConnectionId}", connectionId);
            
            // Send error to client
            await Clients.Caller.SendAsync("StreamError", ex.Message, CancellationToken.None);
        }
        finally
        {
            // Clean up cancellation token
            if (_activeStreams.Remove(connectionId, out var activeCts))
            {
                activeCts.Dispose();
            }
        }
    }

    /// <summary>
    /// Stop streaming for the current connection.
    /// </summary>
    public async Task StopStreaming()
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.StopStreaming");
        
        activity?.SetTag("signalr.method", "StopStreaming");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);

        var connectionId = Context.ConnectionId;
        logger.LogInformation("🛑 SignalR client {ConnectionId} requested stop streaming", connectionId);

        if (_activeStreams.TryGetValue(connectionId, out var cts))
        {
            await cts.CancelAsync();
            _activeStreams.Remove(connectionId);
            cts.Dispose();
            
            await Clients.Caller.SendAsync("StreamCompleted", new { Message = "Streaming stopped by user" });
            logger.LogInformation("✅ SignalR streaming stopped for client {ConnectionId}", connectionId);
        }
    }

    /// <summary>
    /// Stream users to connected clients with basic streaming.
    /// </summary>
    public async Task StreamUsers(int count = 10, int delayMs = 500, string? searchTerm = null, bool includeInactive = false)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.StreamUsers");
        
        activity?.SetTag("signalr.method", "StreamUsers");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);
        activity?.SetTag("include_inactive", includeInactive);

        var connectionId = Context.ConnectionId;
        logger.LogInformation("🔗 SignalR client {ConnectionId} started streaming users", connectionId);

        try
        {
            var query = new StreamUsersQuery
            {
                Count = count,
                DelayMs = delayMs,
                SearchTerm = searchTerm,
                IncludeInactive = includeInactive
            };

            var itemCount = 0;
            await foreach (var user in mediator.SendStream(query, Context.ConnectionAborted))
            {
                itemCount++;
                
                activity?.AddEvent(new ActivityEvent($"signalr.stream.item.{itemCount}", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["user.id"] = user.Id,
                    ["user.name"] = user.Name,
                    ["connection.id"] = connectionId
                }));

                // Send to specific client
                await Clients.Caller.SendAsync("ReceiveUser", user, Context.ConnectionAborted);

                // Check if client disconnected
                if (Context.ConnectionAborted.IsCancellationRequested)
                {
                    logger.LogInformation("🔌 SignalR client {ConnectionId} disconnected during streaming at item {ItemCount}", 
                        connectionId, itemCount);
                    break;
                }
            }

            activity?.SetTag("stream.items_streamed", itemCount);
            activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully streamed {itemCount} users via SignalR");
            
            // Send completion signal
            await Clients.Caller.SendAsync("StreamComplete", new { ItemCount = itemCount }, Context.ConnectionAborted);
            
            logger.LogInformation("✅ SignalR client {ConnectionId} completed streaming: {ItemCount} users", 
                connectionId, itemCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            
            logger.LogError(ex, "❌ SignalR streaming failed for client {ConnectionId}", connectionId);
            
            // Send error to client
            await Clients.Caller.SendAsync("StreamError", new { Error = ex.Message }, Context.ConnectionAborted);
        }
    }

    /// <summary>
    /// Stream users with metadata to connected clients.
    /// </summary>
    public async Task StreamUsersWithMetadata(int count = 10, int delayMs = 500, string? searchTerm = null, bool includeInactive = false)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.StreamUsersWithMetadata");
        
        activity?.SetTag("signalr.method", "StreamUsersWithMetadata");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);
        activity?.SetTag("include_inactive", includeInactive);

        var connectionId = Context.ConnectionId;
        logger.LogInformation("📦 SignalR client {ConnectionId} started streaming users with metadata", connectionId);

        try
        {
            var query = new StreamUsersWithMetadataQuery
            {
                Count = count,
                DelayMs = delayMs,
                SearchTerm = searchTerm,
                IncludeInactive = includeInactive
            };

            var itemCount = 0;
            await foreach (var userResponse in mediator.SendStream(query, Context.ConnectionAborted))
            {
                itemCount++;
                
                activity?.AddEvent(new ActivityEvent($"signalr.metadata.item.{itemCount}", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["user.id"] = userResponse.Data.Id,
                    ["user.name"] = userResponse.Data.Name,
                    ["metadata.batch_id"] = userResponse.Metadata.BatchId,
                    ["metadata.is_last"] = userResponse.Metadata.IsLast,
                    ["connection.id"] = connectionId
                }));

                // Send to specific client
                await Clients.Caller.SendAsync("ReceiveUserWithMetadata", userResponse, Context.ConnectionAborted);

                // Check if client disconnected
                if (Context.ConnectionAborted.IsCancellationRequested)
                {
                    logger.LogInformation("🔌 SignalR client {ConnectionId} disconnected during metadata streaming at item {ItemCount}", 
                        connectionId, itemCount);
                    break;
                }
            }

            activity?.SetTag("stream.items_streamed", itemCount);
            activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully streamed {itemCount} users with metadata via SignalR");
            
            // Send completion signal
            await Clients.Caller.SendAsync("StreamComplete", new { ItemCount = itemCount }, Context.ConnectionAborted);
            
            logger.LogInformation("✅ SignalR client {ConnectionId} completed metadata streaming: {ItemCount} users", 
                connectionId, itemCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            
            logger.LogError(ex, "❌ SignalR metadata streaming failed for client {ConnectionId}", connectionId);
            
            // Send error to client
            await Clients.Caller.SendAsync("StreamError", new { Error = ex.Message }, Context.ConnectionAborted);
        }
    }

    /// <summary>
    /// Broadcast streaming to all connected clients.
    /// </summary>
    public async Task BroadcastStreamUsers(int count = 5, int delayMs = 1000, string? searchTerm = null)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.BroadcastStreamUsers");
        
        activity?.SetTag("signalr.method", "BroadcastStreamUsers");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        activity?.SetTag("signalr.broadcast", true);
        activity?.SetTag("stream.count", count);
        activity?.SetTag("stream.delay_ms", delayMs);
        activity?.SetTag("search_term", searchTerm);

        var connectionId = Context.ConnectionId;
        logger.LogInformation("📡 SignalR client {ConnectionId} started broadcast streaming", connectionId);

        try
        {
            var query = new StreamUsersWithMetadataQuery
            {
                Count = count,
                DelayMs = delayMs,
                SearchTerm = searchTerm,
                IncludeInactive = false
            };

            var itemCount = 0;
            var broadcastId = Guid.NewGuid().ToString("N")[..8];
            
            activity?.SetTag("stream.broadcast_id", broadcastId);

            // Notify all clients that broadcast is starting
            await Clients.All.SendAsync("BroadcastStarted", new { BroadcastId = broadcastId, InitiatedBy = connectionId });

            await foreach (var userResponse in mediator.SendStream(query, Context.ConnectionAborted))
            {
                itemCount++;
                
                activity?.AddEvent(new ActivityEvent($"signalr.broadcast.item.{itemCount}", default, new ActivityTagsCollection
                {
                    ["item.number"] = itemCount,
                    ["user.id"] = userResponse.Data.Id,
                    ["user.name"] = userResponse.Data.Name,
                    ["broadcast.id"] = broadcastId,
                    ["initiated.by"] = connectionId
                }));

                // Enhance metadata with broadcast info
                userResponse.Metadata.BatchId = broadcastId;
                
                // Broadcast to all clients
                await Clients.All.SendAsync("BroadcastUser", userResponse, Context.ConnectionAborted);

                // Check if client disconnected
                if (Context.ConnectionAborted.IsCancellationRequested)
                {
                    logger.LogInformation("🔌 Broadcast initiator {ConnectionId} disconnected at item {ItemCount}", 
                        connectionId, itemCount);
                    break;
                }
            }

            activity?.SetTag("stream.items_streamed", itemCount);
            activity?.SetStatus(ActivityStatusCode.Ok, $"Successfully broadcast {itemCount} users via SignalR");
            
            // Send completion signal to all clients
            await Clients.All.SendAsync("BroadcastComplete", new { 
                BroadcastId = broadcastId, 
                ItemCount = itemCount, 
                InitiatedBy = connectionId 
            });
            
            logger.LogInformation("✅ SignalR broadcast completed by {ConnectionId}: {ItemCount} users", 
                connectionId, itemCount);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            
            logger.LogError(ex, "❌ SignalR broadcast failed for client {ConnectionId}", connectionId);
            
            // Send error to all clients
            await Clients.All.SendAsync("BroadcastError", new { Error = ex.Message, InitiatedBy = connectionId });
        }
    }

    /// <summary>
    /// Handle client connection events.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.OnConnectedAsync");
        
        activity?.SetTag("signalr.event", "connected");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        
        logger.LogInformation("🔗 SignalR client connected: {ConnectionId}", Context.ConnectionId);
        
        await Clients.Caller.SendAsync("Connected", new {
            Context.ConnectionId, 
            Timestamp = DateTime.UtcNow,
            Message = "Welcome to OpenTelemetry Streaming Hub!"
        });
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handle client disconnection events.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HubName}.OnDisconnectedAsync");
        
        activity?.SetTag("signalr.event", "disconnected");
        activity?.SetTag("signalr.connection_id", Context.ConnectionId);
        
        // Clean up any active streams for this connection
        if (_activeStreams.TryGetValue(Context.ConnectionId, out var cts))
        {
            await cts.CancelAsync();
            _activeStreams.Remove(Context.ConnectionId);
            cts.Dispose();
        }
        
        if (exception != null)
        {
            activity?.SetTag("disconnect.exception", exception.Message);
            logger.LogWarning(exception, "🔌 SignalR client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            logger.LogInformation("🔌 SignalR client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}