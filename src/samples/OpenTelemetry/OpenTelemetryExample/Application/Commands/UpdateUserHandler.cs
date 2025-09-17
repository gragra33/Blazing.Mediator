using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for UpdateUserCommand using Entity Framework Core.
/// </summary>
public sealed class UpdateUserHandler(ApplicationDbContext context)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 300), cancellationToken);
        
        // Find the user to update
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }
        
        // Update the user properties
        user.Name = request.Name;
        user.Email = request.Email;
        
        // Save changes to database
        await context.SaveChangesAsync(cancellationToken);
        
        Console.WriteLine($"Updated user {user.Id}: {user.Name} ({user.Email})");
    }
}