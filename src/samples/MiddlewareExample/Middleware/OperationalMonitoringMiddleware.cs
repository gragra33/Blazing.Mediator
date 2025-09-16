namespace MiddlewareExample.Middleware;

/// <summary>
/// Middleware for monitoring and logging fire-and-forget operations.
/// Provides operational visibility into asynchronous commands.
/// </summary>
/// <typeparam name="TRequest">The type of command.</typeparam>
public class OperationalMonitoringMiddleware<TRequest>(ILogger<OperationalMonitoringMiddleware<TRequest>> logger)
    : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    /// <inheritdoc />
    public int Order => 40;

    /// <inheritdoc />
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        var operationType = typeof(TRequest).Name;
        logger.LogDebug(">> Monitoring operation started: {OperationType}", operationType);

        var stopwatch = Stopwatch.StartNew();

        await next();

        stopwatch.Stop();
        logger.LogInformation("<< Operation completed: {OperationType} in {ElapsedMs}ms",
            operationType, stopwatch.ElapsedMilliseconds);
    }
}
