using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Commands;

public class ActivateUserAccountHandler(UserManagementDbContext context) : IRequestHandler<ActivateUserAccountCommand>
{
    public async Task Handle(ActivateUserAccountCommand request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        user.ActivateAccount();
        await context.SaveChangesAsync(cancellationToken);
    }
}