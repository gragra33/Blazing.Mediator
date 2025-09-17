using Blazing.Mediator;
using OpenTelemetryExample.Application.Commands;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Shared.DTOs;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUserQuery.
/// </summary>
public sealed class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private static readonly List<UserDto> _users = new()
    {
        new UserDto { Id = 1, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow.AddDays(-30), IsActive = true },
        new UserDto { Id = 2, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow.AddDays(-15), IsActive = true },
        new UserDto { Id = 3, Name = "Bob Johnson", Email = "bob@example.com", CreatedAt = DateTime.UtcNow.AddDays(-7), IsActive = false }
    };

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(10, 100), cancellationToken);
        
        var user = _users.FirstOrDefault(u => u.Id == request.UserId);
        return user ?? throw new NotFoundException($"User with ID {request.UserId} not found");
    }
}