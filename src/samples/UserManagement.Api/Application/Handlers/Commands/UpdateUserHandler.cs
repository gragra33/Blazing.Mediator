using Blazing.Mediator;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class UpdateUserHandler(
    UserManagementDbContext context,
    IValidator<UpdateUserCommand> validator)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.Errors);

        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        // Use domain method
        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.Email);

        await context.SaveChangesAsync(cancellationToken);
    }
}