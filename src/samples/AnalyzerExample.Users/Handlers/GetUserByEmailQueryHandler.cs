using AnalyzerExample.Users.Domain;
using AnalyzerExample.Users.Queries;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for retrieving user by email
/// </summary>
public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserDetailDto?>
{
    public async Task<UserDetailDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken = default)
    {
        // Simulate database query
        await Task.Delay(40, cancellationToken);
        
        if (request.Email == "john.doe@example.com")
        {
            return new UserDetailDto
            {
                Id = 1,
                Email = request.Email,
                FirstName = "John",
                LastName = "Doe",
                Status = UserStatus.Active,
                Profile = new UserProfileDto { Bio = "Sample user" },
                Roles = new List<UserRoleDto>
                {
                    new UserRoleDto { Id = 1, RoleName = "User", IsActive = true, AssignedAt = DateTime.UtcNow.AddDays(-30), AssignedBy = "Admin" }
                },
                Addresses = new List<UserAddressDto>()
            };
        }
        
        return null;
    }
}