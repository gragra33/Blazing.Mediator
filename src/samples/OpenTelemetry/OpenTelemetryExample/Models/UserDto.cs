namespace OpenTelemetryExample.Shared.DTOs;

/// <summary>
/// User data transfer object shared between client and server.
/// </summary>
public sealed class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}