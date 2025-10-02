using Blazing.Mediator;

namespace AnalyzerExample.Users.Notifications;

public class UserLoginEvent : INotification
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
}