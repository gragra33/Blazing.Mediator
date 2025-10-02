using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

public class SearchUsersQuery : IUserQuery<PagedResult<UserSummaryDto>>, IPaginatedQuery<PagedResult<UserSummaryDto>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public List<UserStatus> StatusFilters { get; set; } = new();
    public List<string> RoleFilters { get; set; } = new();
    public bool IncludeInactive { get; set; } = false;
}