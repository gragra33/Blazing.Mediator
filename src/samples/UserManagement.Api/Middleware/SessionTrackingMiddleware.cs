namespace UserManagement.Api.Middleware;

/// <summary>
/// ASP.NET Core middleware that ensures session tracking is properly initialized and managed.
/// This middleware runs early in the pipeline to ensure session state is available.
/// </summary>
public class SessionTrackingMiddleware(
    RequestDelegate next, 
    ILogger<SessionTrackingMiddleware> logger)
{

    /// <summary>
    /// Invokes the middleware to handle session tracking initialization.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Create a fallback session ID that always works
            var fallbackSessionId = $"fallback_{context.TraceIdentifier}_{DateTime.UtcNow.Ticks}";
            
            // Always set a fallback session ID first
            context.Items["StatisticsSessionId"] = fallbackSessionId;

            // Try to enhance with session-based ID if available
            if (context.Session.IsAvailable)
            {
                try
                {
                    await context.Session.LoadAsync();
                    var sessionId = GetOrCreateSessionId(context);
                    context.Items["StatisticsSessionId"] = sessionId;
                    
                    logger.LogDebug("Session tracking initialized: {SessionId} for request {RequestPath}", 
                        sessionId, context.Request.Path);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to load session, using fallback: {SessionId}", fallbackSessionId);
                    // Keep the fallback session ID we already set
                }
            }
            else
            {
                logger.LogDebug("Session not available, using fallback: {SessionId} for request {RequestPath}", 
                    fallbackSessionId, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            // Last resort fallback
            var lastResortSessionId = $"emergency_{context.TraceIdentifier}";
            context.Items["StatisticsSessionId"] = lastResortSessionId;
            
            logger.LogError(ex, "Session tracking completely failed, using emergency fallback: {SessionId}", lastResortSessionId);
        }

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
        var newSessionId = GenerateSessionId(context);
        
        // Store the session ID in session storage for persistence across requests
        var sessionIdBytes = System.Text.Encoding.UTF8.GetBytes(newSessionId);
        context.Session.Set(sessionKey, sessionIdBytes);

        logger.LogInformation("Created new statistics session: {SessionId}", newSessionId);

        return newSessionId;
    }

    private static string GenerateSessionId(HttpContext context)
    {
        // Generate a more meaningful session ID that includes timestamp and connection info
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomPart = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID

        return $"stats_{timestamp}_{randomPart}";
    }
}