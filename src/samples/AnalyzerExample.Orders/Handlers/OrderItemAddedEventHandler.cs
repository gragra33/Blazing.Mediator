using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order item added events
/// </summary>
public class OrderItemAddedEventHandler : INotificationHandler<OrderItemAddedEvent>
{
    private readonly ILogger<OrderItemAddedEventHandler> _logger;

    public OrderItemAddedEventHandler(ILogger<OrderItemAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderItemAddedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Item added to order {OrderId}: Product {ProductName} (Qty: {Quantity}) by {AddedBy}",
            notification.OrderId, notification.ProductName, notification.Quantity, notification.AddedBy);

        await UpdateOrderTotal(notification, cancellationToken);
        await CheckInventoryAvailability(notification, cancellationToken);
        await UpdateRecommendations(notification, cancellationToken);
    }

    private async Task UpdateOrderTotal(OrderItemAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating order total for order {OrderId}", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }

    private async Task CheckInventoryAvailability(OrderItemAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking inventory availability for product {ProductId}", notification.ProductId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateRecommendations(OrderItemAddedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating product recommendations based on added item {ProductId}", notification.ProductId);
        await Task.Delay(15, cancellationToken);
    }
}