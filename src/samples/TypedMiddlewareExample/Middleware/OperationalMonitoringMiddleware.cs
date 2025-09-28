namespace TypedMiddlewareExample.Middleware;

/// <summary>
/// Middleware for monitoring operational performance across all requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
public class BasicOperationalMonitoringMiddleware<TRequest>(ILogger<BasicOperationalMonitoringMiddleware<TRequest>> logger)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public int Order => 40; // Execute after business operation audit

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestType = typeof(TRequest).Name;

        logger.LogDebug(">> Starting operation monitoring for: {RequestType}", requestType);

        await next();

        stopwatch.Stop();
        logger.LogDebug("<< Operation monitoring completed for: {RequestType} in {ElapsedMs}ms",
            requestType, stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// Operational monitoring middleware that demonstrates type constraints for product requests only.
/// This middleware will only execute for requests implementing IProductRequest&lt;T&gt;.
/// </summary>
/// <typeparam name="TRequest">The request type constrained to product requests.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public class OperationalMonitoringMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : class, IProductRequest<TResponse>
{
    private readonly ILogger<OperationalMonitoringMiddleware<TRequest, TResponse>> _logger;
    private static readonly ActivitySource ActivitySource = new("TypedMiddlewareExample.OperationalMonitoring", "2.0.0");

    public OperationalMonitoringMiddleware(ILogger<OperationalMonitoringMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 2146483650; // Execute after validation middleware

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"Monitor_{requestName}");

        activity?.SetTag("monitoring.request_type", requestName);
        activity?.SetTag("monitoring.category", "Product");

        _logger.LogInformation("?? [OperationalMonitoring] Monitoring PRODUCT request type {RequestType}", requestName);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next().ConfigureAwait(false);
            
            stopwatch.Stop();
            activity?.SetTag("monitoring.success", "true");
            activity?.SetTag("monitoring.duration_ms", stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("? [OperationalMonitoring] PRODUCT {RequestType} completed successfully in {Duration}ms",
                requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("monitoring.success", "false");
            activity?.SetTag("monitoring.duration_ms", stopwatch.ElapsedMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _logger.LogError(ex, "? [OperationalMonitoring] PRODUCT {RequestType} failed after {Duration}ms: {Error}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}