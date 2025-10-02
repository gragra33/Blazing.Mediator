using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Domain;
using AnalyzerExample.Users.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for searching users with various criteria
/// </summary>
public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PagedResult<UserSummaryDto>>
{
    public async Task<PagedResult<UserSummaryDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database search
        await Task.Delay(70, cancellationToken);
        
        var users = new List<UserSummaryDto>
        {
            new UserSummaryDto
            {
                Id = 1,
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Status = UserStatus.Active
            }
        };
        
        return new PagedResult<UserSummaryDto>
        {
            Items = users,
            TotalCount = users.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}