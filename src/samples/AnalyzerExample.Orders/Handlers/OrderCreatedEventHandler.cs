using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for order creation events - handles order-specific business logic
/// </summary>
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing order creation for Order {OrderId}", notification.OrderId);

        // Simulate order processing business logic
        await InitializeOrderProcessing(notification, cancellationToken);
        await ReserveInventory(notification, cancellationToken);
        await UpdateOrderStatus(notification, cancellationToken);
    }

    private async Task InitializeOrderProcessing(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing order processing for {OrderId}", notification.OrderId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task ReserveInventory(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Reserving inventory for order {OrderId}", notification.OrderId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateOrderStatus(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating order status for {OrderId}", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }
}