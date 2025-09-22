using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents an API error response with validation details.
/// </summary>
public sealed class ApiError
{
    /// <summary>
    /// Gets or sets the main error message describing the API error.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of validation errors associated with the API error.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<ValidationError> Errors { get; set; } = new();
}