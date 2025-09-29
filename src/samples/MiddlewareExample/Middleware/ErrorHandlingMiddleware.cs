namespace MiddlewareExample.Middleware;

/// <summary>
/// Base class for error handling middleware that provides shared error handling logic.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
public abstract class ErrorHandlingMiddlewareBase<TRequest>(ILogger logger)
{
    protected readonly ILogger Logger = logger;

    public int Order => int.MinValue; // Execute first to wrap entire pipeline in error handling

    /// <summary>
    /// Handles errors for validation and general exceptions, converting them to user-friendly errors.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <exception cref="InvalidOperationException">Thrown with user-friendly message to avoid leaking internal details.</exception>
    protected void HandleError(Exception exception)
    {
        var requestTypeName = typeof(TRequest).Name;

        switch (exception)
        {
            case ValidationException vex:
                Logger.LogError("!! Validation failed for {RequestType}: {Errors}", requestTypeName, vex.Message);
                // Convert to a more generic error to avoid leaking internal details
                throw new InvalidOperationException("Validation failed while processing the request", vex);

            default:
                Logger.LogError(exception, "!! ErrorHandlingMiddleware: Caught error in {RequestType}", requestTypeName);
                // Convert to a more generic error to avoid leaking internal details
                throw new InvalidOperationException("An error occurred while processing the request", exception);
        }
    }

    /// <summary>
    /// Logs the start of request handling.
    /// </summary>
    protected void LogStart()
    {
        Logger.LogDebug(">> ErrorHandlingMiddleware: Before handling {RequestType}", typeof(TRequest).Name);
    }

    /// <summary>
    /// Logs the completion of request handling.
    /// </summary>
    protected void LogCompletion()
    {
        Logger.LogDebug("<< ErrorHandlingMiddleware: After handling {RequestType}", typeof(TRequest).Name);
    }
}

/// <summary>
/// Global error handling middleware for commands that wraps the entire pipeline in error handling.
/// Catches exceptions and converts them to user-friendly errors to avoid leaking internal details.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
public class ErrorHandlingMiddleware<TRequest>(ILogger<ErrorHandlingMiddleware<TRequest>> logger)
    : ErrorHandlingMiddlewareBase<TRequest>(logger), IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        LogStart();
        try
        {
            await next();
            LogCompletion();
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }
}

/// <summary>
/// Global error handling middleware for queries that wraps the entire pipeline in error handling.
/// Catches exceptions and converts them to user-friendly errors to avoid leaking internal details.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
public class ErrorHandlingMiddleware<TRequest, TResponse>(ILogger<ErrorHandlingMiddleware<TRequest, TResponse>> logger)
    : ErrorHandlingMiddlewareBase<TRequest>(logger), IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        LogStart();
        try
        {
            var result = await next();
            LogCompletion();
            return result;
        }
        catch (Exception ex)
        {
            HandleError(ex);
            throw; // This will never be reached due to HandleError throwing, but satisfies compiler
        }
    }
}
