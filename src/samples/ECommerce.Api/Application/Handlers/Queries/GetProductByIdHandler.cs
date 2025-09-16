using Blazing.Mediator;
using ECommerce.Api.Application.DTOs;
using ECommerce.Api.Application.Mappings;
using ECommerce.Api.Application.Queries;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Application.Handlers.Queries;

/// <summary>
/// Handler for retrieving a product by its unique identifier.
/// Returns product details as a ProductDto for API consumption.
/// </summary>
/// <param name="context">The database context for querying product data.</param>
// Product Query Handlers
public class GetProductByIdHandler(ECommerceDbContext context)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    /// <summary>
    /// Handles the get product by ID query and returns the product details.
    /// </summary>
    /// <param name="request">The query containing the product ID to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the product details as a ProductDto.</returns>
    /// <remarks>Throws a NotFoundException when the product is not found.</remarks>
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken = default)
    {
        Product? product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        return product.ToDto();
    }
}

// Order Query Handlers