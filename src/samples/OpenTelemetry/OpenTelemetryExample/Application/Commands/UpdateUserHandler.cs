using Blazing.Mediator;
using OpenTelemetryExample.Exceptions;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for UpdateUserCommand.
/// </summary>
public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 300), cancellationToken);
        
        // Simulate user not found
        if (request.UserId > 1000)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }
        
        // In a real application, this would update the database
        Console.WriteLine($"Updated user {request.UserId}: {request.Name} ({request.Email})");
    }
}