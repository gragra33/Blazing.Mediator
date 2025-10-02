using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for updating user information
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate user update
        await Task.Delay(60, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}