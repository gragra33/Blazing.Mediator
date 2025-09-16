using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Validators;

/// <summary>
/// Validator for UpdateCustomerDetailsCommand.
/// </summary>
public class UpdateCustomerDetailsCommandValidator : AbstractValidator<UpdateCustomerDetailsCommand>
{
    public UpdateCustomerDetailsCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .Matches(@"^CUST-\d{6}$")
            .WithMessage("Customer ID must be in format CUST-XXXXXX");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MinimumLength(2)
            .WithMessage("Full name must be at least 2 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Valid email address is required");

        RuleFor(x => x.ContactMethod)
            .Must(method => new[] { "Email", "Phone", "SMS" }.Contains(method))
            .WithMessage("Contact method must be Email, Phone, or SMS");
    }
}