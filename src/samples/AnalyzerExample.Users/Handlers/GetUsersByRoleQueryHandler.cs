using AnalyzerExample.Users.Domain;
using AnalyzerExample.Users.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for retrieving users by role
/// </summary>
public class GetUsersByRoleQueryHandler : IRequestHandler<GetUsersByRoleQuery, List<UserSummaryDto>>
{
    public async Task<List<UserSummaryDto>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(45, cancellationToken);
        
        return new List<UserSummaryDto>
        {
            new UserSummaryDto
            {
                Id = 1,
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Status = UserStatus.Active
            }
        };
    }
}