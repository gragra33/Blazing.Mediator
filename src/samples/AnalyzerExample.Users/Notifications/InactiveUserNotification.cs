using Blazing.Mediator;

namespace AnalyzerExample.Users.Notifications;

public class InactiveUserNotification : INotification
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime LastLoginAt { get; set; }
    public int DaysInactive { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}