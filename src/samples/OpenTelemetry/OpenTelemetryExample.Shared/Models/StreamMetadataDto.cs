using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Metadata about the streaming operation.
/// </summary>
public sealed class StreamMetadataDto
{
    /// <summary>
    /// Gets or sets the item number in the current stream batch.
    /// </summary>
    [JsonPropertyName("itemNumber")]
    public int ItemNumber { get; set; }

    /// <summary>
    /// Gets or sets the total estimated number of items in the stream.
    /// </summary>
    [JsonPropertyName("totalEstimated")]
    public int TotalEstimated { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the metadata was generated.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the processing time in milliseconds for the current item or batch.
    /// </summary>
    [JsonPropertyName("processingTimeMs")]
    public double ProcessingTimeMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is the last item in the stream.
    /// </summary>
    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the current batch.
    /// </summary>
    [JsonPropertyName("batchId")]
    public string BatchId { get; set; } = string.Empty;
}