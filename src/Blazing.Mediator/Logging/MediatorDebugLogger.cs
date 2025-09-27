namespace Blazing.Mediator.Logging;

/// <summary>
/// High-performance logging implementation using compile-time source generation for Blazing.Mediator components.
/// This class provides centralized debug-level logging for analyzers, middleware, send operations, and pipeline resolution.
/// </summary>
internal static partial class MediatorDebugLogger
{
    #region Query/Command Analyzer Logging

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Starting analysis of queries. Service provider: {ServiceProviderType}")]
    public static partial void AnalyzeQueriesStarted(this ILogger logger, string serviceProviderType);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Found {QueryCount} query types during analysis. Detailed: {IsDetailed}")]
    public static partial void AnalyzeQueriesCompleted(this ILogger logger, int queryCount, bool isDetailed);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Query analysis result: {QueryType} -> {ResponseType}, Handler Status: {HandlerStatus}")]
    public static partial void AnalyzeQueryResult(this ILogger logger, string queryType, string responseType, string handlerStatus);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Starting analysis of commands. Service provider: {ServiceProviderType}")]
    public static partial void AnalyzeCommandsStarted(this ILogger logger, string serviceProviderType);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Found {CommandCount} command types during analysis. Detailed: {IsDetailed}")]
    public static partial void AnalyzeCommandsCompleted(this ILogger logger, int commandCount, bool isDetailed);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Command analysis result: {CommandType} -> {ResponseType}, Handler Status: {HandlerStatus}")]
    public static partial void AnalyzeCommandResult(this ILogger logger, string commandType, string responseType, string handlerStatus);

    #endregion

    #region Request Middleware Logging

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Starting request middleware pipeline for {RequestType}. Total middleware registered: {MiddlewareCount}")]
    public static partial void RequestMiddlewarePipelineStarted(this ILogger logger, string requestType, int middlewareCount);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Checking middleware compatibility: {MiddlewareType} for {RequestType}")]
    public static partial void RequestMiddlewareCompatibilityCheck(this ILogger logger, string middlewareType, string requestType);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Middleware {MiddlewareType} is {CompatibilityStatus} for {RequestType}. Order: {Order}")]
    public static partial void RequestMiddlewareCompatibilityResult(this ILogger logger, string middlewareType, string compatibilityStatus, string requestType, int order);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Executing middleware pipeline with {ApplicableMiddlewareCount} applicable middleware components")]
    public static partial void RequestMiddlewarePipelineExecution(this ILogger logger, int applicableMiddlewareCount);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Request middleware pipeline completed for {RequestType}. Duration: {DurationMs}ms")]
    public static partial void RequestMiddlewarePipelineCompleted(this ILogger logger, string requestType, double durationMs);

    #endregion

    #region Notification Middleware Logging

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Starting notification middleware pipeline for {NotificationType}. Total middleware registered: {MiddlewareCount}")]
    public static partial void NotificationMiddlewarePipelineStarted(this ILogger logger, string notificationType, int middlewareCount);

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Checking notification middleware compatibility: {MiddlewareType} for {NotificationType}")]
    public static partial void NotificationMiddlewareCompatibilityCheck(this ILogger logger, string middlewareType, string notificationType);

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Notification middleware {MiddlewareType} is {CompatibilityStatus} for {NotificationType}. Order: {Order}")]
    public static partial void NotificationMiddlewareCompatibilityResult(this ILogger logger, string middlewareType, string compatibilityStatus, string notificationType, int order);

    [LoggerMessage(
        EventId = 2104,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Executing notification pipeline with {ApplicableMiddlewareCount} applicable middleware components")]
    public static partial void NotificationMiddlewarePipelineExecution(this ILogger logger, int applicableMiddlewareCount);

    [LoggerMessage(
        EventId = 2105,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Notification middleware pipeline completed for {NotificationType}. Duration: {DurationMs}ms")]
    public static partial void NotificationMiddlewarePipelineCompleted(this ILogger logger, string notificationType, double durationMs);

    #endregion

    #region Send Operations Logging

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[SEND] Starting Send operation for request: {RequestType}. Telemetry enabled: {TelemetryEnabled}")]
    public static partial void SendOperationStarted(this ILogger logger, string requestType, bool telemetryEnabled);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[SEND] Request type determination: {RequestType} is {RequestCategory}")]
    public static partial void SendRequestTypeClassification(this ILogger logger, string requestType, string requestCategory);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[SEND] Handler resolution: Looking for {HandlerType} for {RequestType}")]
    public static partial void SendHandlerResolution(this ILogger logger, string handlerType, string requestType);

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[SEND] Handler found: {HandlerName} for {RequestType}")]
    public static partial void SendHandlerFound(this ILogger logger, string handlerName, string requestType);

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Debug,
        Message = "[SEND] Send operation completed for {RequestType}. Duration: {DurationMs}ms, Success: {Success}")]
    public static partial void SendOperationCompleted(this ILogger logger, string requestType, double durationMs, bool success);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Starting SendStream operation for request: {RequestType}")]
    public static partial void SendStreamOperationStarted(this ILogger logger, string requestType);

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream handler resolution: Looking for {HandlerType} for {RequestType}")]
    public static partial void SendStreamHandlerResolution(this ILogger logger, string handlerType, string requestType);

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream handler found: {HandlerName} for {RequestType}")]
    public static partial void SendStreamHandlerFound(this ILogger logger, string handlerName, string requestType);

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream item processed: Item #{ItemNumber} for {RequestType}")]
    public static partial void SendStreamItemProcessed(this ILogger logger, int itemNumber, string requestType);

    [LoggerMessage(
        EventId = 3015,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] SendStream operation completed for {RequestType}. Total items: {TotalItems}")]
    public static partial void SendStreamOperationCompleted(this ILogger logger, string requestType, int totalItems);

    #endregion

    #region Pipeline Resolution Logging

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Resolving middleware pipeline for {RequestType}. Pipeline type: {PipelineType}")]
    public static partial void PipelineResolutionStarted(this ILogger logger, string requestType, string pipelineType);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline builder type: {BuilderType}, Request constraints: {ConstraintInfo}")]
    public static partial void PipelineBuilderInfo(this ILogger logger, string builderType, string constraintInfo);

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Middleware registration: {MiddlewareType} with order {Order} registered for {RequestType}")]
    public static partial void PipelineMiddlewareRegistration(this ILogger logger, string middlewareType, int order, string requestType);

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline composition completed. Total applicable middleware: {MiddlewareCount}, Final handler: {HandlerType}")]
    public static partial void PipelineCompositionCompleted(this ILogger logger, int middlewareCount, string handlerType);

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline execution order: {ExecutionOrder}")]
    public static partial void PipelineExecutionOrder(this ILogger logger, string executionOrder);

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-PIPELINE] Resolving notification pipeline for {NotificationType}")]
    public static partial void NotificationPipelineResolutionStarted(this ILogger logger, string notificationType);

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-PIPELINE] Notification pipeline composition completed. Total applicable middleware: {MiddlewareCount}")]
    public static partial void NotificationPipelineCompositionCompleted(this ILogger logger, int middlewareCount);

    #endregion

    #region Publish Operations Logging

    [LoggerMessage(
        EventId = 3021,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Starting Publish operation for notification: {NotificationType}. Telemetry enabled: {TelemetryEnabled}")]
    public static partial void PublishOperationStarted(this ILogger logger, string notificationType, bool telemetryEnabled);

    [LoggerMessage(
        EventId = 3022,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Notification type determination: {NotificationType} is notification")]
    public static partial void PublishNotificationTypeClassification(this ILogger logger, string notificationType);

    [LoggerMessage(
        EventId = 3023,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Subscriber resolution: Found {SubscriberCount} subscribers for {NotificationType}")]
    public static partial void PublishSubscriberResolution(this ILogger logger, int subscriberCount, string notificationType);

    [LoggerMessage(
        EventId = 3024,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Processing subscriber: {SubscriberName} for {NotificationType}")]
    public static partial void PublishSubscriberProcessing(this ILogger logger, string subscriberName, string notificationType);

    [LoggerMessage(
        EventId = 3025,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Subscriber completed: {SubscriberName} for {NotificationType}. Duration: {DurationMs}ms, Success: {Success}")]
    public static partial void PublishSubscriberCompleted(this ILogger logger, string subscriberName, string notificationType, double durationMs, bool success);

    [LoggerMessage(
        EventId = 3026,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Publish operation completed for {NotificationType}. Duration: {DurationMs}ms, Success: {Success}, Subscribers: {SubscriberCount}")]
    public static partial void PublishOperationCompleted(this ILogger logger, string notificationType, double durationMs, bool success, int subscriberCount);

    #endregion

    #region Error and Warning Logging

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Warning,
        Message = "[WARNING] No handler found for {RequestType} during pipeline resolution")]
    public static partial void NoHandlerFoundWarning(this ILogger logger, string requestType);

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Warning,
        Message = "[WARNING] Multiple handlers found for {RequestType}: {HandlerList}")]
    public static partial void MultipleHandlersFoundWarning(this ILogger logger, string requestType, string handlerList);

    #endregion
}