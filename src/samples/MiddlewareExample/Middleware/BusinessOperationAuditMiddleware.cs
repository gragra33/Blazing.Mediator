namespace MiddlewareExample.Middleware;

/// <summary>
/// Middleware for auditing and logging all business operations with responses.
/// Tracks command execution for compliance and monitoring purposes.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class BusinessOperationAuditMiddleware<TRequest, TResponse>(
    ILogger<BusinessOperationAuditMiddleware<TRequest, TResponse>> logger)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc />
    public int Order => 30;

    /// <inheritdoc />
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var operationType = typeof(TRequest).Name;
        logger.LogDebug(">> Starting business operation audit: {OperationType}", operationType);
        
        var stopwatch = Stopwatch.StartNew();
        
        var response = await next();
        
        stopwatch.Stop();
        logger.LogInformation("<< Business operation completed: {OperationType} in {ElapsedMs}ms", 
            operationType, stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}
