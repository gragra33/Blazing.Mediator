using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Blazing.Mediator.Tests.Performance;

/// <summary>
/// Performance metrics and reporting for constraint checking analysis.
/// Provides detailed performance insights and recommendations.
/// </summary>
public class ConstraintPerformanceAnalyzer
{
    private readonly ILogger<ConstraintPerformanceAnalyzer>? _logger;
    private readonly List<PerformanceMetric> _metrics = new();

    public ConstraintPerformanceAnalyzer(ILogger<ConstraintPerformanceAnalyzer>? logger = null)
    {
        _logger = logger;
    }

    public void RecordMetric(string operation, TimeSpan duration, int itemCount = 1, long memoryDelta = 0)
    {
        var metric = new PerformanceMetric
        {
            Operation = operation,
            Duration = duration,
            ItemCount = itemCount,
            MemoryDelta = memoryDelta,
            Timestamp = DateTime.UtcNow
        };

        _metrics.Add(metric);
        
        _logger?.LogDebug("Performance metric recorded: {Operation} - {Duration}ms for {ItemCount} items",
            operation, duration.TotalMilliseconds, itemCount);
    }

    public PerformanceReport GenerateReport()
    {
        var report = new PerformanceReport
        {
            TotalMetrics = _metrics.Count,
            TotalDuration = _metrics.Sum(m => m.Duration.TotalMilliseconds),
            AverageDuration = _metrics.Count > 0 ? _metrics.Average(m => m.Duration.TotalMilliseconds) : 0,
            TotalMemoryDelta = _metrics.Sum(m => m.MemoryDelta),
            OperationBreakdown = _metrics
                .GroupBy(m => m.Operation)
                .ToDictionary(g => g.Key, g => new OperationStats
                {
                    Count = g.Count(),
                    TotalDuration = g.Sum(m => m.Duration.TotalMilliseconds),
                    AverageDuration = g.Average(m => m.Duration.TotalMilliseconds),
                    MinDuration = g.Min(m => m.Duration.TotalMilliseconds),
                    MaxDuration = g.Max(m => m.Duration.TotalMilliseconds),
                    TotalItems = g.Sum(m => m.ItemCount),
                    TotalMemory = g.Sum(m => m.MemoryDelta)
                }),
            Recommendations = GenerateRecommendations()
        };

        _logger?.LogInformation("Performance report generated: {TotalMetrics} metrics, {TotalDuration}ms total duration",
            report.TotalMetrics, report.TotalDuration);

        return report;
    }

    private List<string> GenerateRecommendations()
    {
        var recommendations = new List<string>();

        if (_metrics.Count == 0)
            return recommendations;

        // Analyze constraint checking overhead
        var constraintMetrics = _metrics.Where(m => m.Operation.Contains("Constraint", StringComparison.OrdinalIgnoreCase)).ToList();
        if (constraintMetrics.Any())
        {
            var avgConstraintTime = constraintMetrics.Average(m => m.Duration.TotalMilliseconds / Math.Max(m.ItemCount, 1));
            if (avgConstraintTime > 0.01) // More than 0.01ms per constraint check
            {
                recommendations.Add($"Constraint checking is taking {avgConstraintTime:F4}ms per operation. Consider implementing caching for constraint resolution.");
            }
        }

        // Analyze memory usage
        var memoryMetrics = _metrics.Where(m => m.MemoryDelta > 0).ToList();
        if (memoryMetrics.Any())
        {
            var avgMemoryPerItem = memoryMetrics.Average(m => (double)m.MemoryDelta / Math.Max(m.ItemCount, 1));
            if (avgMemoryPerItem > 1000) // More than 1KB per item
            {
                recommendations.Add($"High memory usage detected: {avgMemoryPerItem:F0} bytes per operation. Review memory allocation patterns.");
            }
        }

        // Analyze operation frequency
        var operationFrequency = _metrics.GroupBy(m => m.Operation).ToDictionary(g => g.Key, g => g.Count());
        var mostFrequentOp = operationFrequency.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        if (mostFrequentOp.Value > 1000)
        {
            recommendations.Add($"Operation '{mostFrequentOp.Key}' is called {mostFrequentOp.Value} times. Consider optimization for high-frequency operations.");
        }

        // Analyze pipeline efficiency
        var pipelineMetrics = _metrics.Where(m => m.Operation.Contains("Pipeline", StringComparison.OrdinalIgnoreCase)).ToList();
        if (pipelineMetrics.Any())
        {
            var avgPipelineTime = pipelineMetrics.Average(m => m.Duration.TotalMilliseconds);
            if (avgPipelineTime > 10) // More than 10ms per pipeline execution
            {
                recommendations.Add($"Pipeline execution is taking {avgPipelineTime:F2}ms on average. Consider reducing middleware overhead or optimizing middleware order.");
            }
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Performance looks good! No specific optimizations recommended at this time.");
        }

        return recommendations;
    }

    public void ClearMetrics()
    {
        _metrics.Clear();
        _logger?.LogDebug("Performance metrics cleared");
    }

    public static async Task<T> MeasureAsync<T>(ConstraintPerformanceAnalyzer analyzer, string operation, 
        Func<Task<T>> action, int itemCount = 1)
    {
        var stopwatch = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        
        try
        {
            var result = await action();
            return result;
        }
        finally
        {
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            long memoryDelta = finalMemory - initialMemory;
            
            analyzer.RecordMetric(operation, stopwatch.Elapsed, itemCount, memoryDelta);
        }
    }

    public static T Measure<T>(ConstraintPerformanceAnalyzer analyzer, string operation, 
        Func<T> action, int itemCount = 1)
    {
        var stopwatch = Stopwatch.StartNew();
        long initialMemory = GC.GetTotalMemory(false);
        
        try
        {
            var result = action();
            return result;
        }
        finally
        {
            stopwatch.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            long memoryDelta = finalMemory - initialMemory;
            
            analyzer.RecordMetric(operation, stopwatch.Elapsed, itemCount, memoryDelta);
        }
    }
}

public class PerformanceMetric
{
    public string Operation { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public int ItemCount { get; set; }
    public long MemoryDelta { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PerformanceReport
{
    public int TotalMetrics { get; set; }
    public double TotalDuration { get; set; }
    public double AverageDuration { get; set; }
    public long TotalMemoryDelta { get; set; }
    public Dictionary<string, OperationStats> OperationBreakdown { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();

    public void PrintReport(Action<string> output)
    {
        output("=== CONSTRAINT CHECKING PERFORMANCE REPORT ===");
        output("");
        output($"Total Metrics: {TotalMetrics}");
        output($"Total Duration: {TotalDuration:F2}ms");
        output($"Average Duration: {AverageDuration:F4}ms");
        output($"Total Memory Delta: {TotalMemoryDelta:N0} bytes");
        output("");
        
        if (OperationBreakdown.Any())
        {
            output("OPERATION BREAKDOWN:");
            output("?????????????????????????????????????????????");
            
            foreach (var (operation, stats) in OperationBreakdown.OrderByDescending(kvp => kvp.Value.TotalDuration))
            {
                output($"? {operation}:");
                output($"  Count: {stats.Count}");
                output($"  Total: {stats.TotalDuration:F2}ms");
                output($"  Average: {stats.AverageDuration:F4}ms per operation");
                output($"  Range: {stats.MinDuration:F4}ms - {stats.MaxDuration:F4}ms");
                if (stats.TotalItems > 0)
                {
                    output($"  Throughput: {stats.TotalItems} items, {stats.AverageDuration / Math.Max(stats.TotalItems / stats.Count, 1):F6}ms per item");
                }
                if (stats.TotalMemory != 0)
                {
                    output($"  Memory: {stats.TotalMemory:N0} bytes total");
                }
                output("");
            }
        }
        
        if (Recommendations.Any())
        {
            output("RECOMMENDATIONS:");
            output("?????????????????");
            for (int i = 0; i < Recommendations.Count; i++)
            {
                output($"{i + 1}. {Recommendations[i]}");
            }
        }
        
        output("===============================================");
    }
}

public class OperationStats
{
    public int Count { get; set; }
    public double TotalDuration { get; set; }
    public double AverageDuration { get; set; }
    public double MinDuration { get; set; }
    public double MaxDuration { get; set; }
    public int TotalItems { get; set; }
    public long TotalMemory { get; set; }
}