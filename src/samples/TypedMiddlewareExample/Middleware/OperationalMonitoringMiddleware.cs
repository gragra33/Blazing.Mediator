namespace TypedMiddlewareExample.Middleware;

/// <summary>
/// Middleware for monitoring operational performance across all requests.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
public class OperationalMonitoringMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<OperationalMonitoringMiddleware<TRequest>> _logger;

    public OperationalMonitoringMiddleware(ILogger<OperationalMonitoringMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public int Order => 40; // Execute after business operation audit

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestType = typeof(TRequest).Name;

        _logger.LogDebug(">> Starting operation monitoring for: {RequestType}", requestType);

        await next();

        stopwatch.Stop();
        _logger.LogDebug("<< Operation monitoring completed for: {RequestType} in {ElapsedMs}ms",
            requestType, stopwatch.ElapsedMilliseconds);
    }
}

/// <summary>
/// Middleware for monitoring operational performance across all requests that return responses.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
public class OperationalMonitoringMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<OperationalMonitoringMiddleware<TRequest, TResponse>> _logger;

    public OperationalMonitoringMiddleware(ILogger<OperationalMonitoringMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => 40; // Execute after business operation audit

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestType = typeof(TRequest).Name;

        _logger.LogDebug(">> Starting operation monitoring for: {RequestType}", requestType);

        var response = await next();

        stopwatch.Stop();
        _logger.LogDebug("<< Operation monitoring completed for: {RequestType} in {ElapsedMs}ms",
            requestType, stopwatch.ElapsedMilliseconds);

        return response;
    }
}