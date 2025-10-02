using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Notifications;

/// <summary>
/// User domain events and notifications
/// </summary>
public class UserCreatedEvent : IDomainEvent
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType => "UserCreated";
    public int Version => 1;
}