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
    /// Gets whether constraint logging is enabled based on options.
    /// </summary>
    private bool IsConstraintLoggingEnabled => 
        _loggingOptions?.EnableConstraintLogging ?? false;

    /// <summary>
    /// Gets whether middleware routing logging is enabled based on options.
    /// </summary>
    private bool IsMiddlewareRoutingLoggingEnabled => 
        _loggingOptions?.EnableMiddlewareRoutingLogging ?? false;

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

    #endregion

    #region Constraint-Based Middleware Routing Logging (NEW)

    /// <summary>
    /// Logs the start of notification middleware pipeline execution with constraint analysis.
    /// </summary>
    /// <param name="notificationType">The type of the notification being processed.</param>
    /// <param name="totalMiddleware">Total number of middleware components in the pipeline.</param>
    /// <param name="pipelineId">Unique identifier for this pipeline execution.</param>
    public void NotificationPipelineStarted(string notificationType, int totalMiddleware, string pipelineId)
    {
        if (!IsDebugLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;
        LogNotificationPipelineStarted(_logger, notificationType, totalMiddleware, pipelineId, null);
    }

    /// <summary>
    /// Logs the completion of notification middleware pipeline execution.
    /// </summary>
    /// <param name="notificationType">The type of the notification that was processed.</param>
    /// <param name="pipelineId">Unique identifier for this pipeline execution.</param>
    /// <param name="totalDurationMs">Total duration of pipeline execution in milliseconds.</param>
    /// <param name="executedCount">Number of middleware components that actually executed.</param>
    /// <param name="skippedCount">Number of middleware components that were skipped.</param>
    public void NotificationPipelineCompleted(string notificationType, string pipelineId, double totalDurationMs, int executedCount, int skippedCount)
    {
        if (!IsDebugLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;

        if (IsPerformanceLoggingEnabled)
        {
            LogNotificationPipelineCompletedWithPerformance(_logger, notificationType, pipelineId, totalDurationMs, executedCount, skippedCount, null);
        }
        else
        {
            LogNotificationPipelineCompleted(_logger, notificationType, pipelineId, executedCount, skippedCount, null);
        }
    }

    /// <summary>
    /// Logs middleware constraint compatibility checking.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware being checked.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="hasConstraints">Whether the middleware has type constraints.</param>
    /// <param name="constraintTypes">The constraint types if any.</param>
    public void MiddlewareConstraintCheck(string middlewareName, string notificationType, bool hasConstraints, string? constraintTypes = null)
    {
        if (!IsDebugLoggingEnabled || !IsConstraintLoggingEnabled) return;

        if (hasConstraints && !string.IsNullOrEmpty(constraintTypes))
        {
            LogMiddlewareConstraintCheckWithConstraints(_logger, middlewareName, notificationType, constraintTypes, null);
        }
        else
        {
            LogMiddlewareConstraintCheck(_logger, middlewareName, notificationType, hasConstraints, null);
        }
    }

    /// <summary>
    /// Logs the result of constraint compatibility validation.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="isCompatible">Whether the middleware is compatible with the notification.</param>
    /// <param name="reason">The reason for compatibility or incompatibility.</param>
    /// <param name="validationTimeMs">Time taken for validation in milliseconds.</param>
    public void MiddlewareConstraintValidation(string middlewareName, string notificationType, bool isCompatible, string reason, double validationTimeMs = 0)
    {
        if (!IsDebugLoggingEnabled || !IsConstraintLoggingEnabled) return;

        if (IsPerformanceLoggingEnabled && validationTimeMs > 0)
        {
            LogMiddlewareConstraintValidationWithPerformance(_logger, middlewareName, notificationType, isCompatible, reason, validationTimeMs, null);
        }
        else
        {
            LogMiddlewareConstraintValidation(_logger, middlewareName, notificationType, isCompatible, reason, null);
        }
    }

    /// <summary>
    /// Logs middleware execution decision (execute vs skip).
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="willExecute">Whether the middleware will execute.</param>
    /// <param name="reason">The reason for the decision.</param>
    /// <param name="order">The middleware execution order.</param>
    public void MiddlewareExecutionDecision(string middlewareName, string notificationType, bool willExecute, string reason, int order = 0)
    {
        if (!IsDebugLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;

        if (willExecute)
        {
            LogMiddlewareWillExecute(_logger, middlewareName, notificationType, order, reason, null);
        }
        else
        {
            LogMiddlewareWillSkip(_logger, middlewareName, notificationType, reason, null);
        }
    }

    /// <summary>
    /// Logs middleware execution start with constraint information.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="order">The middleware execution order.</param>
    /// <param name="isConstrained">Whether the middleware is type-constrained.</param>
    /// <param name="constraintType">The constraint type if applicable.</param>
    public void MiddlewareExecutionStarted(string middlewareName, string notificationType, int order, bool isConstrained, string? constraintType = null)
    {
        if (!IsDebugLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;

        if (isConstrained && !string.IsNullOrEmpty(constraintType))
        {
            LogMiddlewareExecutionStartedConstrained(_logger, middlewareName, notificationType, order, constraintType, null);
        }
        else
        {
            LogMiddlewareExecutionStarted(_logger, middlewareName, notificationType, order, null);
        }
    }

    /// <summary>
    /// Logs middleware execution completion with performance metrics.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="durationMs">Execution duration in milliseconds.</param>
    /// <param name="successful">Whether execution was successful.</param>
    /// <param name="isConstrained">Whether the middleware is type-constrained.</param>
    public void MiddlewareExecutionCompleted(string middlewareName, string notificationType, double durationMs, bool successful, bool isConstrained = false)
    {
        if (!IsDebugLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;

        if (IsPerformanceLoggingEnabled)
        {
            LogMiddlewareExecutionCompletedWithPerformance(_logger, middlewareName, notificationType, durationMs, successful, isConstrained, null);
        }
        else
        {
            LogMiddlewareExecutionCompleted(_logger, middlewareName, notificationType, successful, isConstrained, null);
        }
    }

    /// <summary>
    /// Logs constraint-specific method invocation (INotificationMiddleware&lt;T&gt;).
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="constraintType">The constraint type being used.</param>
    /// <param name="methodName">The method being invoked (e.g., "InvokeAsync").</param>
    public void ConstrainedMethodInvocation(string middlewareName, string notificationType, string constraintType, string methodName = "InvokeAsync")
    {
        if (!IsDebugLoggingEnabled || !IsConstraintLoggingEnabled) return;
        LogConstrainedMethodInvocation(_logger, middlewareName, notificationType, constraintType, methodName, null);
    }

    /// <summary>
    /// Logs fallback to general method invocation when constraint-specific method fails.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="reason">The reason for fallback.</param>
    public void ConstrainedMethodFallback(string middlewareName, string notificationType, string reason)
    {
        if (!IsDebugLoggingEnabled || !IsConstraintLoggingEnabled) return;
        LogConstrainedMethodFallback(_logger, middlewareName, notificationType, reason, null);
    }

    /// <summary>
    /// Logs constraint validation cache hit or miss.
    /// </summary>
    /// <param name="middlewareName">The name of the middleware.</param>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="cacheHit">Whether it was a cache hit.</param>
    /// <param name="cacheKey">The cache key used.</param>
    public void ConstraintValidationCache(string middlewareName, string notificationType, bool cacheHit, string? cacheKey = null)
    {
        if (!IsDebugLoggingEnabled || !IsConstraintLoggingEnabled) return;

        if (cacheHit)
        {
            LogConstraintValidationCacheHit(_logger, middlewareName, notificationType, cacheKey ?? "unknown", null);
        }
        else
        {
            LogConstraintValidationCacheMiss(_logger, middlewareName, notificationType, cacheKey ?? "unknown", null);
        }
    }

    /// <summary>
    /// Logs performance metrics for constraint checking overhead.
    /// </summary>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="totalConstraintCheckTimeMs">Total time spent on constraint checking.</param>
    /// <param name="middlewareCount">Number of middleware checked.</param>
    /// <param name="cacheHitCount">Number of cache hits.</param>
    public void ConstraintCheckingPerformance(string notificationType, double totalConstraintCheckTimeMs, int middlewareCount, int cacheHitCount = 0)
    {
        if (!IsDebugLoggingEnabled || !IsPerformanceLoggingEnabled || !IsConstraintLoggingEnabled) return;
        
        var averageTimePerMiddleware = middlewareCount > 0 ? totalConstraintCheckTimeMs / middlewareCount : 0;
        LogConstraintCheckingPerformance(_logger, notificationType, totalConstraintCheckTimeMs, middlewareCount, averageTimePerMiddleware, cacheHitCount, null);
    }

    /// <summary>
    /// Logs pipeline efficiency metrics.
    /// </summary>
    /// <param name="notificationType">The type of the notification.</param>
    /// <param name="totalMiddleware">Total middleware in pipeline.</param>
    /// <param name="executedMiddleware">Number of middleware that executed.</param>
    /// <param name="skippedMiddleware">Number of middleware that were skipped.</param>
    /// <param name="efficiencyPercentage">Efficiency percentage (executed / total * 100).</param>
    public void PipelineEfficiencyMetrics(string notificationType, int totalMiddleware, int executedMiddleware, int skippedMiddleware, double efficiencyPercentage)
    {
        if (!IsDebugLoggingEnabled || !IsPerformanceLoggingEnabled || !IsMiddlewareRoutingLoggingEnabled) return;
        LogPipelineEfficiencyMetrics(_logger, notificationType, totalMiddleware, executedMiddleware, skippedMiddleware, efficiencyPercentage, null);
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

    // Constraint-Based Middleware Routing Log Messages (NEW)
    [LoggerMessage(4001, LogLevel.Debug, "?? Notification pipeline started for {NotificationType} with {TotalMiddleware} middleware components [Pipeline: {PipelineId}]")]
    private static partial void LogNotificationPipelineStarted(ILogger logger, string notificationType, int totalMiddleware, string pipelineId, Exception? exception);

    [LoggerMessage(4002, LogLevel.Debug, "? Notification pipeline completed for {NotificationType} [Pipeline: {PipelineId}] - Executed: {ExecutedCount}, Skipped: {SkippedCount}")]
    private static partial void LogNotificationPipelineCompleted(ILogger logger, string notificationType, string pipelineId, int executedCount, int skippedCount, Exception? exception);

    [LoggerMessage(4003, LogLevel.Debug, "? Notification pipeline completed for {NotificationType} [Pipeline: {PipelineId}] in {TotalDurationMs:F2}ms - Executed: {ExecutedCount}, Skipped: {SkippedCount}")]
    private static partial void LogNotificationPipelineCompletedWithPerformance(ILogger logger, string notificationType, string pipelineId, double totalDurationMs, int executedCount, int skippedCount, Exception? exception);

    [LoggerMessage(4004, LogLevel.Debug, "?? Checking constraints for {MiddlewareName} with {NotificationType} (HasConstraints: {HasConstraints})")]
    private static partial void LogMiddlewareConstraintCheck(ILogger logger, string middlewareName, string notificationType, bool hasConstraints, Exception? exception);

    [LoggerMessage(4005, LogLevel.Debug, "?? Checking constraints for {MiddlewareName} with {NotificationType} - Constraints: [{ConstraintTypes}]")]
    private static partial void LogMiddlewareConstraintCheckWithConstraints(ILogger logger, string middlewareName, string notificationType, string constraintTypes, Exception? exception);

    [LoggerMessage(4006, LogLevel.Debug, "?? Constraint validation for {MiddlewareName} + {NotificationType}: {IsCompatible} ({Reason})")]
    private static partial void LogMiddlewareConstraintValidation(ILogger logger, string middlewareName, string notificationType, bool isCompatible, string reason, Exception? exception);

    [LoggerMessage(4007, LogLevel.Debug, "?? Constraint validation for {MiddlewareName} + {NotificationType}: {IsCompatible} ({Reason}) in {ValidationTimeMs:F2}ms")]
    private static partial void LogMiddlewareConstraintValidationWithPerformance(ILogger logger, string middlewareName, string notificationType, bool isCompatible, string reason, double validationTimeMs, Exception? exception);

    [LoggerMessage(4008, LogLevel.Debug, "? {MiddlewareName} will execute for {NotificationType} (Order: {Order}, Reason: {Reason})")]
    private static partial void LogMiddlewareWillExecute(ILogger logger, string middlewareName, string notificationType, int order, string reason, Exception? exception);

    [LoggerMessage(4009, LogLevel.Debug, "?? {MiddlewareName} will skip {NotificationType} (Reason: {Reason})")]
    private static partial void LogMiddlewareWillSkip(ILogger logger, string middlewareName, string notificationType, string reason, Exception? exception);

    [LoggerMessage(4010, LogLevel.Debug, "?? Executing {MiddlewareName} for {NotificationType} (Order: {Order})")]
    private static partial void LogMiddlewareExecutionStarted(ILogger logger, string middlewareName, string notificationType, int order, Exception? exception);

    [LoggerMessage(4011, LogLevel.Debug, "?? Executing constrained {MiddlewareName} for {NotificationType} (Order: {Order}, Constraint: {ConstraintType})")]
    private static partial void LogMiddlewareExecutionStartedConstrained(ILogger logger, string middlewareName, string notificationType, int order, string constraintType, Exception? exception);

    [LoggerMessage(4012, LogLevel.Debug, "? {MiddlewareName} completed {NotificationType} (Success: {Successful}, Constrained: {IsConstrained})")]
    private static partial void LogMiddlewareExecutionCompleted(ILogger logger, string middlewareName, string notificationType, bool successful, bool isConstrained, Exception? exception);

    [LoggerMessage(4013, LogLevel.Debug, "? {MiddlewareName} completed {NotificationType} in {DurationMs:F2}ms (Success: {Successful}, Constrained: {IsConstrained})")]
    private static partial void LogMiddlewareExecutionCompletedWithPerformance(ILogger logger, string middlewareName, string notificationType, double durationMs, bool successful, bool isConstrained, Exception? exception);

    [LoggerMessage(4014, LogLevel.Debug, "?? Invoking constraint-specific {MethodName} on {MiddlewareName} for {NotificationType} using constraint {ConstraintType}")]
    private static partial void LogConstrainedMethodInvocation(ILogger logger, string middlewareName, string notificationType, string constraintType, string methodName, Exception? exception);

    [LoggerMessage(4015, LogLevel.Debug, "?? Falling back to general method for {MiddlewareName} with {NotificationType} (Reason: {Reason})")]
    private static partial void LogConstrainedMethodFallback(ILogger logger, string middlewareName, string notificationType, string reason, Exception? exception);

    [LoggerMessage(4016, LogLevel.Debug, "?? Constraint validation cache HIT for {MiddlewareName} + {NotificationType} (Key: {CacheKey})")]
    private static partial void LogConstraintValidationCacheHit(ILogger logger, string middlewareName, string notificationType, string cacheKey, Exception? exception);

    [LoggerMessage(4017, LogLevel.Debug, "?? Constraint validation cache MISS for {MiddlewareName} + {NotificationType} (Key: {CacheKey})")]
    private static partial void LogConstraintValidationCacheMiss(ILogger logger, string middlewareName, string notificationType, string cacheKey, Exception? exception);

    [LoggerMessage(4018, LogLevel.Debug, "? Constraint checking performance for {NotificationType}: {TotalConstraintCheckTimeMs:F2}ms total, {MiddlewareCount} middleware, {AverageTimePerMiddleware:F2}ms avg, {CacheHitCount} cache hits")]
    private static partial void LogConstraintCheckingPerformance(ILogger logger, string notificationType, double totalConstraintCheckTimeMs, int middlewareCount, double averageTimePerMiddleware, int cacheHitCount, Exception? exception);

    [LoggerMessage(4019, LogLevel.Debug, "?? Pipeline efficiency for {NotificationType}: {ExecutedMiddleware}/{TotalMiddleware} executed, {SkippedMiddleware} skipped, {EfficiencyPercentage:F1}% efficiency")]
    private static partial void LogPipelineEfficiencyMetrics(ILogger logger, string notificationType, int totalMiddleware, int executedMiddleware, int skippedMiddleware, double efficiencyPercentage, Exception? exception);

    #endregion
}