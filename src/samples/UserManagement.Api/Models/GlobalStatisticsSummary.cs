namespace UserManagement.Api.Models;

/// <summary>
/// Global statistics summary.
/// </summary>
public class GlobalStatisticsSummary
{
    /// <summary>
    /// Gets or sets the number of unique query types that have been executed.
    /// </summary>
    public int UniqueQueryTypes { get; set; }

    /// <summary>
    /// Gets or sets the number of unique command types that have been executed.
    /// </summary>
    public int UniqueCommandTypes { get; set; }

    /// <summary>
    /// Gets or sets the number of unique notification types that have been published.
    /// </summary>
    public int UniqueNotificationTypes { get; set; }

    /// <summary>
    /// Gets or sets the total number of query executions across all sessions.
    /// </summary>
    public long TotalQueryExecutions { get; set; }

    /// <summary>
    /// Gets or sets the total number of command executions across all sessions.
    /// </summary>
    public long TotalCommandExecutions { get; set; }

    /// <summary>
    /// Gets or sets the total number of notification publications across all sessions.
    /// </summary>
    public long TotalNotificationExecutions { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of query types and their execution counts.
    /// </summary>
    public Dictionary<string, long> QueryTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the dictionary of command types and their execution counts.
    /// </summary>
    public Dictionary<string, long> CommandTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the dictionary of notification types and their publication counts.
    /// </summary>
    public Dictionary<string, long> NotificationTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of currently active sessions.
    /// </summary>
    public int ActiveSessions { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when these statistics were last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}