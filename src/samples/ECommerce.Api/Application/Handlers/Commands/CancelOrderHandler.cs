using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

public class CancelOrderHandler(ECommerceDbContext context) : IRequestHandler<CancelOrderCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            Order? order = await context.Orders.FindAsync(request.OrderId);
            if (order == null)
                return OperationResult<bool>.ErrorResult($"Order with ID {request.OrderId} not found");

            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
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