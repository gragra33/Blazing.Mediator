using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

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