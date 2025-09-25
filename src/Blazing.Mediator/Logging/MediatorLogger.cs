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

    #endregion
}