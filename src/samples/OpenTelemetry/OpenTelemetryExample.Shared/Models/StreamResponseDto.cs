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