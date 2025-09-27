using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Logging;

/// <summary>
/// High-performance source-generated logger for Blazing.Mediator operations.
/// Provides granular debug-level logging with modern .NET 9 source generation for optimal performance.
/// </summary>
public sealed partial class MediatorLogger
{
    private readonly ILogger<Mediator> _logger;
    private readonly LoggingOptions? _loggingOptions;

    /// <summary>
    /// Initializes a new instance of the MediatorLogger class.
    /// </summary>
    /// <param name="logger">The base logger instance.</param>
    /// <param name="loggingOptions">Optional logging options for configuration.</param>
    public MediatorLogger(ILogger<Mediator> logger, LoggingOptions? loggingOptions = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loggingOptions = loggingOptions;
    }

    /// <summary>
    /// Gets whether debug logging is enabled based on options and logger configuration.
    /// </summary>
    private bool IsDebugLoggingEnabled => 
        _logger.IsEnabled(LogLevel.Debug);

    /// <summary>
    /// Gets whether telemetry logging is enabled based on options.
    /// </summary>
    private bool IsTelemetryLoggingEnabled => 
        _loggingOptions?.EnableDetailedTypeClassification ?? false;

    /// <summary>
    /// Gets whether handler logging is enabled based on options.
    /// </summary>
    private bool IsHandlerLoggingEnabled => 
        _loggingOptions?.EnableDetailedHandlerInfo ?? true;

    /// <summary>
    /// Gets whether performance logging is enabled based on options.
    /// </summary>
    private bool IsPerformanceLoggingEnabled => 
        _loggingOptions?.EnablePerformanceTiming ?? false;

    /// <summary>
    /// Gets whether request middleware logging is enabled based on options.
    /// </summary>
    private bool IsRequestMiddlewareLoggingEnabled => 
        _loggingOptions?.EnableRequestMiddleware ?? true;

    #region Send Operation Logging

    /// <summary>
    /// Logs the start of a Send operation.
    /// </summary>
    /// <param name="requestType">The type of the request being sent.</param>
    /// <param name="telemetryEnabled">Whether telemetry is enabled for this operation.</param>
    public void SendOperationStarted(string requestType, bool telemetryEnabled)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableSend ?? true)) return;

        if (IsTelemetryLoggingEnabled)
        {
            LogSendOperationStartedWithTelemetry(_logger, requestType, telemetryEnabled, null);
        }
        else
        {
            LogSendOperationStarted(_logger, requestType, null);
        }
    }

    /// <summary>
    /// Logs the completion of a Send operation.
    /// </summary>
    /// <param name="requestType">The type of the request that was sent.</param>
    /// <param name="durationMs">The duration of the operation in milliseconds.</param>
    /// <param name="successful">Whether the operation was successful.</param>
    public void SendOperationCompleted(string requestType, double durationMs, bool successful)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableSend ?? true)) return;

        if (IsPerformanceLoggingEnabled)
        {
            LogSendOperationCompletedWithPerformance(_logger, requestType, durationMs, successful, null);
        }
        else
        {
            LogSendOperationCompleted(_logger, requestType, successful, null);
        }
    }

    /// <summary>
    /// Logs the classification of a request type (query vs command).
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <param name="classification">The classification (query or command).</param>
    public void SendRequestTypeClassification(string requestType, string classification)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableDetailedTypeClassification ?? false)) return;
        LogSendRequestTypeClassification(_logger, requestType, classification, null);
    }

    /// <summary>
    /// Logs handler resolution for a request.
    /// </summary>
    /// <param name="handlerType">The type of the handler being resolved.</param>
    /// <param name="requestType">The type of the request.</param>
    public void SendHandlerResolution(string handlerType, string requestType)
    {
        if (!IsHandlerLoggingEnabled || !IsDebugLoggingEnabled || !(_loggingOptions?.EnableSend ?? true)) return;
        LogSendHandlerResolution(_logger, handlerType, requestType, null);
    }

    /// <summary>
    /// Logs when a handler is successfully found.
    /// </summary>
    /// <param name="handlerType">The type of the handler that was found.</param>
    /// <param name="requestType">The type of the request.</param>
    public void SendHandlerFound(string handlerType, string requestType)
    {
        if (!IsHandlerLoggingEnabled || !IsDebugLoggingEnabled || !(_loggingOptions?.EnableSend ?? true)) return;
        LogSendHandlerFound(_logger, handlerType, requestType, null);
    }

    /// <summary>
    /// Logs a warning when no handler is found for a request.
    /// </summary>
    /// <param name="requestType">The type of the request with no handler.</param>
    public void NoHandlerFoundWarning(string requestType)
    {
        if (!(_loggingOptions?.EnableWarnings ?? true)) return;
        LogNoHandlerFoundWarning(_logger, requestType, null);
    }

    /// <summary>
    /// Logs a warning when multiple handlers are found for a request.
    /// </summary>
    /// <param name="requestType">The type of the request with multiple handlers.</param>
    /// <param name="handlerTypes">The types of the handlers found.</param>
    public void MultipleHandlersFoundWarning(string requestType, string handlerTypes)
    {
        if (!(_loggingOptions?.EnableWarnings ?? true)) return;
        LogMultipleHandlersFoundWarning(_logger, requestType, handlerTypes, null);
    }

    #endregion

    #region Stream Operation Logging

    /// <summary>
    /// Logs the start of a SendStream operation.
    /// </summary>
    /// <param name="requestType">The type of the stream request being sent.</param>
    public void SendStreamOperationStarted(string requestType)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableSendStream ?? true)) return;
        LogSendStreamOperationStarted(_logger, requestType, null);
    }

    /// <summary>
    /// Logs stream handler resolution.
    /// </summary>
    /// <param name="handlerType">The type of the stream handler being resolved.</param>
    /// <param name="requestType">The type of the stream request.</param>
    public void SendStreamHandlerResolution(string handlerType, string requestType)
    {
        if (!IsHandlerLoggingEnabled || !IsDebugLoggingEnabled || !(_loggingOptions?.EnableSendStream ?? true)) return;
        LogSendStreamHandlerResolution(_logger, handlerType, requestType, null);
    }

    /// <summary>
    /// Logs when a stream handler is successfully found.
    /// </summary>
    /// <param name="handlerType">The type of the stream handler that was found.</param>
    /// <param name="requestType">The type of the stream request.</param>
    public void SendStreamHandlerFound(string handlerType, string requestType)
    {
        if (!IsHandlerLoggingEnabled || !IsDebugLoggingEnabled || !(_loggingOptions?.EnableSendStream ?? true)) return;
        LogSendStreamHandlerFound(_logger, handlerType, requestType, null);
    }

    #endregion

    #region Publish Operation Logging

    /// <summary>
    /// Logs the start of a Publish operation.
    /// </summary>
    /// <param name="notificationType">The type of the notification being published.</param>
    /// <param name="telemetryEnabled">Whether telemetry is enabled for this operation.</param>
    public void PublishOperationStarted(string notificationType, bool telemetryEnabled)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnablePublish ?? true)) return;

        if (IsTelemetryLoggingEnabled)
        {
            LogPublishOperationStartedWithTelemetry(_logger, notificationType, telemetryEnabled, null);
        }
        else
        {
            LogPublishOperationStarted(_logger, notificationType, null);
        }
    }

    /// <summary>
    /// Logs subscriber resolution for a notification.
    /// </summary>
    /// <param name="totalProcessors">The total number of processors (subscribers and handlers).</param>
    /// <param name="notificationType">The type of the notification.</param>
    public void PublishSubscriberResolution(int totalProcessors, string notificationType)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnablePublish ?? true)) return;
        LogPublishSubscriberResolution(_logger, totalProcessors, notificationType, null);
    }

    /// <summary>
    /// Logs the start of processing for a specific subscriber or handler.
    /// </summary>
    /// <param name="processorName">The name of the processor (subscriber or handler).</param>
    /// <param name="notificationType">The type of the notification.</param>
    public void PublishSubscriberProcessing(string processorName, string notificationType)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableSubscriberDetails ?? true) || !(_loggingOptions?.EnablePublish ?? true)) return;
        LogPublishSubscriberProcessing(_logger, processorName, notificationType, null);
    }

    /// <summary>
    /// Logs the completion of processing for a specific subscriber or handler.
    /// </summary>
    /// <param name="processorName">The name of the processor (subscriber or handler).</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="durationMs">The duration of processing in milliseconds.</param>
    /// <param name="successful">Whether the processing was successful.</param>
    public void PublishSubscriberCompleted(string processorName, string notificationType, double durationMs, bool successful)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnableSubscriberDetails ?? true) || !(_loggingOptions?.EnablePublish ?? true)) return;

        if (IsPerformanceLoggingEnabled)
        {
            LogPublishSubscriberCompletedWithPerformance(_logger, processorName, notificationType, durationMs, successful, null);
        }
        else
        {
            LogPublishSubscriberCompleted(_logger, processorName, notificationType, successful, null);
        }
    }

    /// <summary>
    /// Logs the completion of a Publish operation.
    /// </summary>
    /// <param name="notificationType">The type of the notification that was published.</param>
    /// <param name="durationMs">The duration of the operation in milliseconds.</param>
    /// <param name="successful">Whether the operation was successful.</param>
    /// <param name="totalProcessors">The total number of processors that handled the notification.</param>
    public void PublishOperationCompleted(string notificationType, double durationMs, bool successful, int totalProcessors)
    {
        if (!IsDebugLoggingEnabled || !(_loggingOptions?.EnablePublish ?? true)) return;

        if (IsPerformanceLoggingEnabled)
        {
            LogPublishOperationCompletedWithPerformance(_logger, notificationType, durationMs, successful, totalProcessors, null);
        }
        else
        {
            LogPublishOperationCompleted(_logger, notificationType, successful, totalProcessors, null);
        }
    }

    /// <summary>
    /// Logs the start of notification middleware pipeline execution with constraint analysis.
    /// </summary>
    /// <param name="notificationType">The type of the notification being processed.</param>
    /// <param name="totalMiddleware">Total number of middleware components in the pipeline.</param>
    /// <param name="pipelineId">Unique identifier for this pipeline execution.</param>
    public void NotificationPipelineStarted(string notificationType, int totalMiddleware, string pipelineId)
    {
        if (!IsDebugLoggingEnabled) return;
        LogNotificationPipelineStarted(_logger, notificationType, totalMiddleware, pipelineId, null);
    }
    #endregion

    #region Request Middleware Pipeline Logging

    /// <summary>
    /// Logs the start of a request middleware pipeline execution.
    /// </summary>
    /// <param name="requestType">The name of the request type.</param>
    /// <param name="middlewareCount">The number of middleware components in the pipeline.</param>
    public void MiddlewarePipelineStarted(string requestType, int middlewareCount)
    {
        if (!IsDebugLoggingEnabled || !IsRequestMiddlewareLoggingEnabled) return;
        LogMiddlewarePipelineStarted(_logger, requestType, middlewareCount, null);
    }

    /// <summary>
    /// Logs middleware compatibility checking.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware being checked.</param>
    /// <param name="requestType">The name of the request type.</param>
    public void MiddlewareCompatibilityCheck(string middlewareName, string requestType)
    {
        if (!IsDebugLoggingEnabled || !IsRequestMiddlewareLoggingEnabled) return;
        LogMiddlewareCompatibilityCheck(_logger, middlewareName, requestType, null);
    }

    /// <summary>
    /// Logs when middleware is not compatible with the request type.
    /// </summary>
    /// <param name="middlewareName">The name of the incompatible middleware.</param>
    /// <param name="requestType">The name of the request type.</param>
    /// <param name="order">The order of the middleware.</param>
    public void MiddlewareIncompatible(string middlewareName, string requestType, int order)
    {
        if (!IsDebugLoggingEnabled || !IsRequestMiddlewareLoggingEnabled) return;
        LogMiddlewareIncompatible(_logger, middlewareName, requestType, order, null);
    }

    /// <summary>
    /// Logs when middleware is compatible with the request type.
    /// </summary>
    /// <param name="middlewareName">The name of the compatible middleware.</param>
    /// <param name="requestType">The name of the request type.</param>
    /// <param name="order">The order of the middleware.</param>
    public void MiddlewareCompatible(string middlewareName, string requestType, int order)
    {
        if (!IsDebugLoggingEnabled || !IsRequestMiddlewareLoggingEnabled) return;
        LogMiddlewareCompatible(_logger, middlewareName, requestType, order, null);
    }

    /// <summary>
    /// Logs pipeline execution information.
    /// </summary>
    /// <param name="applicableMiddlewareCount">The number of applicable middleware components.</param>
    public void PipelineExecution(int applicableMiddlewareCount)
    {
        if (!IsDebugLoggingEnabled || !IsRequestMiddlewareLoggingEnabled) return;
        LogPipelineExecution(_logger, applicableMiddlewareCount, null);
    }

    #endregion

    #region Statistics Logging

    /// <summary>
    /// Logs query increment.
    /// </summary>
    /// <param name="queryType">The query type.</param>
    public void QueryIncremented(string queryType)
    {
        if (!IsDebugLoggingEnabled) return;
        LogQueryIncremented(_logger, queryType, null);
    }

    /// <summary>
    /// Logs command increment.
    /// </summary>
    /// <param name="commandType">The command type.</param>
    public void CommandIncremented(string commandType)
    {
        if (!IsDebugLoggingEnabled) return;
        LogCommandIncremented(_logger, commandType, null);
    }

    /// <summary>
    /// Logs notification increment.
    /// </summary>
    /// <param name="notificationType">The notification type.</param>
    public void NotificationIncremented(string notificationType)
    {
        if (!IsDebugLoggingEnabled) return;
        LogNotificationIncremented(_logger, notificationType, null);
    }

    /// <summary>
    /// Logs execution time recording.
    /// </summary>
    /// <param name="requestType">The request type.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="successful">Whether the execution was successful.</param>
    public void ExecutionTimeRecorded(string requestType, long executionTimeMs, bool successful)
    {
        if (!IsDebugLoggingEnabled) return;
        LogExecutionTimeRecorded(_logger, requestType, executionTimeMs, successful, null);
    }

    /// <summary>
    /// Logs memory allocation recording.
    /// </summary>
    /// <param name="bytesAllocated">The bytes allocated.</param>
    public void MemoryAllocationRecorded(long bytesAllocated)
    {
        if (!IsDebugLoggingEnabled) return;
        LogMemoryAllocationRecorded(_logger, bytesAllocated, null);
    }

    /// <summary>
    /// Logs middleware execution recording.
    /// </summary>
    /// <param name="middlewareType">The middleware type.</param>
    /// <param name="executionTimeMs">The execution time in milliseconds.</param>
    /// <param name="successful">Whether the execution was successful.</param>
    public void MiddlewareExecutionRecorded(string middlewareType, long executionTimeMs, bool successful)
    {
        if (!IsDebugLoggingEnabled) return;
        LogMiddlewareExecutionRecorded(_logger, middlewareType, executionTimeMs, successful, null);
    }

    /// <summary>
    /// Logs execution pattern recording.
    /// </summary>
    /// <param name="requestType">The request type.</param>
    /// <param name="executionTime">The execution time.</param>
    public void ExecutionPatternRecorded(string requestType, DateTime executionTime)
    {
        if (!IsDebugLoggingEnabled) return;
        LogExecutionPatternRecorded(_logger, requestType, executionTime, null);
    }

    /// <summary>
    /// Logs statistics report start.
    /// </summary>
    public void StatisticsReportStarted()
    {
        if (!IsDebugLoggingEnabled) return;
        LogStatisticsReportStarted(_logger, null);
    }

    /// <summary>
    /// Logs statistics report completion.
    /// </summary>
    public void StatisticsReportCompleted()
    {
        if (!IsDebugLoggingEnabled) return;
        LogStatisticsReportCompleted(_logger, null);
    }

    /// <summary>
    /// Logs analysis start.
    /// </summary>
    /// <param name="analysisType">The analysis type.</param>
    public void AnalysisStarted(string analysisType)
    {
        if (!IsDebugLoggingEnabled) return;
        LogAnalysisStarted(_logger, analysisType, null);
    }

    /// <summary>
    /// Logs analysis completion.
    /// </summary>
    /// <param name="analysisType">The analysis type.</param>
    /// <param name="resultCount">The result count.</param>
    public void AnalysisCompleted(string analysisType, int resultCount)
    {
        if (!IsDebugLoggingEnabled) return;
        LogAnalysisCompleted(_logger, analysisType, resultCount, null);
    }

    /// <summary>
    /// Logs cleanup start.
    /// </summary>
    /// <param name="expiredEntries">The expired entries count.</param>
    public void CleanupStarted(int expiredEntries)
    {
        if (!IsDebugLoggingEnabled) return;
        LogCleanupStarted(_logger, expiredEntries, null);
    }

    /// <summary>
    /// Logs cleanup completion.
    /// </summary>
    /// <param name="removedEntries">The removed entries count.</param>
    public void CleanupCompleted(int removedEntries)
    {
        if (!IsDebugLoggingEnabled) return;
        LogCleanupCompleted(_logger, removedEntries, null);
    }

    /// <summary>
    /// Logs statistics disposal.
    /// </summary>
    public void StatisticsDisposed()
    {
        if (!IsDebugLoggingEnabled) return;
        LogStatisticsDisposed(_logger, null);
    }

    #endregion

    #region Cleanup Service Logging

    /// <summary>
    /// Logs cleanup service started.
    /// </summary>
    public void CleanupServiceStarted()
    {
        if (!IsDebugLoggingEnabled) return;
        LogCleanupServiceStarted(_logger, null);
    }

    /// <summary>
    /// Logs cleanup service completed.
    /// </summary>
    public void CleanupServiceCompleted()
    {
        if (!IsDebugLoggingEnabled) return;
        LogCleanupServiceCompleted(_logger, null);
    }

    /// <summary>
    /// Logs cleanup service error.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    public void CleanupServiceError(Exception exception)
    {
        if (!IsDebugLoggingEnabled) return;
        LogCleanupServiceError(_logger, exception);
    }

    #endregion

    #region Source-Generated Log Messages
    [LoggerMessage(1001, LogLevel.Debug, "Send operation started for {RequestType}")]
    private static partial void LogSendOperationStarted(ILogger logger, string requestType, Exception? exception);

    [LoggerMessage(1002, LogLevel.Debug, "Send operation started for {RequestType} (Telemetry: {TelemetryEnabled})")]
    private static partial void LogSendOperationStartedWithTelemetry(ILogger logger, string requestType, bool telemetryEnabled, Exception? exception);

    [LoggerMessage(1003, LogLevel.Debug, "Send operation completed for {RequestType} (Success: {Successful})")]
    private static partial void LogSendOperationCompleted(ILogger logger, string requestType, bool successful, Exception? exception);

    [LoggerMessage(1004, LogLevel.Debug, "Send operation completed for {RequestType} in {DurationMs:F2}ms (Success: {Successful})")]
    private static partial void LogSendOperationCompletedWithPerformance(ILogger logger, string requestType, double durationMs, bool successful, Exception? exception);

    [LoggerMessage(1005, LogLevel.Debug, "Request {RequestType} classified as {Classification}")]
    private static partial void LogSendRequestTypeClassification(ILogger logger, string requestType, string classification, Exception? exception);

    [LoggerMessage(1006, LogLevel.Debug, "Resolving handler {HandlerType} for request {RequestType}")]
    private static partial void LogSendHandlerResolution(ILogger logger, string handlerType, string requestType, Exception? exception);

    [LoggerMessage(1007, LogLevel.Debug, "Handler {HandlerType} found for request {RequestType}")]
    private static partial void LogSendHandlerFound(ILogger logger, string handlerType, string requestType, Exception? exception);

    [LoggerMessage(1008, LogLevel.Warning, "No handler found for request type {RequestType}")]
    private static partial void LogNoHandlerFoundWarning(ILogger logger, string requestType, Exception? exception);

    [LoggerMessage(1009, LogLevel.Warning, "Multiple handlers found for request type {RequestType}: {HandlerTypes}")]
    private static partial void LogMultipleHandlersFoundWarning(ILogger logger, string requestType, string handlerTypes, Exception? exception);

    // Stream Operation Log Messages
    [LoggerMessage(2001, LogLevel.Debug, "SendStream operation started for {RequestType}")]
    private static partial void LogSendStreamOperationStarted(ILogger logger, string requestType, Exception? exception);

    [LoggerMessage(2002, LogLevel.Debug, "Resolving stream handler {HandlerType} for request {RequestType}")]
    private static partial void LogSendStreamHandlerResolution(ILogger logger, string handlerType, string requestType, Exception? exception);

    [LoggerMessage(2003, LogLevel.Debug, "Stream handler {HandlerType} found for request {RequestType}")]
    private static partial void LogSendStreamHandlerFound(ILogger logger, string handlerType, string requestType, Exception? exception);

    // Publish Operation Log Messages
    [LoggerMessage(3001, LogLevel.Debug, "Publish operation started for {NotificationType}")]
    private static partial void LogPublishOperationStarted(ILogger logger, string notificationType, Exception? exception);

    [LoggerMessage(3002, LogLevel.Debug, "Publish operation started for {NotificationType} (Telemetry: {TelemetryEnabled})")]
    private static partial void LogPublishOperationStartedWithTelemetry(ILogger logger, string notificationType, bool telemetryEnabled, Exception? exception);

    [LoggerMessage(3003, LogLevel.Debug, "Resolved {TotalProcessors} processors for notification {NotificationType}")]
    private static partial void LogPublishSubscriberResolution(ILogger logger, int totalProcessors, string notificationType, Exception? exception);

    [LoggerMessage(3004, LogLevel.Debug, "Processing notification {NotificationType} with {ProcessorName}")]
    private static partial void LogPublishSubscriberProcessing(ILogger logger, string processorName, string notificationType, Exception? exception);

    [LoggerMessage(3005, LogLevel.Debug, "Processor {ProcessorName} completed notification {NotificationType} (Success: {Successful})")]
    private static partial void LogPublishSubscriberCompleted(ILogger logger, string processorName, string notificationType, bool successful, Exception? exception);

    [LoggerMessage(3006, LogLevel.Debug, "Processor {ProcessorName} completed notification {NotificationType} in {DurationMs:F2}ms (Success: {Successful})")]
    private static partial void LogPublishSubscriberCompletedWithPerformance(ILogger logger, string processorName, string notificationType, double durationMs, bool successful, Exception? exception);

    [LoggerMessage(3007, LogLevel.Debug, "Publish operation completed for {NotificationType} (Success: {Successful}, Processors: {TotalProcessors})")]
    private static partial void LogPublishOperationCompleted(ILogger logger, string notificationType, bool successful, int totalProcessors, Exception? exception);

    [LoggerMessage(3008, LogLevel.Debug, "Publish operation completed for {NotificationType} in {DurationMs:F2}ms (Success: {Successful}, Processors: {TotalProcessors})")]
    private static partial void LogPublishOperationCompletedWithPerformance(ILogger logger, string notificationType, double durationMs, bool successful, int totalProcessors, Exception? exception);

    [LoggerMessage(3009, LogLevel.Debug, "Notification pipeline started for {NotificationType} with {TotalMiddleware} middleware components [Pipeline: {PipelineId}]")]
    private static partial void LogNotificationPipelineStarted(ILogger logger, string notificationType, int totalMiddleware, string pipelineId, Exception? exception);

    // Request Middleware Pipeline Log Messages
    [LoggerMessage(4001, LogLevel.Debug, "Request middleware pipeline started for {RequestType} with {MiddlewareCount} middleware components")]
    private static partial void LogMiddlewarePipelineStarted(ILogger logger, string requestType, int middlewareCount, Exception? exception);

    [LoggerMessage(4002, LogLevel.Debug, "Checking middleware compatibility: {MiddlewareName} with request {RequestType}")]
    private static partial void LogMiddlewareCompatibilityCheck(ILogger logger, string middlewareName, string requestType, Exception? exception);

    [LoggerMessage(4003, LogLevel.Debug, "Middleware {MiddlewareName} is not compatible with request {RequestType}, order: {Order}")]
    private static partial void LogMiddlewareIncompatible(ILogger logger, string middlewareName, string requestType, int order, Exception? exception);

    [LoggerMessage(4004, LogLevel.Debug, "Middleware {MiddlewareName} is compatible with request {RequestType}, order: {Order}")]
    private static partial void LogMiddlewareCompatible(ILogger logger, string middlewareName, string requestType, int order, Exception? exception);

    [LoggerMessage(4005, LogLevel.Debug, "Executing request middleware pipeline with {ApplicableMiddlewareCount} applicable middleware components")]
    private static partial void LogPipelineExecution(ILogger logger, int applicableMiddlewareCount, Exception? exception);

    // Statistics Log Messages
    [LoggerMessage(5001, LogLevel.Debug, "Query count incremented for {QueryType}")]
    private static partial void LogQueryIncremented(ILogger logger, string queryType, Exception? exception);

    [LoggerMessage(5002, LogLevel.Debug, "Command count incremented for {CommandType}")]
    private static partial void LogCommandIncremented(ILogger logger, string commandType, Exception? exception);

    [LoggerMessage(5003, LogLevel.Debug, "Notification count incremented for {NotificationType}")]
    private static partial void LogNotificationIncremented(ILogger logger, string notificationType, Exception? exception);

    [LoggerMessage(5004, LogLevel.Debug, "Execution time recorded for {RequestType}: {ExecutionTimeMs}ms (Successful: {Successful})")]
    private static partial void LogExecutionTimeRecorded(ILogger logger, string requestType, long executionTimeMs, bool successful, Exception? exception);

    [LoggerMessage(5005, LogLevel.Debug, "Memory allocation recorded: {BytesAllocated} bytes")]
    private static partial void LogMemoryAllocationRecorded(ILogger logger, long bytesAllocated, Exception? exception);

    [LoggerMessage(5006, LogLevel.Debug, "Middleware execution recorded for {MiddlewareType}: {ExecutionTimeMs}ms (Successful: {Successful})")]
    private static partial void LogMiddlewareExecutionRecorded(ILogger logger, string middlewareType, long executionTimeMs, bool successful, Exception? exception);

    [LoggerMessage(5007, LogLevel.Debug, "Execution pattern recorded for {RequestType} at {ExecutionTime}")]
    private static partial void LogExecutionPatternRecorded(ILogger logger, string requestType, DateTime executionTime, Exception? exception);

    [LoggerMessage(5008, LogLevel.Debug, "Statistics report started")]
    private static partial void LogStatisticsReportStarted(ILogger logger, Exception? exception);

    [LoggerMessage(5009, LogLevel.Debug, "Statistics report completed")]
    private static partial void LogStatisticsReportCompleted(ILogger logger, Exception? exception);

    [LoggerMessage(5010, LogLevel.Debug, "Analysis started for {AnalysisType}")]
    private static partial void LogAnalysisStarted(ILogger logger, string analysisType, Exception? exception);

    [LoggerMessage(5011, LogLevel.Debug, "Analysis completed for {AnalysisType} with {ResultCount} results")]
    private static partial void LogAnalysisCompleted(ILogger logger, string analysisType, int resultCount, Exception? exception);

    [LoggerMessage(5012, LogLevel.Debug, "Statistics cleanup started: {ExpiredEntries} expired entries found")]
    private static partial void LogCleanupStarted(ILogger logger, int expiredEntries, Exception? exception);

    [LoggerMessage(5013, LogLevel.Debug, "Statistics cleanup completed: {RemovedEntries} entries removed")]
    private static partial void LogCleanupCompleted(ILogger logger, int removedEntries, Exception? exception);

    [LoggerMessage(5014, LogLevel.Debug, "MediatorStatistics disposed")]
    private static partial void LogStatisticsDisposed(ILogger logger, Exception? exception);

    // Cleanup Service Log Messages
    [LoggerMessage(6001, LogLevel.Debug, "Disposing static Mediator resources")]
    private static partial void LogCleanupServiceStarted(ILogger logger, Exception? exception);

    [LoggerMessage(6002, LogLevel.Debug, "Static Mediator resources disposed successfully")]
    private static partial void LogCleanupServiceCompleted(ILogger logger, Exception? exception);

    [LoggerMessage(6003, LogLevel.Warning, "Error disposing static Mediator resources")]
    private static partial void LogCleanupServiceError(ILogger logger, Exception exception);

    #endregion
}