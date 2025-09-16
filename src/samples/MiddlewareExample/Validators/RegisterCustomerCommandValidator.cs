namespace MiddlewareExample.Validators;

/// <summary>
/// Validator for <see cref="RegisterCustomerCommand"/> that ensures customer data is valid before processing.
/// </summary>
public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(2).WithMessage("Full name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name can only contain letters and spaces");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.ContactMethod)
            .NotEmpty().WithMessage("Contact method is required")
            .Must(method => new[] { "Email", "Phone", "SMS" }.Contains(method))
            .WithMessage("Contact method must be one of: Email, Phone, SMS");
    }
}
