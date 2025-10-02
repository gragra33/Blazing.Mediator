using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Middleware;

public class DomainEventMiddleware<TNotification> : INotificationMiddleware<TNotification>
    where TNotification : IDomainEvent
{
    public static int Order => 200;
    
    private readonly ILogger<DomainEventMiddleware<TNotification>> _logger;

    public DomainEventMiddleware(ILogger<DomainEventMiddleware<TNotification>> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("??? [Domain Event] Processing {EventType} v{Version} at {OccurredAt}",
            notification.EventType, notification.Version, notification.OccurredAt);
            
        await next(notification, cancellationToken);
        
        _logger.LogInformation("??? [Domain Event] Completed {EventType}", notification.EventType);
    }

    public async Task InvokeAsync<TNotificationGeneric>(TNotificationGeneric notification, NotificationDelegate<TNotificationGeneric> next, CancellationToken cancellationToken) where TNotificationGeneric : INotification
    {
        if (notification is TNotification domainEvent)
        {
            var typedNext = new NotificationDelegate<TNotification>((n, ct) => next((TNotificationGeneric)(object)n, ct));
            await InvokeAsync(domainEvent, typedNext, cancellationToken);
        }
        else
        {
            await next(notification, cancellationToken);
        }
    }
}