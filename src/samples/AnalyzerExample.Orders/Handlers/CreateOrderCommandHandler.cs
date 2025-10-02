using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for creating new orders
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OperationResult<int>>
{
    public async Task<OperationResult<int>> Handle(CreateOrderCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate order creation
        await Task.Delay(100, cancellationToken);
        
        // Return success with new order ID
        var orderId = Random.Shared.Next(1000, 9999);
        return OperationResult<int>.Success(orderId);
    }
}