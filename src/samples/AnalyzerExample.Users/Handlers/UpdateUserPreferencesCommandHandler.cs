using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for updating user preferences
/// </summary>
public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate preferences update
        await Task.Delay(35, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}