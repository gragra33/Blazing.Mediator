using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving products that have stock levels below a specified threshold.
/// </summary>
/// <param name="context">The database context for accessing product data.</param>
public class GetLowStockProductsHandler(ECommerceDbContext context)
    : IRequestHandler<GetLowStockProductsQuery, List<ProductDto>>
{
    /// <summary>
    /// Handles the get low stock products query by filtering active products below the threshold.
    /// </summary>
    /// <param name="request">The query containing the stock threshold.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of products with low stock levels, ordered by stock quantity and name.</returns>
    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken = default)
    {
        List<Product> products = await context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity <= request.Threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return products.ToDto();
    }
}