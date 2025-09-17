using Blazing.Mediator;
using OpenTelemetryExample.Exceptions;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for DeleteUserCommand.
/// </summary>
public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(25, 150), cancellationToken);
        
        // Simulate user not found
        if (request.UserId > 1000)
        {
            throw new NotFoundException($"User with ID {request.UserId} not found");
        }
        
        // In a real application, this would delete from the database
        Console.WriteLine($"Deleted user {request.UserId}");
    }
}