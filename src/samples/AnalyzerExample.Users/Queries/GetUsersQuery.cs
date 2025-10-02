using AnalyzerExample.Common.Domain;
using AnalyzerExample.Common.Interfaces;
using AnalyzerExample.Users.Domain;

namespace AnalyzerExample.Users.Queries;

public class GetUsersQuery : IUserQuery<PagedResult<UserSummaryDto>>, IPaginatedQuery<PagedResult<UserSummaryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public UserStatus? StatusFilter { get; set; }
    public List<string> RoleFilters { get; set; } = new();
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? LastLoginAfter { get; set; }
    public UserSortBy SortBy { get; set; } = UserSortBy.CreatedDate;
    public bool SortDescending { get; set; } = true;
    public bool IncludeInactive { get; set; } = false;
}