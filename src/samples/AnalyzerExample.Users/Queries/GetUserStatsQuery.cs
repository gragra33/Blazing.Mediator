namespace AnalyzerExample.Users.Queries;

public class GetUserStatsQuery : IUserQuery<UserStatsDto>
{
    public int UserId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}