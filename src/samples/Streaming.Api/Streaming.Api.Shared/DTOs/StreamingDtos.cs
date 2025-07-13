namespace Streaming.Api.Shared.DTOs;

/// <summary>
/// Statistics about the streaming operation
/// </summary>
public class StreamStatistics
{
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public double ItemsPerSecond => ProcessedItems / Math.Max(ElapsedTime.TotalSeconds, 0.001);
}

/// <summary>
/// Streaming response wrapper with metadata
/// </summary>
public class StreamResponse<T>
{
    public T Data { get; set; } = default(T)!;
    public StreamStatistics? Statistics { get; set; }
    public bool IsComplete { get; set; }
    public string? Message { get; set; }
}
