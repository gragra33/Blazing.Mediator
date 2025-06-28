using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IMediator mediator, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var query = new GetOrderByIdQuery { OrderId = id };
        var order = await mediator.Send(query);
        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? customerId = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            CustomerId = customerId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(
        int customerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetCustomerOrdersQuery
        {
            CustomerId = customerId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var orders = await mediator.Send(query);
        return Ok(orders);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new GetOrderStatisticsQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        var statistics = await mediator.Send(query);
        return Ok(statistics);
    }

    [HttpPost]
    public async Task<ActionResult<OperationResult<int>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

            var result = await mediator.Send(command);

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

    [HttpPost("process")]
    public async Task<ActionResult<OperationResult<ProcessOrderResponse>>> ProcessOrder([FromBody] ProcessOrderCommand command)
    {
        try
        {
            logger.LogInformation("Processing complete order for customer {CustomerId}", command.CustomerId);

            var result = await mediator.Send(command);

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

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusCommand command)
    {
        command.OrderId = id;
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<OperationResult<bool>>> CancelOrder(int id, [FromBody] CancelOrderCommand command)
    {
        command.OrderId = id;
        var result = await mediator.Send(command);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}