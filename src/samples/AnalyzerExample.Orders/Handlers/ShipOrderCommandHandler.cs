using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for shipping orders
/// </summary>
public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, OperationResult>
{
    public async Task<OperationResult> Handle(ShipOrderCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate order shipping
        await Task.Delay(60, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}