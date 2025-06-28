using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

public class GetOrderStatisticsHandler(ECommerceDbContext context)
    : IRequestHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
{
    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken = default)
    {
        var query = context.Orders.AsNoTracking();

        if (request.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

        var orders = await query.ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var pendingOrders = orders.Count(o => o.Status == Domain.Entities.OrderStatus.Pending);
        var completedOrders = orders.Count(o => o.Status == Domain.Entities.OrderStatus.Delivered);
        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        // Get top products
        var topProducts = await context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantitySold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new OrderStatisticsDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            CompletedOrders = completedOrders,
            AverageOrderValue = averageOrderValue,
            TopProducts = topProducts
        };
    }
}