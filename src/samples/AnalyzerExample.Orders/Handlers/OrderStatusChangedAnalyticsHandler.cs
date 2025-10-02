using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Analytics handler for order status change events
/// </summary>
public class OrderStatusChangedAnalyticsHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedAnalyticsHandler> _logger;

    public OrderStatusChangedAnalyticsHandler(ILogger<OrderStatusChangedAnalyticsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing analytics for order {OrderId} status change: {FromStatus} ? {ToStatus}", 
            notification.OrderId, notification.FromStatus, notification.ToStatus);

        await UpdateOrderMetrics(notification, cancellationToken);
        await TrackStatusTransitionTime(notification, cancellationToken);
        await UpdateCustomerBehaviorAnalytics(notification, cancellationToken);
    }

    private async Task UpdateOrderMetrics(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating order metrics for status change to {ToStatus}", notification.ToStatus);
        await Task.Delay(30, cancellationToken);
    }

    private async Task TrackStatusTransitionTime(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Tracking status transition time for order {OrderId}", notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateCustomerBehaviorAnalytics(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating customer behavior analytics for order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }
}