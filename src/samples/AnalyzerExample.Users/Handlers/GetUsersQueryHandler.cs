using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Domain;
using AnalyzerExample.Users.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for retrieving all users with pagination
/// </summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    public async Task<PagedResult<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(50, cancellationToken);
        
        var users = new List<UserSummaryDto>
        {
            new UserSummaryDto
            {
                Id = 1,
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Status = UserStatus.Active
            },
            new UserSummaryDto
            {
                Id = 2,
                Email = "jane.smith@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Status = UserStatus.Active
            }
        };
        
        return new PagedResult<UserSummaryDto>
        {
            Items = users.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList(),
            TotalCount = users.Count,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}