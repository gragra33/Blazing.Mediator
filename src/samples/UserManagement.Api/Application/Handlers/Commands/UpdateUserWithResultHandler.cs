using Blazing.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class UpdateUserWithResultHandler(
    UserManagementDbContext context,
    IValidator<UpdateUserWithResultCommand> validator)
    : IRequestHandler<UpdateUserWithResultCommand, OperationResult>
{
    public async Task<OperationResult> Handle(UpdateUserWithResultCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Validation failed",
                    Data = new Dictionary<string, object>
                    {
                        ["errors"] = validationResult.Errors.Select(e => e.ErrorMessage)
                    }
                };
            }

            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = $"User with ID {request.UserId} not found"
                };
            }

            user.UpdatePersonalInfo(request.FirstName, request.LastName, request.Email);
            await context.SaveChangesAsync(cancellationToken);

            return new OperationResult
            {
                Success = true,
                Message = "User updated successfully",
                Data = new Dictionary<string, object>
                {
                    ["userId"] = user.Id,
                    ["updatedAt"] = user.UpdatedAt ?? DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            return new OperationResult
            {
                Success = false,
                Message = $"An error occurred: {ex.Message}"
            };
        }
    }
}