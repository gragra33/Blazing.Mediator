using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for DeleteUserCommand using Entity Framework Core.
/// </summary>
public sealed class DeleteUserHandler(ApplicationDbContext context)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(25, 150), cancellationToken);
        
        // Find the user to delete
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }
        
        // Remove the user from database
        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        
        Console.WriteLine($"Deleted user {user.Id}: {user.Name} ({user.Email})");
    }
}