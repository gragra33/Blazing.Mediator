using ECommerce.Api.Application.Commands;
using FluentValidation;

namespace ECommerce.Api.Application.Validators;

/// <summary>
/// Validator for UpdateProductCommand to ensure product update data is valid.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateProductCommandValidator class.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Product description cannot exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Product price must be greater than 0")
            .LessThanOrEqualTo(999999.99m)
            .WithMessage("Product price cannot exceed $999,999.99");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative");
    }
}
