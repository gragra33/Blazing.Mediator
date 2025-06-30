using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

public class GetCustomerOrdersHandler(ECommerceDbContext context)
    : IRequestHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken = default)
    {
        List<Order>? orders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.CustomerId == request.CustomerId)
            .Where(o => !request.FromDate.HasValue || o.CreatedAt >= request.FromDate.Value)
            .Where(o => !request.ToDate.HasValue || o.CreatedAt <= request.ToDate.Value)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.ToDto();
    }
}