using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Mappings;
using UserManagement.Api.Application.Queries;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Queries;

public class GetUsersHandler(UserManagementDbContext context) : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        var query = context.Users.AsNoTracking();

        // Apply filters
        if (!request.IncludeInactive)
            query = query.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return users.ToPagedDto(totalCount, request.Page, request.PageSize);
    }
}