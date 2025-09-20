using System.Collections.Concurrent;

namespace UserManagement.Api.Models;

/// <summary>
/// Statistics for a specific session.
/// </summary>
public class SessionStatistics
{
    /// <summary>
    /// Gets the query execution counts by query type for this session.
    /// </summary>
    public ConcurrentDictionary<string, long> QueryCounts { get; } = new();

    /// <summary>
    /// Gets the command execution counts by command type for this session.
    /// </summary>
    public ConcurrentDictionary<string, long> CommandCounts { get; } = new();

    /// <summary>
    /// Gets the notification publication counts by notification type for this session.
    /// </summary>
    public ConcurrentDictionary<string, long> NotificationCounts { get; } = new();

    /// <summary>
    /// Gets or sets the last activity timestamp for this session.
    /// </summary>
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}