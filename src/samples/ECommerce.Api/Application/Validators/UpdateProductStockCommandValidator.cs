using ECommerce.Api.Application.Commands;
using FluentValidation;

namespace ECommerce.Api.Application.Validators;

/// <summary>
/// Validator for UpdateProductStockCommand to ensure stock update data is valid.
/// </summary>
public class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    /// <summary>
    /// Initializes a new instance of the UpdateProductStockCommandValidator class.
    /// </summary>
    public UpdateProductStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative");
    }
}
