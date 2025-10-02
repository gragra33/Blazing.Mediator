using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for assigning user roles
/// </summary>
public class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand, OperationResult>
{
    public async Task<OperationResult> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate role assignment
        await Task.Delay(40, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}