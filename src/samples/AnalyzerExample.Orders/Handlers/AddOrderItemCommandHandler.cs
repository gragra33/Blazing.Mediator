using AnalyzerExample.Common.Domain;
using AnalyzerExample.Orders.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for adding items to orders
/// </summary>
public class AddOrderItemCommandHandler : IRequestHandler<AddOrderItemCommand, OperationResult>
{
    public async Task<OperationResult> Handle(AddOrderItemCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate adding order item
        await Task.Delay(40, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}