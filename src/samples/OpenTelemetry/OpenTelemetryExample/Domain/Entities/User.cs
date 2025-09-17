using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// User entity for the in-memory database.
/// </summary>
public sealed class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Converts the User entity to a UserDto.
    /// </summary>
    public UserDto ToDto()
    {
        return new UserDto
        {
            Id = Id,
            Name = Name,
            Email = Email,
            CreatedAt = CreatedAt,
            IsActive = IsActive
        };
    }

    /// <summary>
    /// Creates a User entity from a UserDto.
    /// </summary>
    public static User FromDto(UserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = dto.CreatedAt,
            IsActive = dto.IsActive
        };
    }

    /// <summary>
    /// Updates the User entity from a UserDto.
    /// </summary>
    public void UpdateFromDto(UserDto dto)
    {
        Name = dto.Name;
        Email = dto.Email;
        IsActive = dto.IsActive;
        // Don't update Id and CreatedAt
    }
}