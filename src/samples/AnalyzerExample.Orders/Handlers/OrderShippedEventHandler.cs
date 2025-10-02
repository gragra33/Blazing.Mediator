using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order shipping events
/// </summary>
public class OrderShippedEventHandler : INotificationHandler<OrderShippedEvent>
{
    private readonly ILogger<OrderShippedEventHandler> _logger;

    public OrderShippedEventHandler(ILogger<OrderShippedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} shipped with tracking {TrackingNumber}", 
            notification.OrderId, notification.TrackingNumber);

        await UpdateInventoryLevels(notification, cancellationToken);
        await InitializeTrackingSystem(notification, cancellationToken);
    }

    private async Task UpdateInventoryLevels(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating inventory levels for shipped order {OrderId}", notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task InitializeTrackingSystem(OrderShippedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing tracking for {TrackingNumber}", notification.TrackingNumber);
        await Task.Delay(15, cancellationToken);
    }
}