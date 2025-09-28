namespace Blazing.Mediator.Configuration;

/// <summary>
/// Configuration options for mediator debug logging.
/// Provides granular control over what debug information is logged and at what level.
/// </summary>
public sealed class LoggingOptions
{
    /// <summary>
    /// Gets or sets whether to enable request middleware logging.
    /// When enabled, logs middleware pipeline execution, compatibility checks, and timing.
    /// Default: true.
    /// </summary>
    public bool EnableRequestMiddleware { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable notification middleware logging.
    /// When enabled, logs notification middleware pipeline execution and compatibility.
    /// Default: true.
    /// </summary>
    public bool EnableNotificationMiddleware { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable Send operation logging.
    /// When enabled, logs Send command/query execution, handler resolution, and timing.
    /// Default: true.
    /// </summary>
    public bool EnableSend { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable SendStream operation logging.
    /// When enabled, logs streaming operations, handler resolution, and item processing.
    /// Default: true.
    /// </summary>
    public bool EnableSendStream { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable Publish operation logging.
    /// When enabled, logs notification publishing, subscriber resolution, and execution.
    /// Default: true.
    /// </summary>
    public bool EnablePublish { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable request pipeline resolution logging.
    /// When enabled, logs pipeline construction, middleware registration, and composition.
    /// Default: true.
    /// </summary>
    public bool EnableRequestPipelineResolution { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable notification pipeline resolution logging.
    /// When enabled, logs notification pipeline construction and middleware composition.
    /// Default: true.
    /// </summary>
    public bool EnableNotificationPipelineResolution { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable warning messages.
    /// When enabled, logs warnings for missing handlers, multiple handlers, and validation issues.
    /// Default: true.
    /// </summary>
    public bool EnableWarnings { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable query analyzer logging.
    /// When enabled, logs query analysis results, handler discovery, and type classification.
    /// Default: true.
    /// </summary>
    public bool EnableQueryAnalyzer { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable command analyzer logging.
    /// When enabled, logs command analysis results, handler discovery, and type classification.
    /// Default: true.
    /// </summary>
    public bool EnableCommandAnalyzer { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable statistics logging.
    /// When enabled, logs statistics collection, analysis, cleanup, and reporting operations.
    /// Default: true.
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log detailed request type classification.
    /// When enabled, provides additional details about request type determination.
    /// Default: false.
    /// </summary>
    public bool EnableDetailedTypeClassification { get; set; }

    /// <summary>
    /// Gets or sets whether to log detailed handler information.
    /// When enabled, includes handler type names, assembly information, and registration details.
    /// Default: false.
    /// </summary>
    public bool EnableDetailedHandlerInfo { get; set; }

    /// <summary>
    /// Gets or sets whether to log middleware execution order.
    /// When enabled, logs the complete execution order of middleware in the pipeline.
    /// Default: false.
    /// </summary>
    public bool EnableMiddlewareExecutionOrder { get; set; }

    /// <summary>
    /// Gets or sets whether to log performance timing details.
    /// When enabled, includes detailed timing information for all logged operations.
    /// Default: true.
    /// </summary>
    public bool EnablePerformanceTiming { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to log subscriber processing details.
    /// When enabled, logs individual subscriber execution during notification publishing.
    /// Default: true.
    /// </summary>
    public bool EnableSubscriberDetails { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable constraint-based middleware routing logging.
    /// When enabled, logs detailed constraint validation, middleware skipping decisions, 
    /// and execution flow for type-constrained notification middleware.
    /// Default: false (can be verbose).
    /// </summary>
    public bool EnableConstraintLogging { get; set; }

    /// <summary>
    /// Gets or sets whether to enable detailed middleware routing logging.
    /// When enabled, logs middleware execution decisions, pipeline flow, 
    /// and constraint-based routing information.
    /// Default: false (can be verbose).
    /// </summary>
    public bool EnableMiddlewareRoutingLogging { get; set; }

    /// <summary>
    /// Validates the logging options and returns any validation errors.
    /// </summary>
    /// <returns>A list of validation error messages, empty if all options are valid.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Check for potentially performance-impacting combinations
        if (EnableConstraintLogging && EnableMiddlewareRoutingLogging && EnablePerformanceTiming)
        {
            errors.Add("Warning: Enabling constraint logging, middleware routing logging, and performance timing together may significantly impact performance in high-volume scenarios.");
        }

        // Currently no other specific validation rules, but could be extended in the future
        // for things like mutually exclusive options or performance considerations

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Creates a copy of the current logging options.
    /// </summary>
    /// <returns>A new LoggingOptions instance with the same configuration.</returns>
    public LoggingOptions Clone()
    {
        return new LoggingOptions
        {
            EnableRequestMiddleware = EnableRequestMiddleware,
            EnableNotificationMiddleware = EnableNotificationMiddleware,
            EnableSend = EnableSend,
            EnableSendStream = EnableSendStream,
            EnablePublish = EnablePublish,
            EnableRequestPipelineResolution = EnableRequestPipelineResolution,
            EnableNotificationPipelineResolution = EnableNotificationPipelineResolution,
            EnableWarnings = EnableWarnings,
            EnableQueryAnalyzer = EnableQueryAnalyzer,
            EnableCommandAnalyzer = EnableCommandAnalyzer,
            EnableStatistics = EnableStatistics,
            EnableDetailedTypeClassification = EnableDetailedTypeClassification,
            EnableDetailedHandlerInfo = EnableDetailedHandlerInfo,
            EnableMiddlewareExecutionOrder = EnableMiddlewareExecutionOrder,
            EnablePerformanceTiming = EnablePerformanceTiming,
            EnableSubscriberDetails = EnableSubscriberDetails,
            EnableConstraintLogging = EnableConstraintLogging,
            EnableMiddlewareRoutingLogging = EnableMiddlewareRoutingLogging
        };
    }

    /// <summary>
    /// Creates logging options with all features disabled.
    /// Useful for high-performance scenarios where logging overhead should be minimized.
    /// </summary>
    /// <returns>A new LoggingOptions instance with all options disabled.</returns>
    public static LoggingOptions CreateMinimal()
    {
        return new LoggingOptions
        {
            EnableRequestMiddleware = false,
            EnableNotificationMiddleware = false,
            EnableSend = false,
            EnableSendStream = false,
            EnablePublish = false,
            EnableRequestPipelineResolution = false,
            EnableNotificationPipelineResolution = false,
            EnableWarnings = true, // Keep warnings enabled even in minimal mode
            EnableQueryAnalyzer = false,
            EnableCommandAnalyzer = false,
            EnableStatistics = false,
            EnableDetailedTypeClassification = false,
            EnableDetailedHandlerInfo = false,
            EnableMiddlewareExecutionOrder = false,
            EnablePerformanceTiming = false,
            EnableSubscriberDetails = false,
            EnableConstraintLogging = false,
            EnableMiddlewareRoutingLogging = false
        };
    }

    /// <summary>
    /// Creates logging options with all features enabled for maximum observability.
    /// Useful for development and debugging scenarios.
    /// </summary>
    /// <returns>A new LoggingOptions instance with all options enabled.</returns>
    public static LoggingOptions CreateVerbose()
    {
        return new LoggingOptions
        {
            EnableRequestMiddleware = true,
            EnableNotificationMiddleware = true,
            EnableSend = true,
            EnableSendStream = true,
            EnablePublish = true,
            EnableRequestPipelineResolution = true,
            EnableNotificationPipelineResolution = true,
            EnableWarnings = true,
            EnableQueryAnalyzer = true,
            EnableCommandAnalyzer = true,
            EnableStatistics = true,
            EnableDetailedTypeClassification = true,
            EnableDetailedHandlerInfo = true,
            EnableMiddlewareExecutionOrder = true,
            EnablePerformanceTiming = true,
            EnableSubscriberDetails = true,
            EnableConstraintLogging = true,
            EnableMiddlewareRoutingLogging = true
        };
    }

    /// <summary>
    /// Creates logging options optimized for constraint-based middleware debugging.
    /// Enables constraint and middleware routing logging while keeping other verbose options disabled.
    /// </summary>
    /// <returns>A new LoggingOptions instance optimized for constraint debugging.</returns>
    public static LoggingOptions CreateConstraintDebugging()
    {
        return new LoggingOptions
        {
            EnableRequestMiddleware = false,
            EnableNotificationMiddleware = true,
            EnableSend = false,
            EnableSendStream = false,
            EnablePublish = true,
            EnableRequestPipelineResolution = false,
            EnableNotificationPipelineResolution = true,
            EnableWarnings = true,
            EnableQueryAnalyzer = false,
            EnableCommandAnalyzer = false,
            EnableStatistics = false,
            EnableDetailedTypeClassification = false,
            EnableDetailedHandlerInfo = false,
            EnableMiddlewareExecutionOrder = true,
            EnablePerformanceTiming = true,
            EnableSubscriberDetails = false,
            EnableConstraintLogging = true,
            EnableMiddlewareRoutingLogging = true
        };
    }
}