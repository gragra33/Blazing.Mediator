using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Commands;

public class UpdateUserPreferencesCommand : IUserCommand<OperationResult>
{
    public int UserId { get; set; }
    public UserPreferences Preferences { get; set; } = new();
}