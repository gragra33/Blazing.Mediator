using Blazing.Mediator;
using OpenTelemetryExample.Shared.DTOs;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUsersQuery.
/// </summary>
public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private static readonly List<UserDto> _users = new()
    {
        new UserDto { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-30), IsActive = true },
        new UserDto { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-15), IsActive = true },
        new UserDto { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7), IsActive = false },
        new UserDto { Id = 4, Name = "Alice Brown", Email = "alice@example.com", CreatedAt = DateTime.UtcNow.AddDays(-2), IsActive = true },
        new UserDto { Id = 5, Name = "Charlie Wilson", Email = "charlie@example.com", CreatedAt = DateTime.UtcNow.AddDays(-1), IsActive = true }
    };

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);
        
        var query = _users.AsQueryable();
        
        if (!request.IncludeInactive)
        {
            query = query.Where(u => u.IsActive);
        }
        
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                     u.Email.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }
        
        return query.ToList();
    }
}