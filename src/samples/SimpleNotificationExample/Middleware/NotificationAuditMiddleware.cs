using Blazing.Mediator.Abstractions;

namespace SimpleNotificationExample.Middleware;

/// <summary>
/// Middleware that audits notification processing for compliance and security.
/// This demonstrates how to add auditing capabilities to notifications.
/// </summary>
public class NotificationAuditMiddleware(ILogger<NotificationAuditMiddleware> logger)
    : INotificationMiddleware
{
    public int Order => 20; // Execute after metrics

    private static readonly List<AuditEntry> _auditLog = [];
    private static readonly object _lock = new();

    public async Task InvokeAsync<TNotification>(TNotification notification,
        NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var auditEntry = new AuditEntry
        {
            NotificationType = typeof(TNotification).Name,
            Timestamp = DateTime.UtcNow,
            NotificationData = notification?.ToString() ?? "null",
            Status = "Processing"
        };

        lock (_lock)
        {
            _auditLog.Add(auditEntry);
        }

        logger.LogInformation("& Audit: Started processing {NotificationType} at {Timestamp}", 
            auditEntry.NotificationType, auditEntry.Timestamp);

        try
        {
            await next(notification, cancellationToken);
            
            auditEntry.Status = "Completed";
            auditEntry.CompletedAt = DateTime.UtcNow;
            auditEntry.Duration = (auditEntry.CompletedAt.Value - auditEntry.Timestamp).TotalMilliseconds;

            logger.LogInformation("& Audit: Completed {NotificationType} in {Duration}ms", 
                auditEntry.NotificationType, auditEntry.Duration);
        }
        catch (Exception ex)
        {
            auditEntry.Status = "Failed";
            auditEntry.CompletedAt = DateTime.UtcNow;
            auditEntry.Duration = (auditEntry.CompletedAt.Value - auditEntry.Timestamp).TotalMilliseconds;
            auditEntry.ErrorMessage = ex.Message;

            logger.LogWarning("& Audit: Failed {NotificationType} after {Duration}ms - {Error}", 
                auditEntry.NotificationType, auditEntry.Duration, ex.Message);
            throw;
        }
    }

    public static List<AuditEntry> GetAuditLog()
    {
        lock (_lock)
        {
            return new List<AuditEntry>(_auditLog);
        }
    }

    public class AuditEntry
    {
        public string NotificationType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string NotificationData { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Duration { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
