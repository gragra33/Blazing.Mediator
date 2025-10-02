using AnalyzerExample.Common.Domain;

namespace AnalyzerExample.Users.Domain;

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}