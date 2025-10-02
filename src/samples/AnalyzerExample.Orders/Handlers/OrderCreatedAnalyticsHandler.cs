using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Additional analytics handler for order created events
/// </summary>
public class OrderCreatedAnalyticsHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedAnalyticsHandler> _logger;

    public OrderCreatedAnalyticsHandler(ILogger<OrderCreatedAnalyticsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing analytics for newly created order {OrderId} ({OrderNumber}) - Amount: {TotalAmount:C}", 
            notification.OrderId, notification.OrderNumber, notification.TotalAmount);

        await UpdateSalesMetrics(notification, cancellationToken);
        await TrackCustomerPurchasePattern(notification, cancellationToken);
        await UpdateRevenueForecasting(notification, cancellationToken);
    }

    private async Task UpdateSalesMetrics(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating sales metrics for new order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task TrackCustomerPurchasePattern(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Tracking purchase patterns for customer of order {OrderId}", notification.OrderId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task UpdateRevenueForecasting(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating revenue forecasting with order {OrderId} data", notification.OrderId);
        await Task.Delay(35, cancellationToken);
    }
}