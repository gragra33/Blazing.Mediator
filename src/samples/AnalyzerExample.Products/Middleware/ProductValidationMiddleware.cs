using AnalyzerExample.Products.Commands;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Middleware;

/// <summary>
/// Product-specific middleware
/// </summary>
public class ProductValidationMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IProductCommand
{
    public static int Order => 150;
    
    private readonly ILogger<ProductValidationMiddleware<TRequest>> _logger;

    public ProductValidationMiddleware(ILogger<ProductValidationMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("? [Products] Validating product command: {CommandType} for ProductId: {ProductId}", 
            typeof(TRequest).Name, request.ProductId);
        
        // Simulate product existence validation
        if (request.ProductId <= 0)
        {
            _logger.LogError("? [Products] Invalid ProductId: {ProductId}", request.ProductId);
            throw new ArgumentException($"ProductId must be greater than 0, got: {request.ProductId}");
        }
        
        // Simulate database check
        await Task.Delay(10, cancellationToken);
        
        if (request.ProductId > 10000)
        {
            _logger.LogError("? [Products] Product not found: {ProductId}", request.ProductId);
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");
        }
        
        _logger.LogInformation("? [Products] Product validation passed for ID: {ProductId}", request.ProductId);
        await next();
    }
}