using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for updating order addresses
/// </summary>
public class UpdateOrderAddressCommandHandler : IRequestHandler<UpdateOrderAddressCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateOrderAddressCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate address update
        await Task.Delay(45, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}