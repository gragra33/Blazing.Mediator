namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Interface for inspecting the notification middleware pipeline for debugging and monitoring.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface INotificationMiddlewarePipelineInspector
{
    /// <summary>
    /// Gets a read-only list of registered notification middleware types.
    /// </summary>
    /// <returns>Read-only list of notification middleware types in the order they were registered</returns>
    IReadOnlyList<Type> GetRegisteredMiddleware();

    /// <summary>
    /// Gets configuration information for registered notification middleware.
    /// Each tuple contains the middleware type and its associated configuration object (if any).
    /// </summary>
    /// <returns>Read-only list of middleware configuration information</returns>
    IReadOnlyList<(Type Type, object? Configuration)> GetMiddlewareConfiguration();

    /// <summary>
    /// Gets detailed information about registered notification middleware including types, order values, and configuration.
    /// When a service provider is provided, gets actual runtime order values from DI-registered instances.
    /// </summary>
    /// <param name="serviceProvider">Optional service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of notification middleware information with type, order, and configuration</returns>
    IReadOnlyList<(Type Type, int Order, object? Configuration)> GetDetailedMiddlewareInfo(IServiceProvider? serviceProvider = null);

    /// <summary>
    /// Analyzes the notification middleware pipeline and returns structured information about each middleware component.
    /// Provides detailed analysis including order display formatting, class names, type parameters, and constraint information.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances for actual order values.</param>
    /// <param name="isDetailed">Indicates whether to include detailed analysis information. Defaults to true.</param>
    /// <returns>Read-only list of middleware analysis information sorted by execution order</returns>
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider, bool? isDetailed = true);

    /// <summary>
    /// Analyzes the notification middleware pipeline and returns structured information about each middleware component.
    /// Provides detailed analysis including order display formatting, class names, and type parameters.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances for actual order values.</param>
    /// <returns>Read-only list of middleware analysis information sorted by execution order</returns>
    IReadOnlyList<MiddlewareAnalysis> AnalyzeMiddleware(IServiceProvider serviceProvider);

    /// <summary>
    /// Gets type constraint information for a specific notification type, showing which middleware will execute.
    /// This method analyzes constraint compatibility to help with debugging and performance optimization.
    /// </summary>
    /// <param name="notificationType">The notification type to analyze constraints for.</param>
    /// <param name="serviceProvider">Service provider to resolve middleware instances.</param>
    /// <returns>Constraint analysis result showing which middleware will execute and which will be skipped</returns>
    NotificationConstraintAnalysis AnalyzeConstraints(Type notificationType, IServiceProvider serviceProvider);

    /// <summary>
    /// Gets type constraint information for a specific notification type, showing which middleware will execute.
    /// Generic version for better type safety.
    /// </summary>
    /// <typeparam name="TNotification">The notification type to analyze constraints for.</typeparam>
    /// <param name="serviceProvider">Service provider to resolve middleware instances.</param>
    /// <returns>Constraint analysis result showing which middleware will execute and which will be skipped</returns>
    NotificationConstraintAnalysis AnalyzeConstraints<TNotification>(IServiceProvider serviceProvider)
        where TNotification : INotification;

    /// <summary>
    /// Gets a summary of all registered constraint types and the middleware that use them.
    /// Useful for understanding the overall constraint usage patterns in the pipeline.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances.</param>
    /// <returns>Dictionary mapping constraint types to the middleware that use them</returns>
    IReadOnlyDictionary<Type, IReadOnlyList<Type>> GetConstraintUsageMap(IServiceProvider serviceProvider);

    /// <summary>
    /// Performs comprehensive constraint compatibility analysis for the entire pipeline.
    /// Identifies potential issues with constraint configurations and provides optimization suggestions.
    /// </summary>
    /// <param name="serviceProvider">Service provider to resolve middleware instances.</param>
    /// <returns>Comprehensive pipeline constraint analysis with recommendations</returns>
    PipelineConstraintAnalysis AnalyzePipelineConstraints(IServiceProvider serviceProvider);

    /// <summary>
    /// Gets middleware execution path analysis for a specific notification instance.
    /// Shows the exact middleware that would execute for this notification, helping with debugging.
    /// </summary>
    /// <param name="notification">The notification instance to analyze.</param>
    /// <param name="serviceProvider">Service provider to resolve middleware instances.</param>
    /// <returns>Execution path analysis showing which middleware will execute and their order</returns>
    MiddlewareExecutionPath AnalyzeExecutionPath(INotification notification, IServiceProvider serviceProvider);
}

/// <summary>
/// Analysis result for notification constraint compatibility.
/// Used in Phase 3 Step 3.9 for constraint-based middleware routing analysis.
/// </summary>
public class NotificationConstraintAnalysis
{
    /// <summary>
    /// The notification type being analyzed.
    /// </summary>
    public Type NotificationType { get; set; } = null!;

    /// <summary>
    /// List of middleware types that will execute for this notification type.
    /// </summary>
    public List<Type> ApplicableMiddleware { get; set; } = new();

    /// <summary>
    /// Dictionary of middleware types that will be skipped and the reasons why.
    /// </summary>
    public Dictionary<Type, string> SkippedMiddleware { get; set; } = new();

    /// <summary>
    /// Total number of registered middleware.
    /// </summary>
    public int TotalMiddlewareCount { get; set; }

    /// <summary>
    /// Efficiency ratio (applicable / total) indicating pipeline optimization.
    /// </summary>
    public double ExecutionEfficiency { get; set; }
}

/// <summary>
/// Comprehensive analysis of pipeline constraint configuration.
/// Used in Phase 3 Step 3.9 for pipeline-wide constraint analysis.
/// </summary>
public class PipelineConstraintAnalysis
{
    /// <summary>
    /// Total number of middleware in the pipeline.
    /// </summary>
    public int TotalMiddlewareCount { get; set; }

    /// <summary>
    /// Number of middleware with no type constraints (execute for all notifications).
    /// </summary>
    public int GeneralMiddlewareCount { get; set; }

    /// <summary>
    /// Number of middleware with type constraints.
    /// </summary>
    public int ConstrainedMiddlewareCount { get; set; }

    /// <summary>
    /// Number of unique constraint types used across all middleware.
    /// </summary>
    public int UniqueConstraintTypes { get; set; }

    /// <summary>
    /// Mapping of constraint types to middleware that use them.
    /// </summary>
    public IReadOnlyDictionary<Type, IReadOnlyList<Type>> ConstraintUsageMap { get; set; } =
        new Dictionary<Type, IReadOnlyList<Type>>();

    /// <summary>
    /// List of optimization recommendations based on constraint analysis.
    /// </summary>
    public List<string> OptimizationRecommendations { get; set; } = new();
}

/// <summary>
/// Analysis of middleware execution path for a specific notification.
/// Used in Phase 3 Step 3.9 for execution path debugging.
/// </summary>
public class MiddlewareExecutionPath
{
    /// <summary>
    /// The notification type for this execution path.
    /// </summary>
    public Type NotificationType { get; set; } = null!;

    /// <summary>
    /// Total number of middleware in the pipeline.
    /// </summary>
    public int TotalMiddlewareCount { get; set; }

    /// <summary>
    /// Number of middleware that will execute for this notification.
    /// </summary>
    public int ExecutingMiddlewareCount { get; set; }

    /// <summary>
    /// Number of middleware that will be skipped for this notification.
    /// </summary>
    public int SkippingMiddlewareCount { get; set; }

    /// <summary>
    /// Detailed execution steps for each middleware.
    /// </summary>
    public List<MiddlewareExecutionStep> ExecutionSteps { get; set; } = new();

    /// <summary>
    /// Estimated total execution time in milliseconds.
    /// </summary>
    public double EstimatedTotalDurationMs { get; set; }
}

/// <summary>
/// Individual middleware execution step in the pipeline.
/// Used in Phase 3 Step 3.9 for detailed execution path analysis.
/// </summary>
public class MiddlewareExecutionStep
{
    /// <summary>
    /// The middleware type.
    /// </summary>
    public Type MiddlewareType { get; set; } = null!;

    /// <summary>
    /// Execution order of the middleware.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether this middleware will execute for the analyzed notification.
    /// </summary>
    public bool WillExecute { get; set; }

    /// <summary>
    /// Reason why the middleware will execute or be skipped.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Estimated execution duration in milliseconds.
    /// </summary>
    public double EstimatedDurationMs { get; set; }
}
