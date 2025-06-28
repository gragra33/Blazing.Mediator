using Blazing.Mediator;
using FluentValidation;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class CreateUserWithIdHandler(
    UserManagementDbContext context,
    IValidator<CreateUserWithIdCommand> validator)
    : IRequestHandler<CreateUserWithIdCommand, int>
{
    public async Task<int> Handle(CreateUserWithIdCommand request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.Errors);

        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}