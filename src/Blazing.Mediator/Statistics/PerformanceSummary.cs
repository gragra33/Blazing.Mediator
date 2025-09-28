namespace Blazing.Mediator.Statistics;

/// <summary>
/// Represents overall performance summary for mediator operations (requests, notifications, etc.).
/// </summary>
/// <param name="TotalOperations">Total number of operations processed.</param>
/// <param name="TotalFailures">Total number of failed operations.</param>
/// <param name="AverageExecutionTimeMs">Average execution time across all operations in milliseconds.</param>
/// <param name="OverallSuccessRate">Overall success rate as a percentage (0-100).</param>
/// <param name="TotalMemoryAllocatedBytes">Total memory allocated in bytes.</param>
/// <param name="UniqueOperationTypes">Number of unique operation types tracked.</param>
public readonly record struct PerformanceSummary(
    long TotalOperations,
    long TotalFailures,
    double AverageExecutionTimeMs,
    double OverallSuccessRate,
    long TotalMemoryAllocatedBytes,
    int UniqueOperationTypes
);