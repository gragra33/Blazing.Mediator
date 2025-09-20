using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Middleware for handling errors and exceptions for void commands.
/// </summary>
public sealed class ErrorHandlingMiddleware<TRequest>(ILogger<ErrorHandlingMiddleware<TRequest>> logger)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger _logger = logger;

    public int Order => int.MinValue; // Execute first

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}", typeof(TRequest).Name);

            // Add exception details to current activity
            Activity.Current?.SetTag("error", true);
            Activity.Current?.SetTag("error.type", ex.GetType().Name);
            Activity.Current?.SetTag("error.message", ex.Message);

            throw;
        }
    }
}

/// <summary>
/// Middleware for handling errors and exceptions.
/// </summary>
public sealed class ErrorHandlingMiddleware<TRequest, TResponse>(ILogger<ErrorHandlingMiddleware<TRequest, TResponse>> logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger = logger;

    public int Order => int.MinValue; // Execute first

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {RequestType}", typeof(TRequest).Name);

            // Add exception details to current activity
            Activity.Current?.SetTag("error", true);
            Activity.Current?.SetTag("error.type", ex.GetType().Name);
            Activity.Current?.SetTag("error.message", ex.Message);

            throw;
        }
    }
}