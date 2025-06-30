using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Application.Queries;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Queries;

public class GetUserStatisticsHandler(UserManagementDbContext context)
    : IRequestHandler<GetUserStatisticsQuery, UserStatisticsDto>
{
    public async Task<UserStatisticsDto> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        return new UserStatisticsDto
        {
            UserId = user.Id,
            FullName = user.GetFullName(),
            AccountAgeInDays = (DateTime.UtcNow - user.CreatedAt).Days,
            Status = user.IsActive ? "Active" : "Inactive"
        };
    }
}