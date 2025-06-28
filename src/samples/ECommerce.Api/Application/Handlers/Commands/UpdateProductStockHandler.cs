using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Exceptions;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;

namespace ECommerce.Api.Application.Handlers.Commands;

public class UpdateProductStockHandler(ECommerceDbContext context, IValidator<UpdateProductStockCommand> validator) : IRequestHandler<UpdateProductStockCommand>
{
    public async Task Handle(UpdateProductStockCommand request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        var product = await context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.UpdateStock(request.StockQuantity);
        await context.SaveChangesAsync(cancellationToken);
    }
}