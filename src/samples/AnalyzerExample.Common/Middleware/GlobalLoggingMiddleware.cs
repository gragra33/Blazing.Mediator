using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

/// <summary>
/// Common middleware shared across all modules
/// </summary>
public class GlobalLoggingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public static int Order => int.MinValue + 1000; // Very early in pipeline
    
    private readonly ILogger<GlobalLoggingMiddleware<TRequest>> _logger;

    public GlobalLoggingMiddleware(ILogger<GlobalLoggingMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        _logger.LogInformation("?? [Global] Starting execution of {RequestType}", requestType);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await next();
            stopwatch.Stop();
            _logger.LogInformation("?? [Global] Completed {RequestType} in {ElapsedMs}ms", 
                requestType, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "?? [Global] Failed {RequestType} after {ElapsedMs}ms", 
                requestType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}