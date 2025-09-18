using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetLiveMetricsQuery that retrieves real telemetry data from the database.
/// </summary>
public sealed class GetLiveMetricsHandler(ApplicationDbContext context, ILogger<GetLiveMetricsHandler> logger)
    : IRequestHandler<GetLiveMetricsQuery, LiveMetricsDto>
{
    public async Task<LiveMetricsDto> Handle(GetLiveMetricsQuery request, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow - request.TimeWindow;

        try
        {
            // Get recent metrics from the database
            var recentMetrics = await context.TelemetryMetrics
                .Where(m => m.Timestamp >= cutoffTime)
                .OrderByDescending(m => m.Timestamp)
                .Take(request.MaxRecords)
                .ToListAsync(cancellationToken);

            if (!recentMetrics.Any())
            {
                logger.LogInformation("No telemetry metrics found in the last {TimeWindow} minutes", request.TimeWindow.TotalMinutes);
                return new LiveMetricsDto
                {
                    Timestamp = DateTime.UtcNow,
                    Metrics = new MetricsData(),
                    Commands = [],
                    Queries = [],
                    Message = "No telemetry data available yet. Try making some requests to the API first."
                };
            }

            // Calculate aggregated metrics
            var totalRequests = recentMetrics.Count;
            var successfulRequests = recentMetrics.Count(m => m.IsSuccess);
            var averageResponseTime = recentMetrics.Average(m => m.Duration);
            var errorRate = totalRequests > 0 ? (double)(totalRequests - successfulRequests) / totalRequests * 100 : 0;

            // Simulate some system metrics (in a real system, these would come from system monitoring)
            var activeConnections = Math.Max(1, totalRequests / 10);
            var memoryUsage = 50 + (totalRequests % 30); // Simulate memory usage between 50-80%
            var cpuUsage = 20 + (totalRequests % 40); // Simulate CPU usage between 20-60%

            // Group commands and queries
            var commandMetrics = recentMetrics
                .Where(m => m.Category == "Command")
                .GroupBy(m => m.RequestName)
                .Select(g => new CommandPerformanceDto
                {
                    Name = g.Key,
                    Count = g.Count(),
                    AvgDuration = Math.Round(g.Average(m => m.Duration), 1)
                })
                .OrderByDescending(c => c.Count)
                .Take(10)
                .ToList();

            var queryMetrics = recentMetrics
                .Where(m => m.Category == "Query")
                .GroupBy(m => m.RequestName)
                .Select(g => new QueryPerformanceDto
                {
                    Name = g.Key,
                    Count = g.Count(),
                    AvgDuration = Math.Round(g.Average(m => m.Duration), 1)
                })
                .OrderByDescending(q => q.Count)
                .Take(10)
                .ToList();

            var result = new LiveMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Metrics = new MetricsData
                {
                    RequestCount = totalRequests,
                    AverageResponseTime = Math.Round(averageResponseTime, 1),
                    ErrorRate = Math.Round(errorRate, 1),
                    ActiveConnections = activeConnections,
                    MemoryUsage = memoryUsage,
                    CpuUsage = cpuUsage
                },
                Commands = commandMetrics,
                Queries = queryMetrics,
                Message = $"Live metrics from the last {request.TimeWindow.TotalMinutes:F0} minutes - {totalRequests} total requests, {successfulRequests} successful"
            };

            logger.LogInformation("Retrieved live metrics: {TotalRequests} requests, {SuccessRate:F1}% success rate", 
                totalRequests, successfulRequests * 100.0 / totalRequests);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving live metrics");
            
            // Return fallback data in case of error
            return new LiveMetricsDto
            {
                Timestamp = DateTime.UtcNow,
                Metrics = new MetricsData(),
                Commands = [],
                Queries = [],
                Message = $"Error retrieving live metrics: {ex.Message}"
            };
        }
    }
}