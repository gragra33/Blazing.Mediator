using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Validators;

/// <summary>
/// Validator for RegisterCustomerCommand.
/// </summary>
public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
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