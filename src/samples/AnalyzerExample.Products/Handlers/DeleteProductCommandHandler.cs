using AnalyzerExample.Products.Commands;
using Blazing.Mediator;

namespace AnalyzerExample.Products.Handlers;

/// <summary>
/// Handler for deleting products
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken = default)
    {
        // Simulate product deletion
        await Task.Delay(40, cancellationToken);
        
        // Product deleted successfully
    }
}