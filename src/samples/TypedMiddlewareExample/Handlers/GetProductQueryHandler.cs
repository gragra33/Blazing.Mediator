using TypedMiddlewareExample.Queries;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for retrieving product information.
/// </summary>
public class GetProductQueryHandler : IQueryHandler<GetProductQuery, string>
{
    private readonly ILogger<GetProductQueryHandler> _logger;
    private static readonly Dictionary<string, object> _products = new()
    {
        { "WIDGET-001", new { Name = "High Quality Widget", Price = 99.99m, InStock = 25 } }
    };

    public GetProductQueryHandler(ILogger<GetProductQueryHandler> logger)
    {
        _logger = logger;
    }

    public async Task<string> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug(".. Retrieving product information for: {ProductId}", request.ProductId);

        // Simulate database lookup
        await Task.Delay(5, cancellationToken);

        if (_products.TryGetValue(request.ProductId, out var product))
        {
            var productInfo = $"-- Product: {request.ProductId} - {((dynamic)product).Name}, Price: ${((dynamic)product).Price}, In Stock: {((dynamic)product).InStock} units";
            return productInfo;
        }

        return $"-- Product {request.ProductId} not found";
    }
}