using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for canceling an existing order with validation checks.
/// </summary>
/// <param name="context">The database context for accessing order data.</param>
public class CancelOrderHandler(ECommerceDbContext context) : IRequestHandler<CancelOrderCommand, OperationResult<bool>>
{
    /// <summary>
    /// Handles the cancel order command by validating the order status and updating it to cancelled.
    /// </summary>
    /// <param name="request">The command containing the order ID and cancellation reason.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result indicating success or failure of the cancellation.</returns>
    public async Task<OperationResult<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            Order? order = await context.Orders.FindAsync([request.OrderId], cancellationToken); // Forwarding the cancellationToken parameter
            if (order == null)
                return OperationResult<bool>.ErrorResult($"Order with ID {request.OrderId} not found");

            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                return OperationResult<bool>.ErrorResult($"Order cannot be cancelled. Current status: {order.Status}");

            order.UpdateStatus(OrderStatus.Cancelled);
            await context.SaveChangesAsync(cancellationToken);

            return OperationResult<bool>.SuccessResult(true, "Order cancelled successfully");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.ErrorResult($"Error cancelling order: {ex.Message}");
        }
    }
}