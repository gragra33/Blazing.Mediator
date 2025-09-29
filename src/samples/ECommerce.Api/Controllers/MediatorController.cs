using Blazing.Mediator;
using Blazing.Mediator.Statistics;
using ECommerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Controller for mediator statistics and analysis endpoints.
/// Demonstrates real-time statistics tracking both globally and per session.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MediatorController(MediatorStatistics mediatorStatistics, MediatorStatisticsTracker statisticsTracker, IServiceProvider serviceProvider) : ControllerBase
{
    /// <summary>
    /// Gets the current session ID for statistics tracking.
    /// Useful for identifying which session to query for session-specific statistics.
    /// </summary>
    /// <returns>The current session ID and session information</returns>
    /// <response code="200">Returns the session ID information</response>
    [HttpGet("session")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetSessionId()
    {
        // Ensure session is loaded
        HttpContext.Session.LoadAsync().Wait();

        // Get the session ID from session storage (same key as SessionTrackingMiddleware)
        const string sessionKey = "MediatorStatisticsSessionId";
        string? sessionId = null;

        if (HttpContext.Session.TryGetValue(sessionKey, out var sessionBytes))
        {
            sessionId = System.Text.Encoding.UTF8.GetString(sessionBytes);
        }

        // Also check HttpContext.Items as backup
        if (string.IsNullOrEmpty(sessionId) &&
            HttpContext.Items.TryGetValue("StatisticsSessionId", out var sessionIdObj) &&
            sessionIdObj is string itemsSessionId)
        {
            sessionId = itemsSessionId;
        }

        // Return the session ID information
        return Ok(new
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
                InitializeSession = "Make any API request (like GET /api/products) to initialize session tracking",
                CheckAgain = "After making a request, call this endpoint again to see your session ID"
            } : null,
            SessionInfo = new
            {
                SessionAvailable = HttpContext.Session.IsAvailable,
                AspNetCoreSessionId = HttpContext.Session.Id,
                StatisticsSessionId = sessionId,
                SessionKeys = HttpContext.Session.Keys.ToArray()
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets comprehensive real-time mediator statistics including global and session-based tracking.
    /// This returns detailed statistics that update as queries and commands are executed.
    /// </summary>
    /// <returns>Comprehensive real-time mediator usage statistics</returns>
    /// <response code="200">Returns the global mediator statistics</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetStatistics()
    {
        var globalStats = statisticsTracker.GetGlobalStatistics();

        return Ok(new
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
                TypeAnalysis = "Use /api/mediator/analyze endpoints for detailed type analysis"
            },
            globalStats.LastUpdated
        });
    }

    /// <summary>
    /// Gets statistics for a specific session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Session-specific statistics</returns>
    /// <response code="200">Returns the session-specific statistics</response>
    /// <response code="404">Session not found or has no activity</response>
    [HttpGet("statistics/session/{sessionId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public IActionResult GetSessionStatistics(string sessionId)
    {
        var sessionStats = statisticsTracker.GetSessionStatistics(sessionId);

        if (sessionStats == null)
        {
            return NotFound(new { Error = $"Session '{sessionId}' not found or has no activity" });
        }

        return Ok(new
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
    }

    /// <summary>
    /// Gets statistics for all active sessions.
    /// </summary>
    /// <returns>Statistics for all active sessions</returns>
    /// <response code="200">Returns statistics for all active sessions</response>
    [HttpGet("statistics/sessions")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAllSessionStatistics()
    {
        var allSessionStats = statisticsTracker.GetAllSessionStatistics();

        return Ok(new
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
    }

    /// <summary>
    /// Analyzes all queries in the application grouped by assembly and namespace.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Detailed analysis of all discovered queries</returns>
    /// <response code="200">Returns the query analysis results</response>
    [HttpGet("analyze/queries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult AnalyzeQueries([FromQuery] bool detailed = true)
    {
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider, detailed);

        var result = queries
            .GroupBy(q => q.Assembly)
            .OrderBy(g => g.Key)
            .Select(assemblyGroup => new
            {
                Assembly = assemblyGroup.Key,
                Namespaces = assemblyGroup
                    .GroupBy(q => q.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(namespaceGroup => new
                    {
                        Namespace = namespaceGroup.Key,
                        Queries = namespaceGroup
                            .OrderBy(q => q.ClassName)
                            .Select(q => new
                            {
                                q.ClassName,
                                q.TypeParameters,
                                q.PrimaryInterface,
                                ResponseType = q.ResponseType?.Name,
                                q.IsResultType,
                                FullTypeName = q.Type.FullName,
                                HandlerStatus = q.HandlerStatus.ToString(),
                                q.HandlerDetails,
                                Handlers = q.Handlers.Select(h => h.Name).ToList(),
                                StatusIcon = q.HandlerStatus switch
                                {
                                    HandlerStatus.Single => "+",
                                    HandlerStatus.Missing => "!",
                                    HandlerStatus.Multiple => "#",
                                    _ => "?"
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return Ok(new
        {
            TotalQueries = queries.Count,
            IsDetailed = detailed,
            QueriesByAssembly = result,
            Summary = new
            {
                WithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Analyzes all commands in the application grouped by assembly and namespace.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Detailed analysis of all discovered commands</returns>
    /// <response code="200">Returns the command analysis results</response>
    [HttpGet("analyze/commands")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult AnalyzeCommands([FromQuery] bool detailed = true)
    {
        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider, detailed);

        var result = commands
            .GroupBy(c => c.Assembly)
            .OrderBy(g => g.Key)
            .Select(assemblyGroup => new
            {
                Assembly = assemblyGroup.Key,
                Namespaces = assemblyGroup
                    .GroupBy(c => c.Namespace)
                    .OrderBy(g => g.Key)
                    .Select(namespaceGroup => new
                    {
                        Namespace = namespaceGroup.Key,
                        Commands = namespaceGroup
                            .OrderBy(c => c.ClassName)
                            .Select(c => new
                            {
                                c.ClassName,
                                c.TypeParameters,
                                c.PrimaryInterface,
                                ResponseType = c.ResponseType?.Name,
                                c.IsResultType,
                                FullTypeName = c.Type.FullName,
                                HandlerStatus = c.HandlerStatus.ToString(),
                                c.HandlerDetails,
                                Handlers = c.Handlers.Select(h => h.Name).ToList(),
                                StatusIcon = c.HandlerStatus switch
                                {
                                    HandlerStatus.Single => "+",
                                    HandlerStatus.Missing => "!",
                                    HandlerStatus.Multiple => "#",
                                    _ => "?"
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return Ok(new
        {
            TotalCommands = commands.Count,
            IsDetailed = detailed,
            CommandsByAssembly = result,
            Summary = new
            {
                WithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                MissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                MultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
            },
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets a comprehensive analysis of both queries and commands.
    /// </summary>
    /// <param name="detailed">If true, returns comprehensive analysis with all properties. If false, returns compact analysis with basic information only.</param>
    /// <returns>Complete mediator analysis including queries, commands, and statistics</returns>
    /// <response code="200">Returns the complete analysis results</response>
    [HttpGet("analyze")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetCompleteAnalysis([FromQuery] bool detailed = true)
    {
        var queries = mediatorStatistics.AnalyzeQueries(serviceProvider, detailed);
        var commands = mediatorStatistics.AnalyzeCommands(serviceProvider, detailed);

        return Ok(new
        {
            Summary = new
            {
                TotalQueries = queries.Count,
                TotalCommands = commands.Count,
                TotalTypes = queries.Count + commands.Count,
                IsDetailed = detailed,
                HealthStatus = new
                {
                    QueriesWithHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Single),
                    QueriesMissingHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Missing),
                    QueriesWithMultipleHandlers = queries.Count(q => q.HandlerStatus == HandlerStatus.Multiple),
                    CommandsWithHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Single),
                    CommandsMissingHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Missing),
                    CommandsWithMultipleHandlers = commands.Count(c => c.HandlerStatus == HandlerStatus.Multiple)
                }
            },
            Queries = queries
                .GroupBy(q => q.Assembly)
                .ToDictionary(g => g.Key, g => g.GroupBy(q => q.Namespace).ToDictionary(n => n.Key, n => n.Select(q => new
                {
                    q.ClassName,
                    q.TypeParameters,
                    q.PrimaryInterface,
                    ResponseType = q.ResponseType?.Name,
                    q.IsResultType,
                    HandlerStatus = q.HandlerStatus.ToString(),
                    q.HandlerDetails,
                    StatusIcon = q.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList())),
            Commands = commands
                .GroupBy(c => c.Assembly)
                .ToDictionary(g => g.Key, g => g.GroupBy(c => c.Namespace).ToDictionary(n => n.Key, n => n.Select(c => new
                {
                    c.ClassName,
                    c.TypeParameters,
                    c.PrimaryInterface,
                    ResponseType = c.ResponseType?.Name,
                    c.IsResultType,
                    HandlerStatus = c.HandlerStatus.ToString(),
                    c.HandlerDetails,
                    StatusIcon = c.HandlerStatus switch
                    {
                        HandlerStatus.Single => "+",
                        HandlerStatus.Missing => "!",
                        HandlerStatus.Multiple => "#",
                        _ => "?"
                    }
                }).ToList())),
            Legend = new
            {
                Symbols = new { Success = "+", Missing = "!", Multiple = "#" },
                Description = "+ = Handler found, ! = No handler, # = Multiple handlers"
            },
            Timestamp = DateTime.UtcNow
        });
    }
}