using TypedMiddlewareExample.Queries;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for getting product information.
/// Handles custom IProductRequest interface.
/// </summary>
public class GetProductQueryHandler(ILogger<GetProductQueryHandler> logger) : IRequestHandler<GetProductQuery, string>
{
    public Task<string> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug(".. Retrieving product information for: {ProductId}", request.ProductId);

        // Simulate product lookup
        var product = $"-- Product: {request.ProductId} - High Quality Widget, Price: $99.99, In Stock: 25 units";
        
        return Task.FromResult(product);
    }
}