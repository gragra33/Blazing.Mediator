namespace TypedMiddlewareExample.Middleware;

/// <summary>
/// Global error handling middleware that catches exceptions and provides clean error messages.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
public class ErrorHandlingMiddleware<TRequest> : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    private readonly ILogger<ErrorHandlingMiddleware<TRequest>> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware<TRequest>> logger)
    {
        _logger = logger;
    }

    public int Order => int.MinValue; // Execute first

    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        try
        {
            await next();
        }
        catch (ValidationException ex)
        {
            _logger.LogError("!! {RequestType} failed due to validation errors: {ErrorMessage}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!! Unexpected error processing {RequestType}: {ErrorMessage}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Global error handling middleware for requests that return a response.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response being returned.</typeparam>
public class ErrorHandlingMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ErrorHandlingMiddleware<TRequest, TResponse>> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public int Order => int.MinValue; // Execute first

    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            _logger.LogError("!! {RequestType} failed due to validation errors: {ErrorMessage}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "!! Unexpected error processing {RequestType}: {ErrorMessage}",
                typeof(TRequest).Name, ex.Message);
            throw;
        }
    }
}