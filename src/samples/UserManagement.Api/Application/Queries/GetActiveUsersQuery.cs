using Blazing.Mediator;
using UserManagement.Api.Application.DTOs;

namespace UserManagement.Api.Application.Queries;

/// <summary>
/// Query to retrieve all active users from the system.
/// This is a CQRS query that represents a read operation.
/// </summary>
public class GetActiveUsersQuery : IRequest<List<UserDto>>
{
}