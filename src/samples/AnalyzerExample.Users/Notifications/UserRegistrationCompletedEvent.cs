using AnalyzerExample.Common.Interfaces;

namespace AnalyzerExample.Users.Notifications;

public class UserRegistrationCompletedEvent : IIntegrationEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Source => "UserService";
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> InitialRoles { get; set; } = new();
    public DateTime RegistrationDate { get; set; }
}