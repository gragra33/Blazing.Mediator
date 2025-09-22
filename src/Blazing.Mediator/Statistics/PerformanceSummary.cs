namespace Blazing.Mediator.Statistics;

/// <summary>
/// Represents overall performance summary for all mediator operations.
/// </summary>
/// <param name="TotalRequests">Total number of requests processed.</param>
/// <param name="TotalFailures">Total number of failed requests.</param>
/// <param name="AverageExecutionTimeMs">Average execution time across all requests in milliseconds.</param>
/// <param name="OverallSuccessRate">Overall success rate as a percentage (0-100).</param>
/// <param name="TotalMemoryAllocatedBytes">Total memory allocated in bytes.</param>
/// <param name="UniqueRequestTypes">Number of unique request types tracked.</param>
public readonly record struct PerformanceSummary(
    long TotalRequests,
    long TotalFailures,
    double AverageExecutionTimeMs,
    double OverallSuccessRate,
    long TotalMemoryAllocatedBytes,
    int UniqueRequestTypes
);