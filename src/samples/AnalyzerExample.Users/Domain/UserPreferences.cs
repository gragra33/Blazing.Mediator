namespace AnalyzerExample.Users.Domain;

public class UserPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool MarketingEmails { get; set; } = true;
    public string Timezone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
    public string Currency { get; set; } = "USD";
}