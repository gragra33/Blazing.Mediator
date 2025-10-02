using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order cancellation events
/// </summary>
public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} cancelled. Reason: {CancellationReason}", 
            notification.OrderId, notification.CancellationReason);

        await ProcessRefund(notification, cancellationToken);
        await ReleaseInventory(notification, cancellationToken);
        await UpdateCustomerHistory(notification, cancellationToken);
    }

    private async Task ProcessRefund(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing refund for cancelled order {OrderId}", notification.OrderId);
        await Task.Delay(40, cancellationToken);
    }

    private async Task ReleaseInventory(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Releasing inventory for cancelled order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task UpdateCustomerHistory(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating customer history for cancelled order {OrderId}", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }
}