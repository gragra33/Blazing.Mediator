using AnalyzerExample.Products.Commands;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Middleware;

public class ProductInventoryMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public static int Order => 125;
    
    private readonly ILogger<ProductInventoryMiddleware<TRequest>> _logger;

    public ProductInventoryMiddleware(ILogger<ProductInventoryMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Only process stock-related commands
        if (request is UpdateProductStockCommand stockCommand)
        {
            _logger.LogInformation("?? [Products] Processing inventory change: ProductId: {ProductId}, Change: {QuantityChange}", 
                stockCommand.ProductId, stockCommand.QuantityChange);
            
            // Simulate inventory validation
            await Task.Delay(15, cancellationToken);
            
            if (stockCommand.QuantityChange < -1000)
            {
                _logger.LogWarning("?? [Products] Large stock decrease detected: {QuantityChange} for ProductId: {ProductId}", 
                    stockCommand.QuantityChange, stockCommand.ProductId);
            }
        }
        
        await next();
    }
}