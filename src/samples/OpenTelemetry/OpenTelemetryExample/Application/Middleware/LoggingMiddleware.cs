using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Middleware for logging requests for void commands.
/// </summary>
public sealed class LoggingMiddleware<TRequest>(ILogger<LoggingMiddleware<TRequest>> logger) : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger _logger = logger;

    public int Order => -500; // Execute after validation

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting processing of {RequestType}", requestType);
        _logger.LogDebug("Request details for {RequestType}: {@Request}", requestType, request);

        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("request.logged", true);
        Activity.Current?.SetTag("middleware.logging", true);

        try
        {
            await next();
            stopwatch.Stop();

            _logger.LogInformation("Successfully completed {RequestType} in {ElapsedMs}ms", 
                requestType, stopwatch.ElapsedMilliseconds);
            Activity.Current?.SetTag("request.success", true);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process {RequestType} after {ElapsedMs}ms: {ErrorMessage}", 
                requestType, stopwatch.ElapsedMilliseconds, ex.Message);
            Activity.Current?.SetTag("request.success", false);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);
            Activity.Current?.SetTag("request.error", ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Middleware for logging requests and responses.
/// </summary>
public class LoggingMiddleware<TRequest, TResponse>(ILogger<LoggingMiddleware<TRequest, TResponse>> logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger = logger;

    public int Order => -500; // Execute after validation

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var responseType = typeof(TResponse).Name;
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting processing of {RequestType} expecting {ResponseType}", 
            requestType, responseType);
        _logger.LogDebug("Request details for {RequestType}: {@Request}", requestType, request);

        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("response.type", responseType);
        Activity.Current?.SetTag("request.logged", true);
        Activity.Current?.SetTag("middleware.logging", true);

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogInformation("Successfully completed {RequestType} -> {ResponseType} in {ElapsedMs}ms", 
                requestType, responseType, stopwatch.ElapsedMilliseconds);
            _logger.LogDebug("Response for {RequestType}: {@Response}", requestType, response);
            
            Activity.Current?.SetTag("request.success", true);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to process {RequestType} -> {ResponseType} after {ElapsedMs}ms: {ErrorMessage}", 
                requestType, responseType, stopwatch.ElapsedMilliseconds, ex.Message);
            
            Activity.Current?.SetTag("request.success", false);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);
            Activity.Current?.SetTag("request.error", ex.Message);
            throw;
        }
    }
}