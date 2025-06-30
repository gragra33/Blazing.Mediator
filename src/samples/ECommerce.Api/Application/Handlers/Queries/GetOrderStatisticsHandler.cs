using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

public class GetOrderStatisticsHandler(ECommerceDbContext context)
    : IRequestHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
{
    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<Order>? query = context.Orders.AsNoTracking();

        if (request.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

        List<Order>? orders = await query.ToListAsync(cancellationToken);

        int totalOrders = orders.Count;
        decimal totalRevenue = orders.Sum(o => o.TotalAmount);
        int pendingOrders = orders.Count(o => o.Status == Domain.Entities.OrderStatus.Pending);
        int completedOrders = orders.Count(o => o.Status == Domain.Entities.OrderStatus.Delivered);
        decimal averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        // Get top products
        List<ProductSalesDto>? topProducts = await context.OrderItems
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