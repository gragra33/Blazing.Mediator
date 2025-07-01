using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving a specific order by its unique identifier.
/// </summary>
/// <param name="context">The database context for accessing order data.</param>
public class GetOrderByIdHandler(ECommerceDbContext context) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    /// <summary>
    /// Handles the get order by ID query by retrieving the order with its items and product details.
    /// </summary>
    /// <param name="request">The query containing the order ID to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The order details if found.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the order is not found.</exception>
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");

        return order.ToDto();
    }
}