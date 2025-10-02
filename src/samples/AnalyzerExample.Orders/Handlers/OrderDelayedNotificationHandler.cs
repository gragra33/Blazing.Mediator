using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order delay notifications
/// </summary>
public class OrderDelayedNotificationHandler : INotificationHandler<OrderDelayedNotification>
{
    private readonly ILogger<OrderDelayedNotificationHandler> _logger;

    public OrderDelayedNotificationHandler(ILogger<OrderDelayedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderDelayedNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Order {OrderId} ({OrderNumber}) is delayed. Expected: {ExpectedDate}, Actual: {ActualDate}, Reason: {Reason}",
            notification.OrderId, notification.OrderNumber, notification.ExpectedDate, 
            notification.ActualDate, notification.DelayReason);

        await NotifyCustomerOfDelay(notification, cancellationToken);
        await UpdateDeliverySchedule(notification, cancellationToken);
        await EscalateToManager(notification, cancellationToken);
    }

    private async Task NotifyCustomerOfDelay(OrderDelayedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending delay notification to customer {UserEmail} for order {OrderId}", 
            notification.UserEmail, notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateDeliverySchedule(OrderDelayedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating delivery schedule for order {OrderId}", notification.OrderId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task EscalateToManager(OrderDelayedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Escalating delayed order {OrderId} to management", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }
}