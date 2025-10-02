using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

public class GlobalNotificationMiddleware : INotificationMiddleware
{
    public static int Order => int.MinValue + 500;
    
    private readonly ILogger<GlobalNotificationMiddleware> _logger;

    public GlobalNotificationMiddleware(ILogger<GlobalNotificationMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken) 
        where TNotification : INotification
    {
        var notificationType = typeof(TNotification).Name;
        _logger.LogInformation("?? [Global Notification] Processing {NotificationType}", notificationType);
        
        try
        {
            await next(notification, cancellationToken);
            _logger.LogInformation("?? [Global Notification] Completed {NotificationType}", notificationType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? [Global Notification] Failed {NotificationType}", notificationType);
            throw;
        }
    }
}