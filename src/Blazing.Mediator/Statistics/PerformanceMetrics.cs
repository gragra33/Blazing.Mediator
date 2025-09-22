namespace Blazing.Mediator.Statistics;

/// <summary>
/// Represents performance metrics for a specific request type.
/// </summary>
/// <param name="RequestType">The name of the request type.</param>
/// <param name="TotalExecutions">Total number of executions.</param>
/// <param name="FailedExecutions">Number of failed executions.</param>
/// <param name="AverageExecutionTimeMs">Average execution time in milliseconds.</param>
/// <param name="SuccessRate">Success rate as a percentage (0-100).</param>
/// <param name="LastExecutionTime">Timestamp of the last execution.</param>
/// <param name="P50ExecutionTimeMs">50th percentile execution time in milliseconds.</param>
/// <param name="P95ExecutionTimeMs">95th percentile execution time in milliseconds.</param>
/// <param name="P99ExecutionTimeMs">99th percentile execution time in milliseconds.</param>
public readonly record struct PerformanceMetrics(
    string RequestType,
    long TotalExecutions,
    long FailedExecutions,
    double AverageExecutionTimeMs,
    double SuccessRate,
    DateTime LastExecutionTime,
    double P50ExecutionTimeMs,
    double P95ExecutionTimeMs,
    double P99ExecutionTimeMs
);