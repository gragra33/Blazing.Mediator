using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Notifications;

public class UserRoleAssignedEvent : IDomainEvent
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "UserRoleAssigned";
    public int Version => 1;
}