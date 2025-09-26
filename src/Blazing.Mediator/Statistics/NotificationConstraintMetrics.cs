using System.Collections.Concurrent;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Metrics for notification constrained middleware execution and performance.
/// Tracks both executions and skips for type-constrained notification middleware,
/// including detailed information about processed notification types.
/// </summary>
public sealed class NotificationConstraintMetrics
{
    /// <summary>
    /// Gets or sets the notification middleware type name.
    /// </summary>
    public string MiddlewareType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the constraint type name (e.g., IOrderNotification, INotification).
    /// </summary>
    public string ConstraintType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of times this constrained notification middleware has executed.
    /// </summary>
    public long ExecutionCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of times this constrained notification middleware was skipped due to constraint mismatch.
    /// </summary>
    public long SkipCount { get; set; }

    /// <summary>
    /// Gets or sets the total execution time in milliseconds for all executions.
    /// </summary>
    public long TotalExecutionTime { get; set; }

    /// <summary>
    /// Gets or sets the number of successful executions.
    /// </summary>
    public long SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed executions.
    /// </summary>
    public long FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last execution.
    /// </summary>
    public DateTime LastExecution { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last skip.
    /// </summary>
    public DateTime LastSkip { get; set; }

    /// <summary>
    /// Gets the collection of notification types that were processed by this constrained middleware.
    /// </summary>
    public ConcurrentHashSet<string> ProcessedNotificationTypes { get; } = new();

    /// <summary>
    /// Gets the collection of notification types that were skipped by this constrained middleware due to constraint mismatch.
    /// </summary>
    public ConcurrentHashSet<string> SkippedNotificationTypes { get; } = new();

    /// <summary>
    /// Gets the average execution time per execution in milliseconds.
    /// </summary>
    public double AverageExecutionTime => ExecutionCount > 0 ? (double)TotalExecutionTime / ExecutionCount : 0;

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => ExecutionCount > 0 ? (double)SuccessCount / ExecutionCount * 100 : 0;

    /// <summary>
    /// Gets the efficiency rate (executions vs total opportunities) as a percentage.
    /// </summary>
    public double Efficiency => (ExecutionCount + SkipCount) > 0 ? (double)ExecutionCount / (ExecutionCount + SkipCount) * 100 : 0;

    /// <summary>
    /// Gets the total number of opportunities (executions + skips).
    /// </summary>
    public long TotalOpportunities => ExecutionCount + SkipCount;

    /// <summary>
    /// Gets the number of unique notification types processed.
    /// </summary>
    public int UniqueNotificationTypesProcessed => ProcessedNotificationTypes.Count;

    /// <summary>
    /// Gets the number of unique notification types skipped.
    /// </summary>
    public int UniqueNotificationTypesSkipped => SkippedNotificationTypes.Count;

    /// <summary>
    /// Gets the total number of unique notification types encountered (processed + skipped).
    /// </summary>
    public int TotalUniqueNotificationTypes => ProcessedNotificationTypes.Union(SkippedNotificationTypes).Count();

    /// <summary>
    /// Returns a string representation of the notification constraint metrics.
    /// </summary>
    public override string ToString()
    {
        return $"{MiddlewareType}<{ConstraintType}>: " +
               $"Executions={ExecutionCount}, " +
               $"Skips={SkipCount}, " +
               $"Efficiency={Efficiency:F1}%, " +
               $"Success={SuccessRate:F1}%, " +
               $"AvgTime={AverageExecutionTime:F2}ms, " +
               $"UniqueTypes={TotalUniqueNotificationTypes}";
    }
}