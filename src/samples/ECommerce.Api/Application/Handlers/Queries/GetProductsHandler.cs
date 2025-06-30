using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

public class GetProductsHandler(ECommerceDbContext context) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<Product>? query = context.Products.AsNoTracking();

        if (request.ActiveOnly)
            query = query.Where(p => p.IsActive);

        if (request.InStockOnly)
            query = query.Where(p => p.StockQuantity > 0);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string? searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Product>? products = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return products.ToPagedDto(totalCount, request.Page, request.PageSize);
    }
}