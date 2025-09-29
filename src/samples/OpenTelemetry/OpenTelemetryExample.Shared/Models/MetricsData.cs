using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents system metrics data.
/// </summary>
public sealed class MetricsData
{
    /// <summary>
    /// Gets or sets the total number of requests.
    /// </summary>
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; set; }

    /// <summary>
    /// Gets or sets the average response time in milliseconds.
    /// </summary>
    [JsonPropertyName("averageResponseTime")]
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Gets or sets the error rate as a percentage.
    /// </summary>
    [JsonPropertyName("errorRate")]
    public double ErrorRate { get; set; }

    /// <summary>
    /// Gets or sets the number of active connections.
    /// </summary>
    [JsonPropertyName("activeConnections")]
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Gets or sets the memory usage in megabytes.
    /// </summary>
    [JsonPropertyName("memoryUsage")]
    public double MemoryUsage { get; set; }

    /// <summary>
    /// Gets or sets the CPU usage as a percentage.
    /// </summary>
    [JsonPropertyName("cpuUsage")]
    public double CpuUsage { get; set; }
}