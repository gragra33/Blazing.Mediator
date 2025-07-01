using Blazing.Mediator;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Application.Queries;

namespace UserManagement.Api.Endpoints;

/// <summary>
/// Handles user query endpoints following single responsibility principle.
/// </summary>
public static class UserQueryEndpoints
{
    /// <summary>
    /// Maps user query endpoints to the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder MapUserQueryEndpoints(this RouteGroupBuilder group)
    {
        group.MapGetUserById();
        group.MapGetUsers();
        group.MapGetActiveUsers();
        group.MapGetUserStatistics();

        return group;
    }

    private static void MapGetUserById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetUserByIdQuery { UserId = id };
                    var user = await mediator.Send(query);
                    return Results.Ok(user);
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves a user by their unique identifier")
            .Produces<UserDto>()
            .Produces(404);
    }

    private static void MapGetUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
                IMediator mediator,
                int page = 1,
                int pageSize = 10,
                string searchTerm = "",
                bool includeInactive = false) =>
            {
                var query = new GetUsersQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    IncludeInactive = includeInactive
                };

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetUsers")
            .WithSummary("Get paginated users")
            .WithDescription("Retrieves a paginated list of users with optional filtering")
            .Produces<PagedResult<UserDto>>();
    }

    private static void MapGetActiveUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/active", async (IMediator mediator) =>
            {
                var query = new GetActiveUsersQuery();
                var users = await mediator.Send(query);
                return Results.Ok(users);
            })
            .WithName("GetActiveUsers")
            .WithSummary("Get active users")
            .WithDescription("Retrieves all active users")
            .Produces<List<UserDto>>();
    }

    private static void MapGetUserStatistics(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/statistics", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetUserStatisticsQuery { UserId = id };
                    var statistics = await mediator.Send(query);
                    return Results.Ok(statistics);
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("GetUserStatistics")
            .WithSummary("Get user statistics")
            .WithDescription("Retrieves statistics for a specific user")
            .Produces<UserStatisticsDto>()
            .Produces(404);
    }
}
