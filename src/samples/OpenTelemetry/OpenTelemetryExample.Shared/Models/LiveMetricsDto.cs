using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents live telemetry metrics data for the dashboard.
/// </summary>
public sealed class LiveMetricsDto
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("metrics")]
    public MetricsData Metrics { get; set; } = new();

    [JsonPropertyName("commands")]
    public List<CommandPerformanceDto> Commands { get; set; } = new();

    [JsonPropertyName("queries")]
    public List<QueryPerformanceDto> Queries { get; set; } = new();

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents system metrics data.
/// </summary>
public sealed class MetricsData
{
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; set; }

    [JsonPropertyName("averageResponseTime")]
    public double AverageResponseTime { get; set; }

    [JsonPropertyName("errorRate")]
    public double ErrorRate { get; set; }

    [JsonPropertyName("activeConnections")]
    public int ActiveConnections { get; set; }

    [JsonPropertyName("memoryUsage")]
    public double MemoryUsage { get; set; }

    [JsonPropertyName("cpuUsage")]
    public double CpuUsage { get; set; }
}

/// <summary>
/// Represents command performance metrics.
/// </summary>
public sealed class CommandPerformanceDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("avgDuration")]
    public double AvgDuration { get; set; }
}

/// <summary>
/// Represents query performance metrics.
/// </summary>
public sealed class QueryPerformanceDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("avgDuration")]
    public double AvgDuration { get; set; }
}