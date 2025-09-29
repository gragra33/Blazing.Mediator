using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Test notification metrics middleware for unit tests.
/// </summary>
public class NotificationMetricsMiddleware(ILogger<NotificationMetricsMiddleware>? logger = null)
    : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var startTime = DateTime.UtcNow;
        logger?.LogInformation("Metrics start for: {NotificationType}", typeof(TNotification).Name);
        
        try
        {
            await next(notification, cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            logger?.LogInformation("Metrics completed for: {NotificationType} in {Duration}ms", typeof(TNotification).Name, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            logger?.LogError(ex, "Metrics failed for: {NotificationType} in {Duration}ms", typeof(TNotification).Name, duration.TotalMilliseconds);
            throw;
        }
    }
}