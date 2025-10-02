using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for creating new users
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, OperationResult<int>>
{
    public async Task<OperationResult<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate user creation
        await Task.Delay(90, cancellationToken);
        
        // Return success with new user ID
        var userId = Random.Shared.Next(1000, 9999);
        return OperationResult<int>.Success(userId);
    }
}