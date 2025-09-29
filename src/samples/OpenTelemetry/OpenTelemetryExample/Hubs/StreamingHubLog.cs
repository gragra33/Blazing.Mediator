namespace OpenTelemetryExample.Hubs;

public static partial class StreamingHubLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} started streaming")]
    public static partial void LogClientStartedStreaming(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "SignalR client {ConnectionId} completed streaming: {ItemCount} items")]
    public static partial void LogClientCompletedStreaming(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "?? SignalR streaming cancelled for client {ConnectionId}")]
    public static partial void LogStreamingCancelled(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "? SignalR streaming failed for client {ConnectionId}")]
    public static partial void LogStreamingFailed(this ILogger logger, Exception exception, string connectionId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} requested stop streaming")]
    public static partial void LogClientRequestedStop(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "SignalR streaming stopped for client {ConnectionId}")]
    public static partial void LogStreamingStopped(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} started streaming users")]
    public static partial void LogClientStartedStreamingUsers(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 8, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} disconnected during streaming at item {ItemCount}")]
    public static partial void LogClientDisconnectedDuringStreaming(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "SignalR client {ConnectionId} completed streaming: {ItemCount} users")]
    public static partial void LogClientCompletedStreamingUsers(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} started streaming users with metadata")]
    public static partial void LogClientStartedStreamingWithMetadata(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} disconnected during metadata streaming at item {ItemCount}")]
    public static partial void LogClientDisconnectedDuringMetadataStreaming(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "SignalR client {ConnectionId} completed metadata streaming: {ItemCount} users")]
    public static partial void LogClientCompletedMetadataStreaming(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 13, Level = LogLevel.Information, Message = "?? SignalR client {ConnectionId} started broadcast streaming")]
    public static partial void LogClientStartedBroadcastStreaming(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "?? Broadcast initiator {ConnectionId} disconnected at item {ItemCount}")]
    public static partial void LogBroadcastInitiatorDisconnected(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "SignalR broadcast completed by {ConnectionId}: {ItemCount} users")]
    public static partial void LogBroadcastCompleted(this ILogger logger, string connectionId, int itemCount);

    [LoggerMessage(EventId = 16, Level = LogLevel.Error, Message = "? SignalR broadcast failed for client {ConnectionId}")]
    public static partial void LogBroadcastFailed(this ILogger logger, Exception exception, string connectionId);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "?? SignalR client connected: {ConnectionId}")]
    public static partial void LogClientConnected(this ILogger logger, string connectionId);

    [LoggerMessage(EventId = 18, Level = LogLevel.Warning, Message = "?? SignalR client disconnected with error: {ConnectionId}")]
    public static partial void LogClientDisconnectedWithError(this ILogger logger, Exception exception, string connectionId);

    [LoggerMessage(EventId = 19, Level = LogLevel.Information, Message = "?? SignalR client disconnected: {ConnectionId}")]
    public static partial void LogClientDisconnected(this ILogger logger, string connectionId);
}
