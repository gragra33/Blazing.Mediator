using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class DeactivateUserAccountHandler(UserManagementDbContext context)
    : IRequestHandler<DeactivateUserAccountCommand>
{
    public async Task Handle(DeactivateUserAccountCommand request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        user.DeactivateAccount();
        await context.SaveChangesAsync(cancellationToken);
    }
}