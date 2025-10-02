using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for updating product information
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate product update
        await Task.Delay(60, cancellationToken);
        
        // Return success
        return OperationResult.Success();
    }
}