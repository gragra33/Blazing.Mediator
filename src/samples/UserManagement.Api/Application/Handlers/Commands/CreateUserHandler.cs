using Blazing.Mediator;
using FluentValidation;
using FluentValidation.Results;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

// CQRS Command Handlers - Focused on business logic and state changes
public class CreateUserHandler(
    UserManagementDbContext context,
    IValidator<CreateUserCommand> validator,
    ILogger<CreateUserHandler> logger)
    : IRequestHandler<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Business validation
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.Errors);

        logger.LogInformation("Creating user with email {Email}", request.Email);

        // Create domain entity with business logic
        User user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        // Save using context
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} created successfully", user.Id);
    }
}