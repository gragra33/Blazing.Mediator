using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Domain.Entities;

/// <summary>
/// User entity for the in-memory database.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// </summary>
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