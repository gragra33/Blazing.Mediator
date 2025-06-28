using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Handlers.Commands;

public class ProcessOrderHandler(IMediator mediator)
    : IRequestHandler<ProcessOrderCommand, OperationResult<ProcessOrderResponse>>
{
    public async Task<OperationResult<ProcessOrderResponse>> Handle(ProcessOrderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Create the order
            var createOrderCommand = new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                ShippingAddress = request.ShippingAddress,
                Items = request.Items
            };

            var orderResult = await mediator.Send(createOrderCommand, cancellationToken);
            if (!orderResult.Success || orderResult.Data <= 0)
            {
                return OperationResult<ProcessOrderResponse>.ErrorResult(orderResult.Message, orderResult.Errors);
            }

            var orderId = orderResult.Data;

            // Step 2: Update order status to confirmed
            var updateStatusCommand = new UpdateOrderStatusCommand
            {
                OrderId = orderId,
                Status = OrderStatus.Confirmed
            };
            await mediator.Send(updateStatusCommand, cancellationToken);

            // Step 3: Simulate payment processing (would be real payment gateway in production)
            await Task.Delay(100, cancellationToken); // Simulate processing time

            return OperationResult<ProcessOrderResponse>.SuccessResult(
                new ProcessOrderResponse
                {
                    OrderId = orderId,
                    OrderNumber = $"ORD-{orderId:D6}",
                    TotalAmount = 0, // Would be calculated from order
                    CreatedAt = DateTime.UtcNow
                },
                "Order processed successfully");
        }
        catch (Exception ex)
        {
            return OperationResult<ProcessOrderResponse>.ErrorResult($"Error processing order: {ex.Message}");
        }
    }
}