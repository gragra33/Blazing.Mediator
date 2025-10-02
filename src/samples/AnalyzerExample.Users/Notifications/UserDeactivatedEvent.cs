using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Notifications;

public class UserDeactivatedEvent : IDomainEvent
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? ReactivationDate { get; set; }
    public string DeactivatedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "UserDeactivated";
    public int Version => 1;
}