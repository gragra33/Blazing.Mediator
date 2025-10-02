using AnalyzerExample.Products.Commands;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Middleware;

public class ProductOperationMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IProductCommand<TResponse>
{
    public static int Order => 75;
    
    private readonly ILogger<ProductOperationMiddleware<TRequest, TResponse>> _logger;

    public ProductOperationMiddleware(ILogger<ProductOperationMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var commandType = typeof(TRequest).Name;
        _logger.LogInformation("?? [Products] Starting product operation: {CommandType} for ProductId: {ProductId}", 
            commandType, request.ProductId);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await next();
            stopwatch.Stop();
            
            _logger.LogInformation("? [Products] Product operation completed: {CommandType} for ProductId: {ProductId} in {ElapsedMs}ms", 
                commandType, request.ProductId, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "? [Products] Product operation failed: {CommandType} for ProductId: {ProductId} after {ElapsedMs}ms", 
                commandType, request.ProductId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}