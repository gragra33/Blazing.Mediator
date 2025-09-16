namespace UserManagement.Api.Models;

/// <summary>
/// Session statistics summary.
/// </summary>
public class SessionStatisticsSummary
{
    /// <summary>
    /// Gets or sets the unique identifier for the session.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the number of unique query types executed in this session.
    /// </summary>
    public int UniqueQueryTypes { get; set; }
    
    /// <summary>
    /// Gets or sets the number of unique command types executed in this session.
    /// </summary>
    public int UniqueCommandTypes { get; set; }
    
    /// <summary>
    /// Gets or sets the number of unique notification types published in this session.
    /// </summary>
    public int UniqueNotificationTypes { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of query executions in this session.
    /// </summary>
    public long TotalQueryExecutions { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of command executions in this session.
    /// </summary>
    public long TotalCommandExecutions { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of notification publications in this session.
    /// </summary>
    public long TotalNotificationExecutions { get; set; }
    
    /// <summary>
    /// Gets or sets the dictionary of query types and their execution counts for this session.
    /// </summary>
    public Dictionary<string, long> QueryTypes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary of command types and their execution counts for this session.
    /// </summary>
    public Dictionary<string, long> CommandTypes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the dictionary of notification types and their publication counts for this session.
    /// </summary>
    public Dictionary<string, long> NotificationTypes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the timestamp of the last activity in this session.
    /// </summary>
    public DateTime LastActivity { get; set; }
}