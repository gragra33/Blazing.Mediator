using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

public class GetOrderByIdQuery : IRequest<OrderDto>
{
    public int OrderId { get; set; }
}