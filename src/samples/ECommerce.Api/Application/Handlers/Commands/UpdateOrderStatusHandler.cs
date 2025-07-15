using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Notifications;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for updating the status of an existing order.
/// </summary>
/// <param name="context">The database context for accessing order data.</param>
/// <param name="mediator">The mediator for publishing notifications.</param>
public class UpdateOrderStatusHandler(ECommerceDbContext context, IMediator mediator) : IRequestHandler<UpdateOrderStatusCommand>
{
    /// <summary>
    /// Handles the update order status command by finding the order and updating its status.
    /// </summary>
    /// <param name="request">The command containing the order ID and new status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found.</exception>
    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Orders.FindAsync([request.OrderId], cancellationToken);
        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        var previousStatus = order.Status;
        order.UpdateStatus(request.Status);
        await context.SaveChangesAsync(cancellationToken);

        // Publish order status changed notification
        var orderStatusChangedNotification = new OrderStatusChangedNotification(
            order.Id,
            order.CustomerId,
            order.CustomerEmail,
            previousStatus,
            request.Status,
            order.TotalAmount
        );

        await mediator.Publish(orderStatusChangedNotification, cancellationToken);
    }
}