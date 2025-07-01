using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving a paginated list of orders with optional filtering.
/// </summary>
/// <param name="context">The database context for accessing order data.</param>
public class GetOrdersHandler(ECommerceDbContext context) : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    /// <summary>
    /// Handles the get orders query by applying filters and pagination.
    /// </summary>
    /// <param name="request">The query containing filtering and pagination parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated result of orders matching the specified criteria.</returns>
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = context.Orders.AsNoTracking().Include(o => o.Items).ThenInclude(i => i.Product);

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= request.ToDate.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Order> orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return orders.ToPagedDto(totalCount, request.Page, request.PageSize);
    }
}