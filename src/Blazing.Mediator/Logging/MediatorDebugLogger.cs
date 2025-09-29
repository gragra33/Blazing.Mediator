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
    private static partial void AnalyzeQueriesStartedImpl(this ILogger logger, string serviceProviderType);

    public static void AnalyzeQueriesStarted(this ILogger logger, LoggingOptions? options, string serviceProviderType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableQueryAnalyzer ?? true))
        {
            logger.AnalyzeQueriesStartedImpl(serviceProviderType);
        }
    }

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Found {QueryCount} query types during analysis. Detailed: {IsDetailed}")]
    private static partial void AnalyzeQueriesCompletedImpl(this ILogger logger, int queryCount, bool isDetailed);

    public static void AnalyzeQueriesCompleted(this ILogger logger, LoggingOptions? options, int queryCount, bool isDetailed)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableQueryAnalyzer ?? true))
        {
            logger.AnalyzeQueriesCompletedImpl(queryCount, isDetailed);
        }
    }

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Query analysis result: {QueryType} -> {ResponseType}, Handler Status: {HandlerStatus}")]
    private static partial void AnalyzeQueryResultImpl(this ILogger logger, string queryType, string responseType, string handlerStatus);

    public static void AnalyzeQueryResult(this ILogger logger, LoggingOptions? options, string queryType, string responseType, string handlerStatus)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableQueryAnalyzer ?? true))
        {
            logger.AnalyzeQueryResultImpl(queryType, responseType, handlerStatus);
        }
    }

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Starting analysis of commands. Service provider: {ServiceProviderType}")]
    private static partial void AnalyzeCommandsStartedImpl(this ILogger logger, string serviceProviderType);

    public static void AnalyzeCommandsStarted(this ILogger logger, LoggingOptions? options, string serviceProviderType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableCommandAnalyzer ?? true))
        {
            logger.AnalyzeCommandsStartedImpl(serviceProviderType);
        }
    }

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Found {CommandCount} command types during analysis. Detailed: {IsDetailed}")]
    private static partial void AnalyzeCommandsCompletedImpl(this ILogger logger, int commandCount, bool isDetailed);

    public static void AnalyzeCommandsCompleted(this ILogger logger, LoggingOptions? options, int commandCount, bool isDetailed)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableCommandAnalyzer ?? true))
        {
            logger.AnalyzeCommandsCompletedImpl(commandCount, isDetailed);
        }
    }

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Debug,
        Message = "[ANALYZER] Command analysis result: {CommandType} -> {ResponseType}, Handler Status: {HandlerStatus}")]
    private static partial void AnalyzeCommandResultImpl(this ILogger logger, string commandType, string responseType, string handlerStatus);

    public static void AnalyzeCommandResult(this ILogger logger, LoggingOptions? options, string commandType, string responseType, string handlerStatus)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableCommandAnalyzer ?? true))
        {
            logger.AnalyzeCommandResultImpl(commandType, responseType, handlerStatus);
        }
    }

    #endregion

    #region Request Middleware Logging

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Starting request middleware pipeline for {RequestType}. Total middleware registered: {MiddlewareCount}")]
    private static partial void RequestMiddlewarePipelineStartedImpl(this ILogger logger, string requestType, int middlewareCount);

    public static void RequestMiddlewarePipelineStarted(this ILogger logger, LoggingOptions? options, string requestType, int middlewareCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestMiddleware ?? true))
        {
            logger.RequestMiddlewarePipelineStartedImpl(requestType, middlewareCount);
        }
    }

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Checking middleware compatibility: {MiddlewareType} for {RequestType}")]
    private static partial void RequestMiddlewareCompatibilityCheckImpl(this ILogger logger, string middlewareType, string requestType);

    public static void RequestMiddlewareCompatibilityCheck(this ILogger logger, LoggingOptions? options, string middlewareType, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestMiddleware ?? true) && (options?.EnableMiddlewareRoutingLogging ?? false))
        {
            logger.RequestMiddlewareCompatibilityCheckImpl(middlewareType, requestType);
        }
    }

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Middleware {MiddlewareType} is {CompatibilityStatus} for {RequestType}. Order: {Order}")]
    private static partial void RequestMiddlewareCompatibilityResultImpl(this ILogger logger, string middlewareType, string compatibilityStatus, string requestType, int order);

    public static void RequestMiddlewareCompatibilityResult(this ILogger logger, LoggingOptions? options, string middlewareType, string compatibilityStatus, string requestType, int order)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestMiddleware ?? true) && (options?.EnableMiddlewareRoutingLogging ?? false))
        {
            logger.RequestMiddlewareCompatibilityResultImpl(middlewareType, compatibilityStatus, requestType, order);
        }
    }

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Executing middleware pipeline with {ApplicableMiddlewareCount} applicable middleware components")]
    private static partial void RequestMiddlewarePipelineExecutionImpl(this ILogger logger, int applicableMiddlewareCount);

    public static void RequestMiddlewarePipelineExecution(this ILogger logger, LoggingOptions? options, int applicableMiddlewareCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestMiddleware ?? true))
        {
            logger.RequestMiddlewarePipelineExecutionImpl(applicableMiddlewareCount);
        }
    }

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Request middleware pipeline completed for {RequestType}. Duration: {DurationMs}ms")]
    private static partial void RequestMiddlewarePipelineCompletedImpl(this ILogger logger, string requestType, double durationMs);

    public static void RequestMiddlewarePipelineCompleted(this ILogger logger, LoggingOptions? options, string requestType, double durationMs)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestMiddleware ?? true))
        {
            if (options?.EnablePerformanceTiming ?? true)
            {
                logger.RequestMiddlewarePipelineCompletedImpl(requestType, durationMs);
            }
            else
            {
                logger.RequestMiddlewarePipelineCompletedSimpleImpl(requestType);
            }
        }
    }

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Debug,
        Message = "[MIDDLEWARE] Request middleware pipeline completed for {RequestType}")]
    private static partial void RequestMiddlewarePipelineCompletedSimpleImpl(this ILogger logger, string requestType);

    #endregion

    #region Notification Middleware Logging

    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Starting notification middleware pipeline for {NotificationType}. Total middleware registered: {MiddlewareCount}")]
    private static partial void NotificationMiddlewarePipelineStartedImpl(this ILogger logger, string notificationType, int middlewareCount);

    public static void NotificationMiddlewarePipelineStarted(this ILogger logger, LoggingOptions? options, string notificationType, int middlewareCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationMiddleware ?? true))
        {
            logger.NotificationMiddlewarePipelineStartedImpl(notificationType, middlewareCount);
        }
    }

    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Checking notification middleware compatibility: {MiddlewareType} for {NotificationType}")]
    private static partial void NotificationMiddlewareCompatibilityCheckImpl(this ILogger logger, string middlewareType, string notificationType);

    public static void NotificationMiddlewareCompatibilityCheck(this ILogger logger, LoggingOptions? options, string middlewareType, string notificationType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationMiddleware ?? true) && (options?.EnableConstraintLogging ?? false))
        {
            logger.NotificationMiddlewareCompatibilityCheckImpl(middlewareType, notificationType);
        }
    }

    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Notification middleware {MiddlewareType} is {CompatibilityStatus} for {NotificationType}. Order: {Order}")]
    private static partial void NotificationMiddlewareCompatibilityResultImpl(this ILogger logger, string middlewareType, string compatibilityStatus, string notificationType, int order);

    public static void NotificationMiddlewareCompatibilityResult(this ILogger logger, LoggingOptions? options, string middlewareType, string compatibilityStatus, string notificationType, int order)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationMiddleware ?? true) && (options?.EnableConstraintLogging ?? false))
        {
            logger.NotificationMiddlewareCompatibilityResultImpl(middlewareType, compatibilityStatus, notificationType, order);
        }
    }

    [LoggerMessage(
        EventId = 2104,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Executing notification pipeline with {ApplicableMiddlewareCount} applicable middleware components")]
    private static partial void NotificationMiddlewarePipelineExecutionImpl(this ILogger logger, int applicableMiddlewareCount);

    public static void NotificationMiddlewarePipelineExecution(this ILogger logger, LoggingOptions? options, int applicableMiddlewareCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationMiddleware ?? true))
        {
            logger.NotificationMiddlewarePipelineExecutionImpl(applicableMiddlewareCount);
        }
    }

    [LoggerMessage(
        EventId = 2105,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Notification middleware pipeline completed for {NotificationType}. Duration: {DurationMs}ms")]
    private static partial void NotificationMiddlewarePipelineCompletedImpl(this ILogger logger, string notificationType, double durationMs);

    public static void NotificationMiddlewarePipelineCompleted(this ILogger logger, LoggingOptions? options, string notificationType, double durationMs)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationMiddleware ?? true))
        {
            if (options?.EnablePerformanceTiming ?? true)
            {
                logger.NotificationMiddlewarePipelineCompletedImpl(notificationType, durationMs);
            }
            else
            {
                logger.NotificationMiddlewarePipelineCompletedSimpleImpl(notificationType);
            }
        }
    }

    [LoggerMessage(
        EventId = 2106,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-MIDDLEWARE] Notification middleware pipeline completed for {NotificationType}")]
    private static partial void NotificationMiddlewarePipelineCompletedSimpleImpl(this ILogger logger, string notificationType);

    #endregion

    #region Send Operations Logging

    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Debug,
        Message = "[SEND] Starting Send operation for request: {RequestType}. Telemetry enabled: {TelemetryEnabled}")]
    private static partial void SendOperationStartedImpl(this ILogger logger, string requestType, bool telemetryEnabled);

    public static void SendOperationStarted(this ILogger logger, LoggingOptions? options, string requestType, bool telemetryEnabled)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSend ?? true))
        {
            if (options?.EnableDetailedTypeClassification ?? false)
            {
                logger.SendOperationStartedImpl(requestType, telemetryEnabled);
            }
            else
            {
                logger.SendOperationStartedSimpleImpl(requestType);
            }
        }
    }

    [LoggerMessage(
        EventId = 3007,
        Level = LogLevel.Debug,
        Message = "[SEND] Starting Send operation for request: {RequestType}")]
    private static partial void SendOperationStartedSimpleImpl(this ILogger logger, string requestType);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Debug,
        Message = "[SEND] Request type determination: {RequestType} is {RequestCategory}")]
    private static partial void SendRequestTypeClassificationImpl(this ILogger logger, string requestType, string requestCategory);

    public static void SendRequestTypeClassification(this ILogger logger, LoggingOptions? options, string requestType, string requestCategory)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSend ?? true) && (options?.EnableDetailedTypeClassification ?? false))
        {
            logger.SendRequestTypeClassificationImpl(requestType, requestCategory);
        }
    }

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Debug,
        Message = "[SEND] Handler resolution: Looking for {HandlerType} for {RequestType}")]
    private static partial void SendHandlerResolutionImpl(this ILogger logger, string handlerType, string requestType);

    public static void SendHandlerResolution(this ILogger logger, LoggingOptions? options, string handlerType, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSend ?? true) && (options?.EnableDetailedHandlerInfo ?? false))
        {
            logger.SendHandlerResolutionImpl(handlerType, requestType);
        }
    }

    [LoggerMessage(
        EventId = 3004,
        Level = LogLevel.Debug,
        Message = "[SEND] Handler found: {HandlerName} for {RequestType}")]
    private static partial void SendHandlerFoundImpl(this ILogger logger, string handlerName, string requestType);

    public static void SendHandlerFound(this ILogger logger, LoggingOptions? options, string handlerName, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSend ?? true) && (options?.EnableDetailedHandlerInfo ?? false))
        {
            logger.SendHandlerFoundImpl(handlerName, requestType);
        }
    }

    [LoggerMessage(
        EventId = 3005,
        Level = LogLevel.Debug,
        Message = "[SEND] Send operation completed for {RequestType}. Duration: {DurationMs}ms, Success: {Success}")]
    private static partial void SendOperationCompletedImpl(this ILogger logger, string requestType, double durationMs, bool success);

    public static void SendOperationCompleted(this ILogger logger, LoggingOptions? options, string requestType, double durationMs, bool success)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSend ?? true))
        {
            if (options?.EnablePerformanceTiming ?? true)
            {
                logger.SendOperationCompletedImpl(requestType, durationMs, success);
            }
            else
            {
                logger.SendOperationCompletedSimpleImpl(requestType, success);
            }
        }
    }

    [LoggerMessage(
        EventId = 3008,
        Level = LogLevel.Debug,
        Message = "[SEND] Send operation completed for {RequestType}. Success: {Success}")]
    private static partial void SendOperationCompletedSimpleImpl(this ILogger logger, string requestType, bool success);

    [LoggerMessage(
        EventId = 3011,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Starting SendStream operation for request: {RequestType}")]
    private static partial void SendStreamOperationStartedImpl(this ILogger logger, string requestType);

    public static void SendStreamOperationStarted(this ILogger logger, LoggingOptions? options, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSendStream ?? true))
        {
            logger.SendStreamOperationStartedImpl(requestType);
        }
    }

    [LoggerMessage(
        EventId = 3012,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream handler resolution: Looking for {HandlerType} for {RequestType}")]
    private static partial void SendStreamHandlerResolutionImpl(this ILogger logger, string handlerType, string requestType);

    public static void SendStreamHandlerResolution(this ILogger logger, LoggingOptions? options, string handlerType, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSendStream ?? true) && (options?.EnableDetailedHandlerInfo ?? false))
        {
            logger.SendStreamHandlerResolutionImpl(handlerType, requestType);
        }
    }

    [LoggerMessage(
        EventId = 3013,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream handler found: {HandlerName} for {RequestType}")]
    private static partial void SendStreamHandlerFoundImpl(this ILogger logger, string handlerName, string requestType);

    public static void SendStreamHandlerFound(this ILogger logger, LoggingOptions? options, string handlerName, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSendStream ?? true) && (options?.EnableDetailedHandlerInfo ?? false))
        {
            logger.SendStreamHandlerFoundImpl(handlerName, requestType);
        }
    }

    [LoggerMessage(
        EventId = 3014,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] Stream item processed: Item #{ItemNumber} for {RequestType}")]
    private static partial void SendStreamItemProcessedImpl(this ILogger logger, int itemNumber, string requestType);

    public static void SendStreamItemProcessed(this ILogger logger, LoggingOptions? options, int itemNumber, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSendStream ?? true))
        {
            logger.SendStreamItemProcessedImpl(itemNumber, requestType);
        }
    }

    [LoggerMessage(
        EventId = 3015,
        Level = LogLevel.Debug,
        Message = "[SEND-STREAM] SendStream operation completed for {RequestType}. Total items: {TotalItems}")]
    private static partial void SendStreamOperationCompletedImpl(this ILogger logger, string requestType, int totalItems);

    public static void SendStreamOperationCompleted(this ILogger logger, LoggingOptions? options, string requestType, int totalItems)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableSendStream ?? true))
        {
            logger.SendStreamOperationCompletedImpl(requestType, totalItems);
        }
    }

    #endregion

    #region Pipeline Resolution Logging

    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Resolving middleware pipeline for {RequestType}. Pipeline type: {PipelineType}")]
    private static partial void PipelineResolutionStartedImpl(this ILogger logger, string requestType, string pipelineType);

    public static void PipelineResolutionStarted(this ILogger logger, LoggingOptions? options, string requestType, string pipelineType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestPipelineResolution ?? true))
        {
            logger.PipelineResolutionStartedImpl(requestType, pipelineType);
        }
    }

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline builder type: {BuilderType}, Request constraints: {ConstraintInfo}")]
    private static partial void PipelineBuilderInfoImpl(this ILogger logger, string builderType, string constraintInfo);

    public static void PipelineBuilderInfo(this ILogger logger, LoggingOptions? options, string builderType, string constraintInfo)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestPipelineResolution ?? true) && (options?.EnableDetailedHandlerInfo ?? false))
        {
            logger.PipelineBuilderInfoImpl(builderType, constraintInfo);
        }
    }

    [LoggerMessage(
        EventId = 4003,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Middleware registration: {MiddlewareType} with order {Order} registered for {RequestType}")]
    private static partial void PipelineMiddlewareRegistrationImpl(this ILogger logger, string middlewareType, int order, string requestType);

    public static void PipelineMiddlewareRegistration(this ILogger logger, LoggingOptions? options, string middlewareType, int order, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestPipelineResolution ?? true) && (options?.EnableMiddlewareExecutionOrder ?? false))
        {
            logger.PipelineMiddlewareRegistrationImpl(middlewareType, order, requestType);
        }
    }

    [LoggerMessage(
        EventId = 4004,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline composition completed. Total applicable middleware: {MiddlewareCount}, Final handler: {HandlerType}")]
    private static partial void PipelineCompositionCompletedImpl(this ILogger logger, int middlewareCount, string handlerType);

    public static void PipelineCompositionCompleted(this ILogger logger, LoggingOptions? options, int middlewareCount, string handlerType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestPipelineResolution ?? true))
        {
            logger.PipelineCompositionCompletedImpl(middlewareCount, handlerType);
        }
    }

    [LoggerMessage(
        EventId = 4005,
        Level = LogLevel.Debug,
        Message = "[PIPELINE] Pipeline execution order: {ExecutionOrder}")]
    private static partial void PipelineExecutionOrderImpl(this ILogger logger, string executionOrder);

    public static void PipelineExecutionOrder(this ILogger logger, LoggingOptions? options, string executionOrder)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableRequestPipelineResolution ?? true) && (options?.EnableMiddlewareExecutionOrder ?? false))
        {
            logger.PipelineExecutionOrderImpl(executionOrder);
        }
    }

    [LoggerMessage(
        EventId = 4011,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-PIPELINE] Resolving notification pipeline for {NotificationType}")]
    private static partial void NotificationPipelineResolutionStartedImpl(this ILogger logger, string notificationType);

    public static void NotificationPipelineResolutionStarted(this ILogger logger, LoggingOptions? options, string notificationType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationPipelineResolution ?? true))
        {
            logger.NotificationPipelineResolutionStartedImpl(notificationType);
        }
    }

    [LoggerMessage(
        EventId = 4012,
        Level = LogLevel.Debug,
        Message = "[NOTIFICATION-PIPELINE] Notification pipeline composition completed. Total applicable middleware: {MiddlewareCount}")]
    private static partial void NotificationPipelineCompositionCompletedImpl(this ILogger logger, int middlewareCount);

    public static void NotificationPipelineCompositionCompleted(this ILogger logger, LoggingOptions? options, int middlewareCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnableNotificationPipelineResolution ?? true))
        {
            logger.NotificationPipelineCompositionCompletedImpl(middlewareCount);
        }
    }

    #endregion

    #region Publish Operations Logging

    [LoggerMessage(
        EventId = 3021,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Starting Publish operation for notification: {NotificationType}. Telemetry enabled: {TelemetryEnabled}")]
    private static partial void PublishOperationStartedImpl(this ILogger logger, string notificationType, bool telemetryEnabled);

    public static void PublishOperationStarted(this ILogger logger, LoggingOptions? options, string notificationType, bool telemetryEnabled)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true))
        {
            if (options?.EnableDetailedTypeClassification ?? false)
            {
                logger.PublishOperationStartedImpl(notificationType, telemetryEnabled);
            }
            else
            {
                logger.PublishOperationStartedSimpleImpl(notificationType);
            }
        }
    }

    [LoggerMessage(
        EventId = 3027,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Starting Publish operation for notification: {NotificationType}")]
    private static partial void PublishOperationStartedSimpleImpl(this ILogger logger, string notificationType);

    [LoggerMessage(
        EventId = 3022,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Notification type determination: {NotificationType} is notification")]
    private static partial void PublishNotificationTypeClassificationImpl(this ILogger logger, string notificationType);

    public static void PublishNotificationTypeClassification(this ILogger logger, LoggingOptions? options, string notificationType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true) && (options?.EnableDetailedTypeClassification ?? false))
        {
            logger.PublishNotificationTypeClassificationImpl(notificationType);
        }
    }

    [LoggerMessage(
        EventId = 3023,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Subscriber resolution: Found {SubscriberCount} subscribers for {NotificationType}")]
    private static partial void PublishSubscriberResolutionImpl(this ILogger logger, int subscriberCount, string notificationType);

    public static void PublishSubscriberResolution(this ILogger logger, LoggingOptions? options, int subscriberCount, string notificationType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true))
        {
            logger.PublishSubscriberResolutionImpl(subscriberCount, notificationType);
        }
    }

    [LoggerMessage(
        EventId = 3024,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Processing subscriber: {SubscriberName} for {NotificationType}")]
    private static partial void PublishSubscriberProcessingImpl(this ILogger logger, string subscriberName, string notificationType);

    public static void PublishSubscriberProcessing(this ILogger logger, LoggingOptions? options, string subscriberName, string notificationType)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true) && (options?.EnableSubscriberDetails ?? true))
        {
            logger.PublishSubscriberProcessingImpl(subscriberName, notificationType);
        }
    }

    [LoggerMessage(
        EventId = 3025,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Subscriber completed: {SubscriberName} for {NotificationType}. Duration: {DurationMs}ms, Success: {Success}")]
    private static partial void PublishSubscriberCompletedImpl(this ILogger logger, string subscriberName, string notificationType, double durationMs, bool success);

    public static void PublishSubscriberCompleted(this ILogger logger, LoggingOptions? options, string subscriberName, string notificationType, double durationMs, bool success)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true) && (options?.EnableSubscriberDetails ?? true))
        {
            if (options?.EnablePerformanceTiming ?? true)
            {
                logger.PublishSubscriberCompletedImpl(subscriberName, notificationType, durationMs, success);
            }
            else
            {
                logger.PublishSubscriberCompletedSimpleImpl(subscriberName, notificationType, success);
            }
        }
    }

    [LoggerMessage(
        EventId = 3028,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Subscriber completed: {SubscriberName} for {NotificationType}. Success: {Success}")]
    private static partial void PublishSubscriberCompletedSimpleImpl(this ILogger logger, string subscriberName, string notificationType, bool success);

    [LoggerMessage(
        EventId = 3026,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Publish operation completed for {NotificationType}. Duration: {DurationMs}ms, Success: {Success}, Subscribers: {SubscriberCount}")]
    private static partial void PublishOperationCompletedImpl(this ILogger logger, string notificationType, double durationMs, bool success, int subscriberCount);

    public static void PublishOperationCompleted(this ILogger logger, LoggingOptions? options, string notificationType, double durationMs, bool success, int subscriberCount)
    {
        if (logger.IsEnabled(LogLevel.Debug) && (options?.EnablePublish ?? true))
        {
            if (options?.EnablePerformanceTiming ?? true)
            {
                logger.PublishOperationCompletedImpl(notificationType, durationMs, success, subscriberCount);
            }
            else
            {
                logger.PublishOperationCompletedSimpleImpl(notificationType, success, subscriberCount);
            }
        }
    }

    [LoggerMessage(
        EventId = 3029,
        Level = LogLevel.Debug,
        Message = "[PUBLISH] Publish operation completed for {NotificationType}. Success: {Success}, Subscribers: {SubscriberCount}")]
    private static partial void PublishOperationCompletedSimpleImpl(this ILogger logger, string notificationType, bool success, int subscriberCount);

    #endregion

    #region Error and Warning Logging

    [LoggerMessage(
        EventId = 9002,
        Level = LogLevel.Warning,
        Message = "[WARNING] No handler found for {RequestType} during pipeline resolution")]
    private static partial void NoHandlerFoundWarningImpl(this ILogger logger, string requestType);

    public static void NoHandlerFoundWarning(this ILogger logger, LoggingOptions? options, string requestType)
    {
        if (logger.IsEnabled(LogLevel.Warning) && (options?.EnableWarnings ?? true))
        {
            logger.NoHandlerFoundWarningImpl(requestType);
        }
    }

    [LoggerMessage(
        EventId = 9003,
        Level = LogLevel.Warning,
        Message = "[WARNING] Multiple handlers found for {RequestType}: {HandlerList}")]
    private static partial void MultipleHandlersFoundWarningImpl(this ILogger logger, string requestType, string handlerList);

    public static void MultipleHandlersFoundWarning(this ILogger logger, LoggingOptions? options, string requestType, string handlerList)
    {
        if (logger.IsEnabled(LogLevel.Warning) && (options?.EnableWarnings ?? true))
        {
            logger.MultipleHandlersFoundWarningImpl(requestType, handlerList);
        }
    }

    #endregion
}