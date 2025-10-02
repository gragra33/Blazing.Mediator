using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Commands;

public class BulkUserUpdateOptions
{
    public UserStatus? Status { get; set; }
    public List<string> RolesToAdd { get; set; } = new();
    public List<string> RolesToRemove { get; set; } = new();
    public UserPreferences? Preferences { get; set; }
    public Dictionary<string, object> CustomFields { get; set; } = new();
}