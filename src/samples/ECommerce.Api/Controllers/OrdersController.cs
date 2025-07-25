using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// Controller for managing order operations in the e-commerce system.
/// Implements CQRS pattern using the Blazing.Mediator library.
/// </summary>
/// <param name="mediator">The mediator instance for handling commands and queries.</param>
/// <param name="logger">The logger instance for logging order-related operations.</param>
[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator, ILogger<OrdersController> logger) : ControllerBase
{
    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order details if found.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        GetOrderByIdQuery query = new() { OrderId = id };
        OrderDto order = await mediator.Send(query);
        return Ok(order);
    }

    /// <summary>
    /// Retrieves a paginated list of orders with optional filtering.
    /// </summary>
    /// <param name="page">The page number (default: 1).</param>
    /// <param name="pageSize">The number of items per page (default: 10).</param>
    /// <param name="customerId">Optional customer ID to filter orders.</param>
    /// <param name="status">Optional order status to filter orders.</param>
    /// <param name="fromDate">Optional start date to filter orders.</param>
    /// <param name="toDate">Optional end date to filter orders.</param>
    /// <returns>A paginated list of orders matching the criteria.</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? customerId = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        GetOrdersQuery query = new()
        {
            Page = page,
            PageSize = pageSize,
            CustomerId = customerId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        PagedResult<OrderDto> result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves all orders for a specific customer with optional date filtering.
    /// </summary>
    /// <param name="customerId">The customer ID to retrieve orders for.</param>
    /// <param name="fromDate">Optional start date to filter orders.</param>
    /// <param name="toDate">Optional end date to filter orders.</param>
    /// <returns>A list of orders for the specified customer.</returns>
    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(
        int customerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        GetCustomerOrdersQuery query = new()
        {
            CustomerId = customerId,
            FromDate = fromDate,
            ToDate = toDate
        };

        List<OrderDto> orders = await mediator.Send(query);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves order statistics with optional date filtering.
    /// </summary>
    /// <param name="fromDate">Optional start date to filter statistics.</param>
    /// <param name="toDate">Optional end date to filter statistics.</param>
    /// <returns>Order statistics for the specified date range.</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        GetOrderStatisticsQuery query = new()
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        OrderStatisticsDto statistics = await mediator.Send(query);
        return Ok(statistics);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="command">The command containing order creation details.</param>
    /// <returns>The operation result with the created order ID.</returns>
    [HttpPost]
    public async Task<ActionResult<OperationResult<int>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

            OperationResult<int> result = await mediator.Send(command);

            if (result.Success && result.Data > 0)
            {
                logger.LogInformation("Order {OrderId} created successfully", result.Data);
                return CreatedAtAction(nameof(GetOrder), new { id = result.Data }, result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order for customer {CustomerId}", command.CustomerId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Processes a complete order operation including creation and initial processing.
    /// </summary>
    /// <param name="command">The command containing order processing details.</param>
    /// <returns>The operation result with processing response.</returns>
    [HttpPost("process")]
    public async Task<ActionResult<OperationResult<ProcessOrderResponse>>> ProcessOrder([FromBody] ProcessOrderCommand command)
    {
        try
        {
            logger.LogInformation("Processing complete order for customer {CustomerId}", command.CustomerId);

            OperationResult<ProcessOrderResponse> result = await mediator.Send(command);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing order for customer {CustomerId}", command.CustomerId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    /// <param name="id">The order ID to update.</param>
    /// <param name="command">The command containing the new status details.</param>
    /// <returns>NoContent response on successful update.</returns>
    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        await mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <param name="id">The order ID to cancel.</param>
    /// <param name="command">The command containing cancellation details.</param>
    /// <returns>The operation result indicating success or failure of the cancellation.</returns>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OperationResult<bool>>> CancelOrder(int id, [FromBody] CancelOrderCommand command)
    {
        command.OrderId = id;
        OperationResult<bool> result = await mediator.Send(command);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }

    /// <summary>
    /// Completes an order by progressing through Processing -> Shipped -> Delivered status.
    /// This demonstrates the notification system with multiple status changes.
    /// </summary>
    /// <param name="id">The order ID to complete.</param>
    /// <returns>ActionResult indicating the completion status.</returns>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteOrder(int id)
    {
        try
        {
            logger.LogInformation("Starting order completion process for order {OrderId}", id);

            // Step 1: Set to Processing
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Processing });
            logger.LogInformation("Order {OrderId} set to Processing", id);

            // Step 2: Set to Shipped (simulate processing delay)
            await Task.Delay(1000);
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Shipped });
            logger.LogInformation("Order {OrderId} set to Shipped", id);

            // Step 3: Set to Delivered (simulate shipping delay)
            await Task.Delay(1000);
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Delivered });
            logger.LogInformation("Order {OrderId} set to Delivered", id);

            return Ok(new { message = "Order completed successfully", orderId = id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing order {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Processes an order through the complete workflow: Confirmed -> Processing -> Shipped -> Delivered.
    /// This demonstrates the full notification system workflow.
    /// </summary>
    /// <param name="id">The order ID to process.</param>
    /// <returns>ActionResult indicating the processing status.</returns>
    [HttpPost("{id}/process-workflow")]
    public async Task<ActionResult> ProcessOrderWorkflow(int id)
    {
        try
        {
            logger.LogInformation("Starting full order workflow for order {OrderId}", id);

            // Step 1: Confirm the order
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Confirmed });
            logger.LogInformation("Order {OrderId} confirmed", id);
            await Task.Delay(500);

            // Step 2: Start processing
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Processing });
            logger.LogInformation("Order {OrderId} processing started", id);
            await Task.Delay(1000);

            // Step 3: Ship the order
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Shipped });
            logger.LogInformation("Order {OrderId} shipped", id);
            await Task.Delay(1500);

            // Step 4: Deliver the order
            await mediator.Send(new UpdateOrderStatusCommand { OrderId = id, Status = OrderStatus.Delivered });
            logger.LogInformation("Order {OrderId} delivered", id);

            return Ok(new { 
                message = "Order workflow completed successfully", 
                orderId = id,
                finalStatus = "Delivered",
                notificationsSent = new[] { "Order Confirmed", "Processing Started", "Order Shipped", "Order Delivered" }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing order workflow for {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}