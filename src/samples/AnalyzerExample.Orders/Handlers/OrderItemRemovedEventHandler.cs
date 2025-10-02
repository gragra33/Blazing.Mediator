using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order item removed events
/// </summary>
public class OrderItemRemovedEventHandler : INotificationHandler<OrderItemRemovedEvent>
{
    private readonly ILogger<OrderItemRemovedEventHandler> _logger;

    public OrderItemRemovedEventHandler(ILogger<OrderItemRemovedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderItemRemovedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Item removed from order {OrderId}: Product {ProductName} by {RemovedBy}. Reason: {Reason}",
            notification.OrderId, notification.ProductName, notification.RemovedBy, notification.Reason);

        await UpdateOrderTotal(notification, cancellationToken);
        await ReleaseInventoryReservation(notification, cancellationToken);
        await UpdateRecommendations(notification, cancellationToken);
    }

    private async Task UpdateOrderTotal(OrderItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating order total for order {OrderId}", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }

    private async Task ReleaseInventoryReservation(OrderItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Releasing inventory reservation for product {ProductId}", notification.ProductId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task UpdateRecommendations(OrderItemRemovedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating product recommendations after item removal for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }
}