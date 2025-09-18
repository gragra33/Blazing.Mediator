using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents an API error response with validation details.
/// </summary>
public sealed class ApiError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ValidationError> Errors { get; set; } = new();
}