using Blazing.Mediator;

namespace UserManagement.Api.Application.Queries;

/// <summary>
/// Query to retrieve statistics for a specific user.
/// This is a CQRS query that represents a read operation.
/// </summary>
public class GetUserStatisticsQuery : IRequest<UserStatisticsDto>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user for which to retrieve statistics.
    /// </summary>
    public int UserId { get; set; }
}