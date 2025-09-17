using Blazing.Mediator;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for CreateUserCommand.
/// </summary>
public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, int>
{
    private static int _nextId = 6; // Start after the seed data
    
    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(100, 500), cancellationToken);
        
        // Simulate potential failures for testing
        if (request.Name.Contains("error", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Simulated error for testing telemetry");
        }
        
        var userId = _nextId++;
        
        // In a real application, this would save to a database
        Console.WriteLine($"Created user: {request.Name} ({request.Email}) with ID {userId}");
        
        return userId;
    }
}