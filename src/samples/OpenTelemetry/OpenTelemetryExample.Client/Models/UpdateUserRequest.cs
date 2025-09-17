namespace OpenTelemetryExample.Client.Models;

public sealed class UpdateUserRequest
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}