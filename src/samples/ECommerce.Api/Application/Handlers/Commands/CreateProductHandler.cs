using Blazing.Mediator;
using ECommerce.Api.Application.Commands;
using ECommerce.Api.Application.Notifications;
using ECommerce.Api.Domain.Entities;
using ECommerce.Api.Infrastructure.Data;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Api.Application.Handlers.Commands;

/// <summary>
/// Handler for creating new products in the e-commerce system.
/// Validates the command and creates a new product entity in the database.
/// </summary>
/// <param name="context">The database context for persisting the product.</param>
/// <param name="validator">The validator for product creation commands.</param>
/// <param name="mediator">The mediator for publishing notifications.</param>
// Product Command Handlers
public class CreateProductHandler(ECommerceDbContext context, IValidator<CreateProductCommand> validator, IMediator mediator)
    : IRequestHandler<CreateProductCommand, int>
{
    /// <summary>
    /// Handles the create product command by validating the request and creating a new product.
    /// </summary>
    /// <param name="request">The create product command containing product details.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the ID of the newly created product.</returns>
    /// <exception cref="Application.Exceptions.ValidationException">Thrown when the command validation fails.</exception>
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Application.Exceptions.ValidationException(validationResult.Errors);

        Product product = Product.Create(request.Name, request.Description, request.Price, request.StockQuantity);

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        // Publish product created notification
        var productCreatedNotification = new ProductCreatedNotification(
            product.Id,
            product.Name,
            product.Price,
            product.StockQuantity
        );

        await mediator.Publish(productCreatedNotification, cancellationToken);

        return product.Id;
    }
}

// Order Command Handlers