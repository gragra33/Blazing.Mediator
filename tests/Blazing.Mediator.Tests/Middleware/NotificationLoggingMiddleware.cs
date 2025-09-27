using Blazing.Mediator;
using Blazing.Mediator.Abstractions;
using Microsoft.Extensions.Logging;

namespace Blazing.Mediator.Tests.Middleware;

/// <summary>
/// Test notification logging middleware for unit tests.
/// </summary>
public class NotificationLoggingMiddleware : INotificationMiddleware
{
    private readonly ILogger<NotificationLoggingMiddleware>? _logger;

    public NotificationLoggingMiddleware(ILogger<NotificationLoggingMiddleware>? logger = null)
    {
        _logger = logger;
    }

    public async Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        _logger?.LogInformation("Processing notification: {NotificationType}", typeof(TNotification).Name);
        
        await next(notification, cancellationToken);
        
        _logger?.LogInformation("Completed notification: {NotificationType}", typeof(TNotification).Name);
    }
}