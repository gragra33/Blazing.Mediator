using System.Text.Json.Serialization;

namespace OpenTelemetryExample.Shared.Models;

/// <summary>
/// Request model for updating an existing user.
/// </summary>
public sealed class UpdateUserRequest
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}