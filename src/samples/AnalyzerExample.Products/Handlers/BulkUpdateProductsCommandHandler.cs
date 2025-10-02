using AnalyzerExample.Common.Domain;
using AnalyzerExample.Products.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for bulk updating products
/// </summary>
public class BulkUpdateProductsCommandHandler : IRequestHandler<BulkUpdateProductsCommand, OperationResult<int>>
{
    public async Task<OperationResult<int>> Handle(BulkUpdateProductsCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate bulk update
        await Task.Delay(200, cancellationToken);
        
        // Return success with number of products updated
        return OperationResult<int>.Success(request.ProductIds.Count);
    }
}