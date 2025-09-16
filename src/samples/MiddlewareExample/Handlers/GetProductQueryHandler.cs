namespace MiddlewareExample.Handlers;

/// <summary>
/// Handles <see cref="GetProductQuery"/> requests and returns product information.
/// </summary>
public class GetProductQueryHandler(ILogger<GetProductQueryHandler> logger) : IRequestHandler<GetProductQuery, string>
{
    /// <inheritdoc />
    public Task<string> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug(".. Retrieving product information for: {ProductId}", request.ProductId);

        // Simulate product lookup
        var productInfo = $"-- Product: {request.ProductId} - High Quality Widget, Price: $99.99, In Stock: 25 units";

        return Task.FromResult(productInfo);
    }
}
