namespace Blazing.Mediator.Logging;

/// <summary>
/// A wrapper that combines ILogger with LoggingOptions to provide granular control over debug logging.
/// Acts as a facade for the MediatorDebugLogger extension methods with selective enabling/disabling.
/// </summary>
public sealed class MediatorLogger
{
    private readonly ILogger<Mediator>? _logger;
    private readonly LoggingOptions _options;

    /// <summary>
    /// Initializes a new instance of the MediatorLogger class.
    /// </summary>
    /// <param name="logger">The underlying logger instance.</param>
    /// <param name="options">The logging options for granular control.</param>
    public MediatorLogger(ILogger<Mediator>? logger, LoggingOptions? options = null)
    {
        _logger = logger;
        _options = options ?? new LoggingOptions();
    }

    #region Query/Command Analyzer Logging

    public void AnalyzeQueriesStarted(string serviceProviderType)
    {
        if (_options.EnableQueryAnalyzer && _logger != null)
            _logger.AnalyzeQueriesStarted(serviceProviderType);
    }

    public void AnalyzeQueriesCompleted(int queryCount, bool isDetailed)
    {
        if (_options.EnableQueryAnalyzer && _logger != null)
            _logger.AnalyzeQueriesCompleted(queryCount, isDetailed);
    }

    public void AnalyzeQueryResult(string queryType, string responseType, string handlerStatus)
    {
        if (_options.EnableQueryAnalyzer && _logger != null)
            _logger.AnalyzeQueryResult(queryType, responseType, handlerStatus);
    }

    public void AnalyzeCommandsStarted(string serviceProviderType)
    {
        if (_options.EnableCommandAnalyzer && _logger != null)
            _logger.AnalyzeCommandsStarted(serviceProviderType);
    }

    public void AnalyzeCommandsCompleted(int commandCount, bool isDetailed)
    {
        if (_options.EnableCommandAnalyzer && _logger != null)
            _logger.AnalyzeCommandsCompleted(commandCount, isDetailed);
    }

    public void AnalyzeCommandResult(string commandType, string responseType, string handlerStatus)
    {
        if (_options.EnableCommandAnalyzer && _logger != null)
            _logger.AnalyzeCommandResult(commandType, responseType, handlerStatus);
    }

    #endregion

    #region Request Middleware Logging

    public void RequestMiddlewarePipelineStarted(string requestType, int middlewareCount)
    {
        if (_options.EnableRequestMiddleware && _logger != null)
            _logger.RequestMiddlewarePipelineStarted(requestType, middlewareCount);
    }

    public void RequestMiddlewareCompatibilityCheck(string middlewareType, string requestType)
    {
        if (_options.EnableRequestMiddleware && _logger != null)
            _logger.RequestMiddlewareCompatibilityCheck(middlewareType, requestType);
    }

    public void RequestMiddlewareCompatibilityResult(string middlewareType, string compatibilityStatus, string requestType, int order)
    {
        if (_options.EnableRequestMiddleware && _logger != null)
            _logger.RequestMiddlewareCompatibilityResult(middlewareType, compatibilityStatus, requestType, order);
    }

    public void RequestMiddlewarePipelineExecution(int applicableMiddlewareCount)
    {
        if (_options.EnableRequestMiddleware && _logger != null)
            _logger.RequestMiddlewarePipelineExecution(applicableMiddlewareCount);
    }

    public void RequestMiddlewarePipelineCompleted(string requestType, double durationMs)
    {
        if (_options.EnableRequestMiddleware && _options.EnablePerformanceTiming && _logger != null)
            _logger.RequestMiddlewarePipelineCompleted(requestType, durationMs);
    }

    #endregion

    #region Notification Middleware Logging

    public void NotificationMiddlewarePipelineStarted(string notificationType, int middlewareCount)
    {
        if (_options.EnableNotificationMiddleware && _logger != null)
            _logger.NotificationMiddlewarePipelineStarted(notificationType, middlewareCount);
    }

    public void NotificationMiddlewareCompatibilityCheck(string middlewareType, string notificationType)
    {
        if (_options.EnableNotificationMiddleware && _logger != null)
            _logger.NotificationMiddlewareCompatibilityCheck(middlewareType, notificationType);
    }

    public void NotificationMiddlewareCompatibilityResult(string middlewareType, string compatibilityStatus, string notificationType, int order)
    {
        if (_options.EnableNotificationMiddleware && _logger != null)
            _logger.NotificationMiddlewareCompatibilityResult(middlewareType, compatibilityStatus, notificationType, order);
    }

    public void NotificationMiddlewarePipelineExecution(int applicableMiddlewareCount)
    {
        if (_options.EnableNotificationMiddleware && _logger != null)
            _logger.NotificationMiddlewarePipelineExecution(applicableMiddlewareCount);
    }

    public void NotificationMiddlewarePipelineCompleted(string notificationType, double durationMs)
    {
        if (_options.EnableNotificationMiddleware && _options.EnablePerformanceTiming && _logger != null)
            _logger.NotificationMiddlewarePipelineCompleted(notificationType, durationMs);
    }

    #endregion

    #region Send Operations Logging

    public void SendOperationStarted(string requestType, bool telemetryEnabled)
    {
        if (_options.EnableSend && _logger != null)
            _logger.SendOperationStarted(requestType, telemetryEnabled);
    }

    public void SendRequestTypeClassification(string requestType, string requestCategory)
    {
        if (_options.EnableSend && _options.EnableDetailedTypeClassification && _logger != null)
            _logger.SendRequestTypeClassification(requestType, requestCategory);
    }

    public void SendHandlerResolution(string handlerType, string requestType)
    {
        if (_options.EnableSend && _logger != null)
            _logger.SendHandlerResolution(handlerType, requestType);
    }

    public void SendHandlerFound(string handlerName, string requestType)
    {
        if (_options.EnableSend && _options.EnableDetailedHandlerInfo && _logger != null)
            _logger.SendHandlerFound(handlerName, requestType);
    }

    public void SendOperationCompleted(string requestType, double durationMs, bool success)
    {
        if (_options.EnableSend && _options.EnablePerformanceTiming && _logger != null)
            _logger.SendOperationCompleted(requestType, durationMs, success);
    }

    public void SendStreamOperationStarted(string requestType)
    {
        if (_options.EnableSendStream && _logger != null)
            _logger.SendStreamOperationStarted(requestType);
    }

    public void SendStreamHandlerResolution(string handlerType, string requestType)
    {
        if (_options.EnableSendStream && _logger != null)
            _logger.SendStreamHandlerResolution(handlerType, requestType);
    }

    public void SendStreamHandlerFound(string handlerName, string requestType)
    {
        if (_options.EnableSendStream && _options.EnableDetailedHandlerInfo && _logger != null)
            _logger.SendStreamHandlerFound(handlerName, requestType);
    }

    public void SendStreamItemProcessed(int itemNumber, string requestType)
    {
        if (_options.EnableSendStream && _logger != null)
            _logger.SendStreamItemProcessed(itemNumber, requestType);
    }

    public void SendStreamOperationCompleted(string requestType, int totalItems)
    {
        if (_options.EnableSendStream && _logger != null)
            _logger.SendStreamOperationCompleted(requestType, totalItems);
    }

    #endregion

    #region Publish Operations Logging

    public void PublishOperationStarted(string notificationType, bool telemetryEnabled)
    {
        if (_options.EnablePublish && _logger != null)
            _logger.PublishOperationStarted(notificationType, telemetryEnabled);
    }

    public void PublishNotificationTypeClassification(string notificationType)
    {
        if (_options.EnablePublish && _options.EnableDetailedTypeClassification && _logger != null)
            _logger.PublishNotificationTypeClassification(notificationType);
    }

    public void PublishSubscriberResolution(int subscriberCount, string notificationType)
    {
        if (_options.EnablePublish && _logger != null)
            _logger.PublishSubscriberResolution(subscriberCount, notificationType);
    }

    public void PublishSubscriberProcessing(string subscriberName, string notificationType)
    {
        if (_options.EnablePublish && _options.EnableSubscriberDetails && _logger != null)
            _logger.PublishSubscriberProcessing(subscriberName, notificationType);
    }

    public void PublishSubscriberCompleted(string subscriberName, string notificationType, double durationMs, bool success)
    {
        if (_options.EnablePublish && _options.EnableSubscriberDetails && _options.EnablePerformanceTiming && _logger != null)
            _logger.PublishSubscriberCompleted(subscriberName, notificationType, durationMs, success);
    }

    public void PublishOperationCompleted(string notificationType, double durationMs, bool success, int subscriberCount)
    {
        if (_options.EnablePublish && _options.EnablePerformanceTiming && _logger != null)
            _logger.PublishOperationCompleted(notificationType, durationMs, success, subscriberCount);
    }

    #endregion

    #region Pipeline Resolution Logging

    public void PipelineResolutionStarted(string requestType, string pipelineType)
    {
        if (_options.EnableRequestPipelineResolution && _logger != null)
            _logger.PipelineResolutionStarted(requestType, pipelineType);
    }

    public void PipelineBuilderInfo(string builderType, string constraintInfo)
    {
        if (_options.EnableRequestPipelineResolution && _logger != null)
            _logger.PipelineBuilderInfo(builderType, constraintInfo);
    }

    public void PipelineMiddlewareRegistration(string middlewareType, int order, string requestType)
    {
        if (_options.EnableRequestPipelineResolution && _logger != null)
            _logger.PipelineMiddlewareRegistration(middlewareType, order, requestType);
    }

    public void PipelineCompositionCompleted(int middlewareCount, string handlerType)
    {
        if (_options.EnableRequestPipelineResolution && _logger != null)
            _logger.PipelineCompositionCompleted(middlewareCount, handlerType);
    }

    public void PipelineExecutionOrder(string executionOrder)
    {
        if (_options.EnableRequestPipelineResolution && _options.EnableMiddlewareExecutionOrder && _logger != null)
            _logger.PipelineExecutionOrder(executionOrder);
    }

    public void NotificationPipelineResolutionStarted(string notificationType)
    {
        if (_options.EnableNotificationPipelineResolution && _logger != null)
            _logger.NotificationPipelineResolutionStarted(notificationType);
    }

    public void NotificationPipelineCompositionCompleted(int middlewareCount)
    {
        if (_options.EnableNotificationPipelineResolution && _logger != null)
            _logger.NotificationPipelineCompositionCompleted(middlewareCount);
    }

    #endregion

    #region Error and Warning Logging

    public void GenericConstraintValidationFailed(string middlewareType, string requestType, string constraintDetails)
    {
        if (_options.EnableWarnings && _logger != null)
            _logger.GenericConstraintValidationFailed(middlewareType, requestType, constraintDetails);
    }

    public void NoHandlerFoundWarning(string requestType)
    {
        if (_options.EnableWarnings && _logger != null)
            _logger.NoHandlerFoundWarning(requestType);
    }

    public void MultipleHandlersFoundWarning(string requestType, string handlerList)
    {
        if (_options.EnableWarnings && _logger != null)
            _logger.MultipleHandlersFoundWarning(requestType, handlerList);
    }

    #endregion
}