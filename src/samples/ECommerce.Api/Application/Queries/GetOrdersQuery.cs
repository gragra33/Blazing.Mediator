using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Queries;

public class GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? CustomerId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}