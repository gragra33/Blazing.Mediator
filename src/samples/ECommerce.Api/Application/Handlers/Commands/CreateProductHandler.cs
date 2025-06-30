using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Exceptions;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Api.Application.Handlers.Commands;

// Product Command Handlers
public class CreateProductHandler(ECommerceDbContext context, IValidator<CreateProductCommand> validator)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        Product? product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

// Order Command Handlers