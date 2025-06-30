using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Exceptions;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Api.Application.Handlers.Commands;

public class UpdateProductHandler(ECommerceDbContext context, IValidator<UpdateProductCommand> validator) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        Product? product = await context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UpdateStock(request.StockQuantity);

        await context.SaveChangesAsync(cancellationToken);
    }
}