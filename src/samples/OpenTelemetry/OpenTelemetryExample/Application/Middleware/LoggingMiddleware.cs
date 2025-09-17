using System.Diagnostics;
using Blazing.Mediator;
using Blazing.Mediator.Abstractions;

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
        _logger.LogInformation("Handling request {RequestType}", requestType);
        
        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("request.logged", true);
        
        try
        {
            await next();
            
            _logger.LogInformation("Successfully handled request {RequestType}", requestType);
            Activity.Current?.SetTag("request.success", true);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to handle request {RequestType}", requestType);
            Activity.Current?.SetTag("request.success", false);
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
        _logger.LogInformation("Handling request {RequestType}", requestType);

        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("request.logged", true);

        try
        {
            var response = await next();

            _logger.LogInformation("Successfully handled request {RequestType}", requestType);
            Activity.Current?.SetTag("request.success", true);

            return response;
        }
        catch (Exception)
        {
            _logger.LogError("Failed to handle request {RequestType}", requestType);
            Activity.Current?.SetTag("request.success", false);
            throw;
        }
    }
}