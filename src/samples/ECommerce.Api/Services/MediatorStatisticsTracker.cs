using ECommerce.Api.Models;
using System.Collections.Concurrent;

namespace ECommerce.Api.Services;

/// <summary>
/// Service for tracking mediator statistics both globally and per session.
/// Provides real-time statistics that update as requests are processed.
/// </summary>
public class MediatorStatisticsTracker
{
    private readonly ConcurrentDictionary<string, long> _globalQueryCounts = new();
    private readonly ConcurrentDictionary<string, long> _globalCommandCounts = new();
    private readonly ConcurrentDictionary<string, long> _globalNotificationCounts = new();

    private readonly ConcurrentDictionary<string, SessionStatistics> _sessionStatistics = new();

    /// <summary>
    /// Tracks a query execution globally and for the specified session.
    /// </summary>
    /// <param name="queryType">The type name of the query.</param>
    /// <param name="sessionId">The session identifier (optional).</param>
    public void TrackQuery(string queryType, string? sessionId = null)
    {
        if (string.IsNullOrEmpty(queryType)) return;

        // Track globally
        _globalQueryCounts.AddOrUpdate(queryType, 1, (_, count) => count + 1);

        // Track per session if sessionId provided
        if (!string.IsNullOrEmpty(sessionId))
        {
            var sessionStats = _sessionStatistics.GetOrAdd(sessionId, _ => new SessionStatistics());
            sessionStats.QueryCounts.AddOrUpdate(queryType, 1, (_, count) => count + 1);
            sessionStats.LastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tracks a command execution globally and for the specified session.
    /// </summary>
    /// <param name="commandType">The type name of the command.</param>
    /// <param name="sessionId">The session identifier (optional).</param>
    public void TrackCommand(string commandType, string? sessionId = null)
    {
        if (string.IsNullOrEmpty(commandType)) return;

        // Track globally
        _globalCommandCounts.AddOrUpdate(commandType, 1, (_, count) => count + 1);

        // Track per session if sessionId provided
        if (!string.IsNullOrEmpty(sessionId))
        {
            var sessionStats = _sessionStatistics.GetOrAdd(sessionId, _ => new SessionStatistics());
            sessionStats.CommandCounts.AddOrUpdate(commandType, 1, (_, count) => count + 1);
            sessionStats.LastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Tracks a notification publication globally and for the specified session.
    /// </summary>
    /// <param name="notificationType">The type name of the notification.</param>
    /// <param name="sessionId">The session identifier (optional).</param>
    public void TrackNotification(string notificationType, string? sessionId = null)
    {
        if (string.IsNullOrEmpty(notificationType)) return;

        // Track globally
        _globalNotificationCounts.AddOrUpdate(notificationType, 1, (_, count) => count + 1);

        // Track per session if sessionId provided
        if (!string.IsNullOrEmpty(sessionId))
        {
            var sessionStats = _sessionStatistics.GetOrAdd(sessionId, _ => new SessionStatistics());
            sessionStats.NotificationCounts.AddOrUpdate(notificationType, 1, (_, count) => count + 1);
            sessionStats.LastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Gets the global statistics summary.
    /// </summary>
    /// <returns>Global statistics summary.</returns>
    public GlobalStatisticsSummary GetGlobalStatistics()
    {
        return new GlobalStatisticsSummary
        {
            UniqueQueryTypes = _globalQueryCounts.Count,
            UniqueCommandTypes = _globalCommandCounts.Count,
            UniqueNotificationTypes = _globalNotificationCounts.Count,
            TotalQueryExecutions = _globalQueryCounts.Values.Sum(),
            TotalCommandExecutions = _globalCommandCounts.Values.Sum(),
            TotalNotificationExecutions = _globalNotificationCounts.Values.Sum(),
            QueryTypes = _globalQueryCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CommandTypes = _globalCommandCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            NotificationTypes = _globalNotificationCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            ActiveSessions = _sessionStatistics.Count,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets statistics for a specific session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Session statistics or null if session not found.</returns>
    public SessionStatisticsSummary? GetSessionStatistics(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId) || !_sessionStatistics.TryGetValue(sessionId, out var sessionStats))
            return null;

        return new SessionStatisticsSummary
        {
            SessionId = sessionId,
            UniqueQueryTypes = sessionStats.QueryCounts.Count,
            UniqueCommandTypes = sessionStats.CommandCounts.Count,
            UniqueNotificationTypes = sessionStats.NotificationCounts.Count,
            TotalQueryExecutions = sessionStats.QueryCounts.Values.Sum(),
            TotalCommandExecutions = sessionStats.CommandCounts.Values.Sum(),
            TotalNotificationExecutions = sessionStats.NotificationCounts.Values.Sum(),
            QueryTypes = sessionStats.QueryCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CommandTypes = sessionStats.CommandCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            NotificationTypes = sessionStats.NotificationCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            LastActivity = sessionStats.LastActivity
        };
    }

    /// <summary>
    /// Gets all active sessions summary.
    /// </summary>
    /// <returns>List of all session summaries.</returns>
    public List<SessionStatisticsSummary> GetAllSessionStatistics()
    {
        return _sessionStatistics.Select(kvp => new SessionStatisticsSummary
        {
            SessionId = kvp.Key,
            UniqueQueryTypes = kvp.Value.QueryCounts.Count,
            UniqueCommandTypes = kvp.Value.CommandCounts.Count,
            UniqueNotificationTypes = kvp.Value.NotificationCounts.Count,
            TotalQueryExecutions = kvp.Value.QueryCounts.Values.Sum(),
            TotalCommandExecutions = kvp.Value.CommandCounts.Values.Sum(),
            TotalNotificationExecutions = kvp.Value.NotificationCounts.Values.Sum(),
            QueryTypes = kvp.Value.QueryCounts.ToDictionary(x => x.Key, x => x.Value),
            CommandTypes = kvp.Value.CommandCounts.ToDictionary(x => x.Key, x => x.Value),
            NotificationTypes = kvp.Value.NotificationCounts.ToDictionary(x => x.Key, x => x.Value),
            LastActivity = kvp.Value.LastActivity
        }).ToList();
    }

    /// <summary>
    /// Cleans up inactive sessions older than the specified timespan.
    /// </summary>
    /// <param name="maxAge">Maximum age for sessions to keep.</param>
    public void CleanupInactiveSessions(TimeSpan maxAge)
    {
        var cutoffTime = DateTime.UtcNow - maxAge;
        var inactiveSessionIds = _sessionStatistics
            .Where(kvp => kvp.Value.LastActivity < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in inactiveSessionIds)
        {
            _sessionStatistics.TryRemove(sessionId, out _);
        }
    }
}