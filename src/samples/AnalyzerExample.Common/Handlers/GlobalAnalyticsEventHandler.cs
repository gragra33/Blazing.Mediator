using AnalyzerExample.Common.Interfaces;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Common.Handlers;

/// <summary>
/// Global analytics handler that tracks metrics for all integration events
/// This demonstrates handling events through shared interfaces
/// </summary>
public class GlobalAnalyticsEventHandler : INotificationHandler<IIntegrationEvent>
{
    private readonly ILogger<GlobalAnalyticsEventHandler> _logger;

    public GlobalAnalyticsEventHandler(ILogger<GlobalAnalyticsEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(IIntegrationEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ANALYTICS: Integration event {EventType} tracked at {Timestamp}", 
            notification.GetType().Name, DateTime.UtcNow);

        await SimulateAnalyticsTracking(notification, cancellationToken);
    }

    private async Task SimulateAnalyticsTracking(IIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // Simulate sending to analytics service
        _logger.LogDebug("Tracking analytics for {EventType}", notification.GetType().Name);
        await Task.Delay(15, cancellationToken);
    }
}