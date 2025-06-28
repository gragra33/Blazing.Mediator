using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Commands;

public class CreateOrderCommand : IRequest<OperationResult<int>>
{
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = [];
}