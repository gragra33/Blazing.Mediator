using Blazing.Mediator;
using UserManagement.Api.Application.DTOs;

namespace UserManagement.Api.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of users with optional filtering.
/// This is a CQRS query that represents a read operation.
/// </summary>
public class GetUsersQuery : IRequest<PagedResult<UserDto>>
{
    /// <summary>
    /// Gets or sets the page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the search term to filter users by name or email.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to include inactive users in the results.
    /// </summary>
    public bool IncludeInactive { get; set; } = false;
}