using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

public class GetUsersByRoleQuery : IUserQuery<List<UserSummaryDto>>
{
    public string RoleName { get; set; } = string.Empty;
    public bool ActiveOnly { get; set; } = true;
    public bool IncludeInactive { get; set; } = false;
}