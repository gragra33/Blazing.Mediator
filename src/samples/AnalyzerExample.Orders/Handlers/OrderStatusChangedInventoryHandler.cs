using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Inventory handler for order status change events
/// </summary>
public class OrderStatusChangedInventoryHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedInventoryHandler> _logger;

    public OrderStatusChangedInventoryHandler(ILogger<OrderStatusChangedInventoryHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing inventory updates for order {OrderId} status change: {FromStatus} ? {ToStatus}", 
            notification.OrderId, notification.FromStatus, notification.ToStatus);

        await UpdateInventoryReservations(notification, cancellationToken);
        await AdjustStockLevels(notification, cancellationToken);
        await UpdateSupplierNotifications(notification, cancellationToken);
    }

    private async Task UpdateInventoryReservations(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating inventory reservations for order {OrderId}", notification.OrderId);
        await Task.Delay(35, cancellationToken);
    }

    private async Task AdjustStockLevels(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Adjusting stock levels based on status change to {ToStatus}", notification.ToStatus);
        await Task.Delay(40, cancellationToken);
    }

    private async Task UpdateSupplierNotifications(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating supplier notifications for order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }
}