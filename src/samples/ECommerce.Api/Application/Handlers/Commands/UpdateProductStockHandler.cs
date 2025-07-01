using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for updating the stock quantity of an existing product.
/// </summary>
/// <param name="context">The database context for accessing product data.</param>
/// <param name="validator">The validator for validating the update product stock command.</param>
public class UpdateProductStockHandler(ECommerceDbContext context, IValidator<UpdateProductStockCommand> validator) : IRequestHandler<UpdateProductStockCommand>
{
    /// <summary>
    /// Handles the update product stock command by validating the request and updating the product's stock quantity.
    /// </summary>
    /// <param name="request">The command containing the product ID and new stock quantity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="Application.Exceptions.ValidationException">Thrown when validation fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the product is not found.</exception>
    public async Task Handle(UpdateProductStockCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        Product? product = await context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        product.UpdateStock(request.StockQuantity);
        await context.SaveChangesAsync(cancellationToken);
    }
}