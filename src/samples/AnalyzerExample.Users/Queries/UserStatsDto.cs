namespace AnalyzerExample.Users.Queries;

public class UserStatsDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalLogins { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DaysSinceRegistration { get; set; }
    public int DaysSinceLastLogin { get; set; }
    public List<string> ActiveRoles { get; set; } = new();
}