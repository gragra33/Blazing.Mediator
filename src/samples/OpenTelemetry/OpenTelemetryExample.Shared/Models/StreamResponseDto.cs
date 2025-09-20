using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Generic streaming response wrapper that includes data and metadata.
/// </summary>
/// <typeparam name="T">The type of data being streamed.</typeparam>
public sealed class StreamResponseDto<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    [JsonPropertyName("metadata")]
    public StreamMetadataDto Metadata { get; set; } = new();
}

/// <summary>
/// Metadata about the streaming operation.
/// </summary>
public sealed class StreamMetadataDto
{
    [JsonPropertyName("itemNumber")]
    public int ItemNumber { get; set; }

    [JsonPropertyName("totalEstimated")]
    public int TotalEstimated { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("processingTimeMs")]
    public double ProcessingTimeMs { get; set; }

    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    [JsonPropertyName("batchId")]
    public string BatchId { get; set; } = string.Empty;
}