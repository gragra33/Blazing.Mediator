using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

public class GetLowStockProductsQuery : IRequest<List<ProductDto>>
{
    public int Threshold { get; set; } = 10;
}