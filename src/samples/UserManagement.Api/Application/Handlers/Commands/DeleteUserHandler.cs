using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class DeleteUserHandler(UserManagementDbContext context, ILogger<DeleteUserHandler> logger)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        logger.LogInformation("Deleting user {UserId}. Reason: {Reason}", request.UserId, request.Reason);

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}