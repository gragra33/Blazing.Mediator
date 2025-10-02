using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for deactivating users
/// </summary>
public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate user deactivation
        await Task.Delay(50, cancellationToken);
        
        // User deactivated successfully
    }
}