using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Commands;

public class ProcessOrderCommand : IRequest<OperationResult<ProcessOrderResponse>>
{
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItemRequest> Items { get; set; } = [];
    public string PaymentMethod { get; set; } = string.Empty;
}