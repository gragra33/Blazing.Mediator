using Blazing.Mediator.Statistics;
using UserManagement.Api.Services;

namespace UserManagement.Api.Endpoints;

/// <summary>
/// Handles mediator analysis and statistics endpoints following single responsibility principle.
/// Provides comprehensive analysis of queries, commands, handlers, and real-time statistics tracking.
/// </summary>
public static class MediatorAnalysisEndpoints
{
    /// <summary>
    /// Maps mediator analysis endpoints to the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder MapMediatorAnalysisEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/health", (ILogger<Program> logger) =>
            {
                logger.LogInformation("Health check requested");

                return Results.Ok(new
                {
                    Status = "Healthy",
                    Service = "User Management API - Mediator Analysis",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0"
                });
            })
            .WithName("GetAnalysisHealth")
            .WithSummary("Get health status")
            .WithDescription("Gets a health check status and basic mediator information")
            .Produces<object>();

        return group;
    }

    /// <summary>
    /// Maps mediator statistics endpoints to the route group.
    /// Provides real-time statistics tracking both globally and per session.
    /// Based on ECommerce.Api MediatorController implementation but using minimal APIs.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder MapMediatorStatisticsEndpoints(this RouteGroupBuilder group)
    {
        // Session ID endpoint - equivalent to ECommerce.Api GetSessionId action
        group.MapGet("/session", (HttpContext httpContext) =>
            {
                // Ensure session is loaded
                httpContext.Session.LoadAsync().Wait();

                // Get the session ID from session storage (same key as SessionTrackingMiddleware)
                const string sessionKey = "MediatorStatisticsSessionId";
                string? sessionId = null;

                if (httpContext.Session.TryGetValue(sessionKey, out var sessionBytes))
                {
                    sessionId = System.Text.Encoding.UTF8.GetString(sessionBytes);
                }

                // Also check HttpContext.Items as backup
                if (string.IsNullOrEmpty(sessionId) &&
                    httpContext.Items.TryGetValue("StatisticsSessionId", out var sessionIdObj) &&
                    sessionIdObj is string itemsSessionId)
                {
                    sessionId = itemsSessionId;
                }

                // Return the session ID information
                return Results.Ok(new
                {
                    Message = string.IsNullOrEmpty(sessionId) ? "Session ID Not Yet Assigned" : "Current Session ID",
                    SessionId = sessionId,
                    Note = string.IsNullOrEmpty(sessionId)
                        ? "No session ID has been assigned yet. Make a request that triggers mediator operations to initialize session tracking."
                        : "This session ID is used for tracking your mediator statistics across requests",
                    Usage = string.IsNullOrEmpty(sessionId) ? null : new
                    {
                        ViewSessionStats = $"GET /api/mediator/statistics/session/{sessionId}",
                        ViewGlobalStats = "GET /api/mediator/statistics",
                        ViewAllSessions = "GET /api/mediator/statistics/sessions"
                    },
                    Instructions = string.IsNullOrEmpty(sessionId) ? new
                    {
                        InitializeSession = "Make any API request (like GET /api/users) to initialize session tracking",
                        CheckAgain = "After making a request, call this endpoint again to see your session ID"
                    } : null,
                    SessionInfo = new
                    {
                        SessionAvailable = httpContext.Session.IsAvailable,
                        AspNetCoreSessionId = httpContext.Session.Id,
                        StatisticsSessionId = sessionId,
                        SessionKeys = httpContext.Session.Keys.ToArray()
                    },
                    Timestamp = DateTime.UtcNow
                });
            })
            .WithName("GetSessionId")
            .WithSummary("Get current session ID")
            .WithDescription("Gets the current session ID for statistics tracking")
            .Produces<object>();

        // Global statistics endpoint - equivalent to ECommerce.Api GetStatistics action
        group.MapGet("/statistics", (MediatorStatisticsTracker statisticsTracker) =>
            {
                var globalStats = statisticsTracker.GetGlobalStatistics();

                return Results.Ok(new
                {
                    Message = "Real-Time Mediator Statistics",
                    Note = "These statistics update dynamically as requests are processed and track both global and session-based usage",
                    GlobalStatistics = new
                    {
                        Summary = new
                        {
                            globalStats.UniqueQueryTypes,
                            globalStats.UniqueCommandTypes,
                            globalStats.UniqueNotificationTypes,
                            globalStats.TotalQueryExecutions,
                            globalStats.TotalCommandExecutions,
                            globalStats.TotalNotificationExecutions,
                            globalStats.ActiveSessions
                        },
                        Details = new
                        {
                            globalStats.QueryTypes,
                            globalStats.CommandTypes,
                            globalStats.NotificationTypes
                        }
                    },
                    TrackingInfo = new
                    {
                        Method = "Real-time tracking via StatisticsTrackingMiddleware with session persistence",
                        Scope = "Global statistics track all application usage, session statistics track per-user activity",
                        SessionTracking = "Enabled - tracks per session/user statistics using ASP.NET Core session state",
                        AutoCleanup = "Inactive sessions are automatically cleaned up after 2 hours"
                    },
                    Instructions = new
                    {
                        GetSessionId = "Use GET /api/mediator/session to get your current session ID",
                        ViewSessionStats = "Use GET /api/mediator/statistics/session/{sessionId} for session-specific statistics",
                        ViewAllSessions = "Use GET /api/mediator/statistics/sessions for all active sessions",
                        TypeAnalysis = "Use /api/analysis/health for basic analysis"
                    },
                    globalStats.LastUpdated
                });
            })
            .WithName("GetStatistics")
            .WithSummary("Get real-time global statistics")
            .WithDescription("Gets comprehensive real-time mediator statistics including global and session-based tracking")
            .Produces<object>();

        // Session-specific statistics endpoint - equivalent to ECommerce.Api GetSessionStatistics action
        group.MapGet("/statistics/session/{sessionId}", (string sessionId, MediatorStatisticsTracker statisticsTracker) =>
            {
                var sessionStats = statisticsTracker.GetSessionStatistics(sessionId);

                if (sessionStats == null)
                {
                    return Results.NotFound(new { Error = $"Session '{sessionId}' not found or has no activity" });
                }

                return Results.Ok(new
                {
                    Message = $"Session Statistics for '{sessionId}'",
                    SessionStatistics = new
                    {
                        sessionStats.SessionId,
                        Summary = new
                        {
                            sessionStats.UniqueQueryTypes,
                            sessionStats.UniqueCommandTypes,
                            sessionStats.UniqueNotificationTypes,
                            sessionStats.TotalQueryExecutions,
                            sessionStats.TotalCommandExecutions,
                            sessionStats.TotalNotificationExecutions
                        },
                        Details = new
                        {
                            sessionStats.QueryTypes,
                            sessionStats.CommandTypes,
                            sessionStats.NotificationTypes
                        },
                        sessionStats.LastActivity
                    },
                    Timestamp = DateTime.UtcNow
                });
            })
            .WithName("GetSessionStatistics")
            .WithSummary("Get session statistics")
            .WithDescription("Gets statistics for a specific session")
            .Produces<object>()
            .Produces(404);

        // All sessions statistics endpoint - equivalent to ECommerce.Api GetAllSessionStatistics action
        group.MapGet("/statistics/sessions", (MediatorStatisticsTracker statisticsTracker) =>
            {
                var allSessionStats = statisticsTracker.GetAllSessionStatistics();

                return Results.Ok(new
                {
                    Message = "Statistics for All Active Sessions",
                    TotalActiveSessions = allSessionStats.Count,
                    Sessions = allSessionStats.Select(session => new
                    {
                        session.SessionId,
                        Summary = new
                        {
                            session.UniqueQueryTypes,
                            session.UniqueCommandTypes,
                            session.UniqueNotificationTypes,
                            session.TotalQueryExecutions,
                            session.TotalCommandExecutions,
                            session.TotalNotificationExecutions
                        },
                        session.LastActivity
                    }).OrderByDescending(s => s.LastActivity),
                    Timestamp = DateTime.UtcNow
                });
            })
            .WithName("GetAllSessionStatistics")
            .WithSummary("Get all session statistics")
            .WithDescription("Gets statistics for all active sessions")
            .Produces<object>();

        return group;
    }
}

/// <summary>
/// Statistics renderer that captures output to a list for API responses.
/// </summary>
public class CapturingStatisticsRenderer : IStatisticsRenderer
{
    private readonly List<string> _capturedOutput;

    /// <summary>
    /// Initializes a new instance of the CapturingStatisticsRenderer class.
    /// </summary>
    /// <param name="capturedOutput">The list to capture output to.</param>
    public CapturingStatisticsRenderer(List<string> capturedOutput)
    {
        _capturedOutput = capturedOutput;
    }

    /// <summary>
    /// Renders a message by adding it to the captured output list.
    /// </summary>
    /// <param name="message">The message to render.</param>
    public void Render(string message)
    {
        _capturedOutput.Add(message);
    }
}