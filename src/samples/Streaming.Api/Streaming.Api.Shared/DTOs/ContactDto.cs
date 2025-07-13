namespace Streaming.Api.Shared.DTOs;

/// <summary>
/// Simplified contact DTO for streaming scenarios
/// </summary>
public class ContactDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
