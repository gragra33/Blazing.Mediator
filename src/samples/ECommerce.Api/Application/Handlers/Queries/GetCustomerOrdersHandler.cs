using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving all orders for a specific customer with optional date filtering.
/// </summary>
/// <param name="context">The database context for accessing order data.</param>
public class GetCustomerOrdersHandler(ECommerceDbContext context)
    : IRequestHandler<GetCustomerOrdersQuery, List<OrderDto>>
{
    /// <summary>
    /// Handles the get customer orders query by filtering orders for the specified customer and date range.
    /// </summary>
    /// <param name="request">The query containing customer ID and optional date filters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of orders for the specified customer, ordered by creation date descending.</returns>
    public async Task<List<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken = default)
    {
        List<Order> orders = await context.Orders
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