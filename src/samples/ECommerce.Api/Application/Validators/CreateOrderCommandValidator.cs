using ECommerce.Api.Application.Commands;
using FluentValidation;

namespace ECommerce.Api.Application.Validators;

/// <summary>
/// Validator for the CreateOrderCommand.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the CreateOrderCommandValidator class.
    /// </summary>
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID must be greater than 0");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Customer email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        });
    }
}