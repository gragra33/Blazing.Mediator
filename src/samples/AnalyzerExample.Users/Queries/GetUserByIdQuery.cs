using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

/// <summary>
/// User queries demonstrating various patterns
/// </summary>
public class GetUserByIdQuery : IUserQuery<UserDetailDto?>
{
    public int UserId { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public bool IncludeProfile { get; set; } = true;
    public bool IncludeRoles { get; set; } = true;
    public bool IncludeAddresses { get; set; } = true;
}