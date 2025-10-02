using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Notifications;

public class UserPasswordResetEvent : IDomainEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime ResetAt { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "UserPasswordReset";
    public int Version => 1;
}