using AnalyzerExample.Products.Queries;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Products.Middleware;

public class ProductQueryCacheMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IProductQuery<TResponse>
{
    public static int Order => 50;
    
    private readonly ILogger<ProductQueryCacheMiddleware<TRequest, TResponse>> _logger;

    public ProductQueryCacheMiddleware(ILogger<ProductQueryCacheMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var queryType = typeof(TRequest).Name;
        var cacheKey = $"product_query_{queryType}_{request.GetHashCode()}";
        
        _logger.LogInformation("??? [Products] Checking cache for query: {QueryType}, Key: {CacheKey}", queryType, cacheKey);
        
        // Simulate cache lookup
        await Task.Delay(5, cancellationToken);
        
        // For demo purposes, simulate cache miss most of the time
        bool cacheHit = Random.Shared.Next(1, 100) <= 20; // 20% cache hit rate
        
        if (cacheHit)
        {
            _logger.LogInformation("?? [Products] Cache HIT for {QueryType}", queryType);
            // In a real scenario, we would return cached data
            // For demo, we'll still call next() but log the cache hit
        }
        else
        {
            _logger.LogInformation("?? [Products] Cache MISS for {QueryType}", queryType);
        }
        
        var result = await next();
        
        if (!cacheHit)
        {
            _logger.LogInformation("?? [Products] Caching result for {QueryType}", queryType);
            // Simulate storing in cache
            await Task.Delay(5, cancellationToken);
        }
        
        return result;
    }
}