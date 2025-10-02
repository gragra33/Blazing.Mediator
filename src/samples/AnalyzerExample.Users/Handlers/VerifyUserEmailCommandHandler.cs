using AnalyzerExample.Common.Domain;
using AnalyzerExample.Users.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for verifying user email addresses
/// </summary>
public class VerifyUserEmailCommandHandler : IRequestHandler<VerifyUserEmailCommand, OperationResult>
{
    public async Task<OperationResult> Handle(VerifyUserEmailCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate email verification
        await Task.Delay(30, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}