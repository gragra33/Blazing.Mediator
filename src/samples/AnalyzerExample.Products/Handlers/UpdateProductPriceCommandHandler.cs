using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for updating product prices
/// </summary>
public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate price update
        await Task.Delay(35, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}