using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for updating order status
/// </summary>
public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate status update
        await Task.Delay(50, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}