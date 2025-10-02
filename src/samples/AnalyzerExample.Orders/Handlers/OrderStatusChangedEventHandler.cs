using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order status change events
/// </summary>
public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(ILogger<OrderStatusChangedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderId} status changed from {FromStatus} to {ToStatus}", 
            notification.OrderId, notification.FromStatus, notification.ToStatus);

        await UpdateCustomerNotificationPreferences(notification, cancellationToken);
        await TriggerWorkflowTransitions(notification, cancellationToken);
    }

    private async Task UpdateCustomerNotificationPreferences(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating customer notification preferences for order {OrderId}", notification.OrderId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task TriggerWorkflowTransitions(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Triggering workflow transitions for order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }
}