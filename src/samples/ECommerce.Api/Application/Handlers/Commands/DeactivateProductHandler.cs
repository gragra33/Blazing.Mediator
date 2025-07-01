using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for deactivating a product, making it unavailable for purchase.
/// </summary>
/// <param name="context">The database context for accessing product data.</param>
public class DeactivateProductHandler(ECommerceDbContext context) : IRequestHandler<DeactivateProductCommand>
{
    /// <summary>
    /// Handles the deactivate product command by finding the product and setting it as inactive.
    /// </summary>
    /// <param name="request">The command containing the product ID to deactivate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found.</exception>
    public async Task Handle(DeactivateProductCommand request, CancellationToken cancellationToken = default)
    {
        Product? product = await context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}