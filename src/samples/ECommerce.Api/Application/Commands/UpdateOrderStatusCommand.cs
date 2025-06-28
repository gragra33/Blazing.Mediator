using Blazing.Mediator;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Commands;

public class UpdateOrderStatusCommand : IRequest
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
}