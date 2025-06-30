using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Mappings;
using UserManagement.Api.Application.Queries;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Queries;

public class GetActiveUsersHandler(UserManagementDbContext context)
    : IRequestHandler<GetActiveUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken = default)
    {
        List<User>? users = await context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return users.ToDto();
    }
}