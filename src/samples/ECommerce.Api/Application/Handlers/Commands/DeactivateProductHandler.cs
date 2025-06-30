using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;

namespace ECommerce.Api.Application.Handlers.Commands;

public class DeactivateProductHandler(ECommerceDbContext context) : IRequestHandler<DeactivateProductCommand>
{
    public async Task Handle(DeactivateProductCommand request, CancellationToken cancellationToken = default)
    {
        Product? product = await context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}