using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Notifications;

public class UserEmailVerifiedEvent : IDomainEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime VerifiedAt { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "UserEmailVerified";
    public int Version => 1;
}