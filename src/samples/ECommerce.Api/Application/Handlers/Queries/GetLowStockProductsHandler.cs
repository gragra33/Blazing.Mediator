using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

public class GetLowStockProductsHandler(ECommerceDbContext context)
    : IRequestHandler<GetLowStockProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken cancellationToken = default)
    {
        var products = await context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity <= request.Threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return products.ToDto();
    }
}