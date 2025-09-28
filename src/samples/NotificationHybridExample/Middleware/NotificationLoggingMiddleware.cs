namespace NotificationHybridExample.Middleware;

/// <summary>
/// Notification logging middleware for hybrid pattern demonstration.
/// Logs all notifications passing through the pipeline.
/// </summary>
public class NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware> logger) 
    : INotificationMiddleware
{
    public int Order => 100; // Execute first

    public async Task InvokeAsync<TNotification>(
        TNotification notification,
        NotificationDelegate<TNotification> next,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        var startTime = DateTime.UtcNow;

        logger.LogInformation("=== [MIDDLEWARE] Notification Pipeline Started ===");
        logger.LogInformation("   Notification Type: {NotificationType}", notificationType);
        logger.LogInformation("   Start Time: {StartTime:HH:mm:ss.fff}", startTime);

        try
        {
            await next(notification, cancellationToken);
            
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("=== [MIDDLEWARE] Notification Pipeline Completed ===");
            logger.LogInformation("   Duration: {Duration} ms", duration.TotalMilliseconds);
            logger.LogInformation("   Status: SUCCESS");
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger.LogError(ex, "=== [MIDDLEWARE] Notification Pipeline Failed ===");
            logger.LogError("   Duration: {Duration} ms", duration.TotalMilliseconds);
            logger.LogError("   Status: FAILED - {ErrorMessage}", ex.Message);
            throw;
        }
    }
}