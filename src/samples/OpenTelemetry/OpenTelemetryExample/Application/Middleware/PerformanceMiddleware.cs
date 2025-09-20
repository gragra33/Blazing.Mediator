using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Middleware;

/// <summary>
/// Middleware for monitoring performance for void commands.
/// </summary>
public sealed class PerformanceMiddleware<TRequest>(ILogger<PerformanceMiddleware<TRequest>> logger)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger _logger = logger;

    public int Order => -100; // Execute close to handler

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next();
            stopwatch.Stop();

            var duration = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("Request {RequestType} completed in {Duration}ms", requestType, duration);

            // Add performance details to current activity
            Activity.Current?.SetTag("performance.duration_ms", duration);
            Activity.Current?.SetTag("performance.measured", true);
        }
        catch (Exception)
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            _logger.LogWarning("Request {RequestType} failed after {Duration}ms", requestType, duration);

            Activity.Current?.SetTag("performance.duration_ms", duration);
            Activity.Current?.SetTag("performance.measured", true);
            Activity.Current?.SetTag("performance.failed", true);

            throw;
        }
    }
}

/// <summary>
/// Middleware for monitoring performance.
/// </summary>
public sealed class PerformanceMiddleware<TRequest, TResponse>(
    ILogger<PerformanceMiddleware<TRequest, TResponse>> logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger = logger;

    public int Order => -100; // Execute close to handler

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            var duration = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation("Request {RequestType} completed in {Duration}ms", requestType, duration);

            // Add performance details to current activity
            Activity.Current?.SetTag("performance.duration_ms", duration);
            Activity.Current?.SetTag("performance.measured", true);

            return response;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            _logger.LogWarning("Request {RequestType} failed after {Duration}ms", requestType, duration);

            Activity.Current?.SetTag("performance.duration_ms", duration);
            Activity.Current?.SetTag("performance.measured", true);
            Activity.Current?.SetTag("performance.failed", true);

            throw;
        }
    }
}