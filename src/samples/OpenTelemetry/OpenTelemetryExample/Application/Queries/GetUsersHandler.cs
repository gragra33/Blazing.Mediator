using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUsersQuery using Entity Framework Core.
/// </summary>
public sealed class GetUsersHandler(ApplicationDbContext context) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);
        
        var query = context.Users.AsQueryable();
        
        if (!request.IncludeInactive)
        {
            query = query.Where(u => u.IsActive);
        }
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(request.SearchTerm)
                                     || u.Email.Contains(request.SearchTerm));
        }
        
        var users = await query.ToListAsync(cancellationToken);
        return users.Select(u => u.ToDto()).ToList();
    }
}