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
        GetOrderByIdQuery? query = new GetOrderByIdQuery { OrderId = id };
        OrderDto? order = await mediator.Send(query);
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
        GetOrdersQuery? query = new GetOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            CustomerId = customerId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        PagedResult<OrderDto>? result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(
        int customerId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        GetCustomerOrdersQuery? query = new GetCustomerOrdersQuery
        {
            CustomerId = customerId,
            FromDate = fromDate,
            ToDate = toDate
        };

        List<OrderDto>? orders = await mediator.Send(query);
        return Ok(orders);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        GetOrderStatisticsQuery? query = new GetOrderStatisticsQuery
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        OrderStatisticsDto? statistics = await mediator.Send(query);
        return Ok(statistics);
    }

    [HttpPost]
    public async Task<ActionResult<OperationResult<int>>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            logger.LogInformation("Creating order for customer {CustomerId}", command.CustomerId);

            OperationResult<int>? result = await mediator.Send(command);

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

            OperationResult<ProcessOrderResponse>? result = await mediator.Send(command);

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
        OperationResult<bool>? result = await mediator.Send(command);

        if (result.Success)
            return Ok(result);
        else
            return BadRequest(result);
    }
}