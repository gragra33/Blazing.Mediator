using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Users.Domain;

public class UserProfile : BaseEntity
{
    public int UserId { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? TwitterHandle { get; set; }
    public string? LinkedInProfile { get; set; }
    public UserPreferences Preferences { get; set; } = new();
}