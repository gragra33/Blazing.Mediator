using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Represents a validation error for a specific property.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Gets or sets the name of the property that has a validation error.
    /// </summary>
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message associated with the property.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}