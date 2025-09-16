using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using ECommerce.Api.Services;

namespace ECommerce.Api.Middleware;

/// <summary>
/// Blazing.Mediator Middleware that automatically tracks mediator statistics for all requests.
/// Integrates with MediatorStatisticsTracker to provide real-time tracking.
/// </summary>
public class StatisticsTrackingMiddleware<TRequest, TResponse>(MediatorStatisticsTracker statisticsTracker, IHttpContextAccessor httpContextAccessor)
    : IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <inheritdoc/>
    public int Order => 0; // Execute first to track all requests

    /// <inheritdoc/>
    public async Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get session ID from HTTP context (set by SessionTrackingMiddleware)
        var sessionId = GetSessionId();
        
        // Determine if this is a query or command
        var requestType = request.GetType().Name;
        var isQuery = IsQuery(request);
        
        // Track the request
        if (isQuery)
        {
            statisticsTracker.TrackQuery(requestType, sessionId);
        }
        else
        {
            statisticsTracker.TrackCommand(requestType, sessionId);
        }

        // Continue with the pipeline
        return await next();
    }

    private string? GetSessionId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // First try to get session ID from HttpContext.Items (set by SessionTrackingMiddleware)
        if (httpContext.Items.TryGetValue("StatisticsSessionId", out var sessionIdObj) && 
            sessionIdObj is string sessionId && !string.IsNullOrEmpty(sessionId))
        {
            return sessionId;
        }

        // Fallback: Try to get from session directly
        const string sessionKey = "MediatorStatisticsSessionId";
        if (httpContext.Session.TryGetValue(sessionKey, out var sessionBytes))
        {
            var existingSessionId = System.Text.Encoding.UTF8.GetString(sessionBytes);
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                return existingSessionId;
            }
        }

        // Final fallback: Generate a basic session ID
        return $"fallback_{httpContext.TraceIdentifier}";
    }

    private static bool IsQuery(TRequest request)
    {
        // Check if request implements IQuery<T>
        if (request is IQuery<TResponse>)
            return true;

        // Fallback to name-based detection
        var typeName = request.GetType().Name;
        return typeName.EndsWith("Query", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Middleware that automatically tracks mediator statistics for void requests.
/// </summary>
public class StatisticsTrackingVoidMiddleware<TRequest>(MediatorStatisticsTracker statisticsTracker, IHttpContextAccessor httpContextAccessor) : IRequestMiddleware<TRequest>
    where TRequest : IRequest
{
    /// <inheritdoc/>
    public int Order => 0; // Execute first to track all requests

    /// <inheritdoc/>
    public async Task HandleAsync(TRequest request, RequestHandlerDelegate next, CancellationToken cancellationToken)
    {
        // Get session ID from HTTP context
        var sessionId = GetSessionId();
        
        // Track as command (void requests are typically commands)
        var requestType = request.GetType().Name;
        statisticsTracker.TrackCommand(requestType, sessionId);

        // Continue with the pipeline
        await next();
    }

    private string? GetSessionId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // First try to get session ID from HttpContext.Items (set by SessionTrackingMiddleware)
        if (httpContext.Items.TryGetValue("StatisticsSessionId", out var sessionIdObj) && 
            sessionIdObj is string sessionId && !string.IsNullOrEmpty(sessionId))
        {
            return sessionId;
        }

        // Fallback: Try to get from session directly
        const string sessionKey = "MediatorStatisticsSessionId";
        if (httpContext.Session.TryGetValue(sessionKey, out var sessionBytes))
        {
            var existingSessionId = System.Text.Encoding.UTF8.GetString(sessionBytes);
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                return existingSessionId;
            }
        }

        // Final fallback: Generate a basic session ID
        return $"fallback_{httpContext.TraceIdentifier}";
    }
}