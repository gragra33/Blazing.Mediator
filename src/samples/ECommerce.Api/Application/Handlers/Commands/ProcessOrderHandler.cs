using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for processing a complete order including creation, validation, and payment processing.
/// </summary>
/// <param name="mediator">The mediator for sending commands and queries.</param>
public class ProcessOrderHandler(IMediator mediator)
    : IRequestHandler<ProcessOrderCommand, OperationResult<ProcessOrderResponse>>
{
    /// <summary>
    /// Handles the process order command by orchestrating order creation and processing steps.
    /// </summary>
    /// <param name="request">The command containing order processing details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The operation result with processing response containing order details.</returns>
    public async Task<OperationResult<ProcessOrderResponse>> Handle(ProcessOrderCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Create the order
            CreateOrderCommand createOrderCommand = new()
            {
                CustomerId = request.CustomerId,
                CustomerEmail = request.CustomerEmail,
                ShippingAddress = request.ShippingAddress,
                Items = request.Items
            };

            OperationResult<int> orderResult = await mediator.Send(createOrderCommand, cancellationToken);
            if (!orderResult.Success || orderResult.Data <= 0)
            {
                return OperationResult<ProcessOrderResponse>.ErrorResult(orderResult.Message, orderResult.Errors);
            }

            int orderId = orderResult.Data;

            // Step 2: Update order status to confirmed
            UpdateOrderStatusCommand updateStatusCommand = new()
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