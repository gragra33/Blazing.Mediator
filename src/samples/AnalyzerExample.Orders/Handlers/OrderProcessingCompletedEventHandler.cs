using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order processing completed events
/// </summary>
public class OrderProcessingCompletedEventHandler : INotificationHandler<OrderProcessingCompletedEvent>
{
    private readonly ILogger<OrderProcessingCompletedEventHandler> _logger;

    public OrderProcessingCompletedEventHandler(ILogger<OrderProcessingCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderProcessingCompletedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order processing completed for order {OrderId} ({OrderNumber}). Final status: {FinalStatus}, Total: {TotalAmount:C}",
            notification.OrderId, notification.OrderNumber, notification.FinalStatus, notification.TotalAmount);

        await NotifyCustomer(notification, cancellationToken);
        await UpdateAnalytics(notification, cancellationToken);
        await TriggerShipping(notification, cancellationToken);
    }

    private async Task NotifyCustomer(OrderProcessingCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying customer {UserEmail} that order {OrderId} processing is complete", 
            notification.UserEmail, notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateAnalytics(OrderProcessingCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating analytics for completed order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task TriggerShipping(OrderProcessingCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Triggering shipping process for order {OrderId}", notification.OrderId);
        await Task.Delay(30, cancellationToken);
    }
}