using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for updating an existing product's information.
/// </summary>
/// <param name="context">The database context for accessing product data.</param>
/// <param name="validator">The validator for validating the update product command.</param>
public class UpdateProductHandler(ECommerceDbContext context, IValidator<UpdateProductCommand> validator)
    : IRequestHandler<UpdateProductCommand>
{
    /// <summary>
    /// Handles the update product command by validating the request and updating the product information.
    /// </summary>
    /// <param name="request">The command containing updated product information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="Application.Exceptions.ValidationException">Thrown when validation fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found.</exception>
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        Product? product = await context.Products.FindAsync([request.ProductId], cancellationToken);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UpdateStock(request.StockQuantity);

        await context.SaveChangesAsync(cancellationToken);
    }
}