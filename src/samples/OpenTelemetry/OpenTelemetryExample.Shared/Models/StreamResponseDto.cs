using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Generic streaming response wrapper that includes data and metadata.
/// </summary>
/// <typeparam name="T">The type of data being streamed.</typeparam>
public sealed class StreamResponseDto<T>
{
    /// <summary>
    /// The data payload being streamed.
    /// </summary>
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    /// <summary>
    /// Metadata about the streaming operation, such as item number, total estimated, timestamp, processing time, and batch information.
    /// </summary>
    [JsonPropertyName("metadata")]
    public StreamMetadataDto Metadata { get; set; } = new();
}