using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order delivered events
/// </summary>
public class OrderDeliveredEventHandler : INotificationHandler<OrderDeliveredEvent>
{
    private readonly ILogger<OrderDeliveredEventHandler> _logger;

    public OrderDeliveredEventHandler(ILogger<OrderDeliveredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderDeliveredEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} ({OrderNumber}) has been delivered at {DeliveredAt}",
            notification.OrderId, notification.OrderNumber, notification.DeliveredAt);

        await SendDeliveryConfirmation(notification, cancellationToken);
        await RequestFeedback(notification, cancellationToken);
        await UpdateInventoryReports(notification, cancellationToken);
    }

    private async Task SendDeliveryConfirmation(OrderDeliveredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending delivery confirmation to {UserEmail} for order {OrderId}", 
            notification.UserEmail, notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task RequestFeedback(OrderDeliveredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Scheduling feedback request for order {OrderId}", notification.OrderId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task UpdateInventoryReports(OrderDeliveredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating inventory reports for delivered order {OrderId}", notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }
}