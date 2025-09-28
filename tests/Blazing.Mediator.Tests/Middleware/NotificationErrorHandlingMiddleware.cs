using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Test notification error handling middleware for unit tests.
/// </summary>
public class NotificationErrorHandlingMiddleware(ILogger<NotificationErrorHandlingMiddleware>? logger = null)
    : INotificationMiddleware
{
    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        try
        {
            logger?.LogInformation("Error handling middleware for: {NotificationType}", typeof(TNotification).Name);
            await next(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error handled in middleware for: {NotificationType}", typeof(TNotification).Name);
            
            // In a real scenario, you might transform exceptions, log to external systems, etc.
            // For testing purposes, we'll re-throw to maintain the error flow
            throw;
        }
    }
}