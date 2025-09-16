namespace ECommerce.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that ensures session tracking is properly initialized and managed.
/// This middleware runs early in the pipeline to ensure session state is available.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SessionTrackingMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the request processing pipeline.</param>
/// <param name="logger">The logger instance.</param>
public class SessionTrackingMiddleware(RequestDelegate next, ILogger<SessionTrackingMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware to track session information for each request.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Ensure session is loaded and available
        await context.Session.LoadAsync();

        // Get or create a persistent session ID for statistics tracking
        var sessionId = GetOrCreateSessionId(context);

        // Store the session ID in HttpContext.Items for use by other middleware
        context.Items["StatisticsSessionId"] = sessionId;

        // Log session activity for debugging
        logger.LogDebug("Session tracking initialized: {SessionId} for request {RequestPath}", 
            sessionId, context.Request.Path);

        await next(context);
    }

    private string GetOrCreateSessionId(HttpContext context)
    {
        const string sessionKey = "MediatorStatisticsSessionId";

        // Try to get existing session ID from session storage
        if (context.Session.TryGetValue(sessionKey, out var sessionBytes))
        {
            var existingSessionId = System.Text.Encoding.UTF8.GetString(sessionBytes);
            if (!string.IsNullOrEmpty(existingSessionId))
            {
                return existingSessionId;
            }
        }

        // Create new session ID if none exists
        var newSessionId = GenerateSessionId();
        
        // Store the session ID in session storage for persistence across requests
        var sessionIdBytes = System.Text.Encoding.UTF8.GetBytes(newSessionId);
        context.Session.Set(sessionKey, sessionIdBytes);

        logger.LogInformation("Created new statistics session: {SessionId}", newSessionId);

        return newSessionId;
    }

    private static string GenerateSessionId()
    {
        // Generate a more meaningful session ID that includes timestamp and connection info
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomPart = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID

        return $"stats_{timestamp}_{randomPart}";
    }
}