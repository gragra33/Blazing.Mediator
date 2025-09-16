namespace MiddlewareExample.Middleware;

/// <summary>
/// Middleware for caching and performance monitoring of product queries.
/// </summary>
public class ProductQueryCacheMiddleware(ILogger<ProductQueryCacheMiddleware> logger)
    : IRequestMiddleware<GetProductQuery, string>
{
    /// <inheritdoc />
    public int Order => 20;

    /// <inheritdoc />
    public async Task<string> HandleAsync(GetProductQuery request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
    {
        logger.LogDebug(">> Checking cache for product: {ProductId}", request.ProductId);

        // Simulate cache check
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        logger.LogDebug("<< Product query completed in {ElapsedMs}ms for: {ProductId}",
            stopwatch.ElapsedMilliseconds, request.ProductId);

        // Simulate cache storage
        logger.LogDebug("-- Caching product data for: {ProductId}", request.ProductId);

        return response;
    }
}
