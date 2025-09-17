using FluentValidation;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Validator for UpdateUserCommand.
/// </summary>
public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
    }
}