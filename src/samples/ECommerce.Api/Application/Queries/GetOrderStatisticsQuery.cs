using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

public class GetOrderStatisticsQuery : IRequest<OrderStatisticsDto>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}