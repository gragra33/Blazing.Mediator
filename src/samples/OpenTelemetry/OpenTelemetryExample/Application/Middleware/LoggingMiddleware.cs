using Blazing.Mediator;
using Blazing.Mediator;
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

        LoggingMiddlewareLog.LogStartProcessing(_logger, requestType);
        LoggingMiddlewareLog.LogRequestDetails(_logger, requestType, request);

        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("request.logged", true);
        Activity.Current?.SetTag("middleware.logging", true);

        try
        {
            await next().ConfigureAwait(false);
            stopwatch.Stop();

            LoggingMiddlewareLog.LogCompleted(_logger, requestType, stopwatch.ElapsedMilliseconds);
            Activity.Current?.SetTag("request.success", true);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LoggingMiddlewareLog.LogFailed(_logger, ex, requestType, stopwatch.ElapsedMilliseconds, ex.Message);
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

        LoggingMiddlewareLog.LogStartProcessingWithResponse(_logger, requestType, responseType);
        LoggingMiddlewareLog.LogRequestDetailsWithResponse(_logger, requestType, request);

        // Add request details to current activity
        Activity.Current?.SetTag("request.type", requestType);
        Activity.Current?.SetTag("response.type", responseType);
        Activity.Current?.SetTag("request.logged", true);
        Activity.Current?.SetTag("middleware.logging", true);

        try
        {
            var response = await next().ConfigureAwait(false);
            stopwatch.Stop();

            LoggingMiddlewareLog.LogCompletedWithResponse(_logger, requestType, responseType, stopwatch.ElapsedMilliseconds);
            LoggingMiddlewareLog.LogResponse(_logger, requestType, response is null ? "null" : response);

            Activity.Current?.SetTag("request.success", true);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LoggingMiddlewareLog.LogFailedWithResponse(_logger, ex, requestType, responseType, stopwatch.ElapsedMilliseconds, ex.Message);

            Activity.Current?.SetTag("request.success", false);
            Activity.Current?.SetTag("request.duration_ms", stopwatch.ElapsedMilliseconds);
            Activity.Current?.SetTag("request.error", ex.Message);
            throw;
        }
    }
}
