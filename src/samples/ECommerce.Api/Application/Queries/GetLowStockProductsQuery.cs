using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;

namespace ECommerce.Api.Application.Queries;

/// <summary>
/// Query to retrieve products that have stock levels below a specified threshold.
/// </summary>
public class GetLowStockProductsQuery : IRequest<List<ProductDto>>
{
    /// <summary>
    /// Gets or sets the stock threshold below which products are considered low stock (default: 10).
    /// </summary>
    public int Threshold { get; set; } = 10;
}