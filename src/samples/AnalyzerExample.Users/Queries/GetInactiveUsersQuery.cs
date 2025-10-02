using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

public class GetInactiveUsersQuery : IUserQuery<List<UserSummaryDto>>
{
    public int DaysInactive { get; set; } = 30;
    public bool IncludeInactive { get; set; } = true;
}