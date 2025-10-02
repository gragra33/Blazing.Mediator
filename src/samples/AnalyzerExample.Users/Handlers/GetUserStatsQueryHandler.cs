using AnalyzerExample.Users.Domain;
using AnalyzerExample.Users.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for retrieving user statistics
/// </summary>
public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    public async Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(65, cancellationToken);
        
        return new UserStatsDto
        {
            UserId = 1,
            FullName = "John Doe",
            Email = "john.doe@example.com",
            TotalOrders = 15,
            TotalSpent = 1250.75m,
            TotalLogins = 120,
            LastLoginAt = DateTime.UtcNow.AddHours(-2),
            CreatedAt = DateTime.UtcNow.AddDays(-365),
            DaysSinceRegistration = 365,
            DaysSinceLastLogin = 0,
            ActiveRoles = new List<string> { "User", "Premium" }
        };
    }
}