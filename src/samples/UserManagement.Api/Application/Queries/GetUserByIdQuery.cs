using Blazing.Mediator;
using UserManagement.Api.Application.DTOs;

namespace UserManagement.Api.Application.Queries;

/// <summary>
/// Query to retrieve a user by their unique identifier.
/// This is a CQRS query that represents a read operation.
/// </summary>
// CQRS Query - Read operation
public class GetUserByIdQuery : IRequest<UserDto>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user to retrieve.
    /// </summary>
    public int UserId { get; set; }
}