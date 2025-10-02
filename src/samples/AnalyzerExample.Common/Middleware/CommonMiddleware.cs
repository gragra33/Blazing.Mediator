using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

public class GlobalLoggingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static int Order => int.MinValue + 1000; // Very early in pipeline
    
    private readonly ILogger<GlobalLoggingMiddleware<TRequest, TResponse>> _logger;

    public GlobalLoggingMiddleware(ILogger<GlobalLoggingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var responseType = typeof(TResponse).Name;
        _logger.LogInformation("?? [Global] Starting execution of {RequestType} -> {ResponseType}", 
            requestType, responseType);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            var result = await next();
            stopwatch.Stop();
            _logger.LogInformation("?? [Global] Completed {RequestType} -> {ResponseType} in {ElapsedMs}ms", 
                requestType, responseType, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? [Global] Failed {RequestType} -> {ResponseType} after {ElapsedMs}ms", 
                requestType, responseType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}