using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Application.Mappings;
using UserManagement.Api.Application.Queries;
using UserManagement.Api.Domain.Entities;
using UserManagement.Api.Infrastructure.Data;

namespace UserManagement.Api.Application.Handlers.Queries;

// CQRS Query Handlers - Optimized for read operations
public class GetUserByIdHandler(UserManagementDbContext context) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .AsNoTracking() // Read-only optimization
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new NotFoundException($"User with ID {request.UserId} not found");

        return user.ToDto();
    }
}