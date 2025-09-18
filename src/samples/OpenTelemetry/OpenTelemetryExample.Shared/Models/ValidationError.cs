using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a validation error for a specific property.
/// </summary>
public sealed class ValidationError
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}