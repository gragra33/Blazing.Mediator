namespace MiddlewareExample.Middleware;

/// <summary>
/// Middleware for logging email operations and tracking email delivery metrics.
/// </summary>
public class EmailLoggingMiddleware(ILogger<EmailLoggingMiddleware> logger)
    : IRequestMiddleware<SendOrderConfirmationCommand>
{
    /// <inheritdoc />
    public int Order => 10;

    /// <inheritdoc />
    public async Task HandleAsync(SendOrderConfirmationCommand request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        logger.LogDebug(">> Email operation started for order: {OrderId} to: {CustomerEmail}", 
            request.OrderId, request.CustomerEmail);
        
        var stopwatch = Stopwatch.StartNew();
        
        await next();
        
        stopwatch.Stop();
        logger.LogDebug("<< Email operation completed in {ElapsedMs}ms for order: {OrderId}", 
            stopwatch.ElapsedMilliseconds, request.OrderId);
    }
}
