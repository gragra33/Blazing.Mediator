using Blazing.Mediator;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for CreateUserCommand using Entity Framework Core.
/// </summary>
public sealed class CreateUserHandler(ApplicationDbContext context)
    : IRequestHandler<CreateUserCommand, int>
{
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
        
        // Simulate potential failures for testing
        if (request.Name.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Simulated error for testing telemetry");
        }
        
        // Create new user entity
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        // Add to database
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        
        Console.WriteLine($"Created user: {user.Name} ({user.Email}) with ID {user.Id}");
        
        return user.Id;
    }
}