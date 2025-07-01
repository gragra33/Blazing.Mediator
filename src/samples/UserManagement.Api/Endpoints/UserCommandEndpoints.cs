using Blazing.Mediator;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Exceptions;

namespace UserManagement.Api.Endpoints;

/// <summary>
/// Handles user command endpoints following single responsibility principle.
/// </summary>
public static class UserCommandEndpoints
{
    /// <summary>
    /// Maps user command endpoints to the route group.
    /// </summary>
    /// <param name="group">The route group builder.</param>
    /// <returns>The route group builder for method chaining.</returns>
    public static RouteGroupBuilder MapUserCommandEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateUser();
        group.MapCreateUserWithId();
        group.MapUpdateUser();
        group.MapUpdateUserWithResult();
        group.MapDeleteUser();
        group.MapActivateUser();
        group.MapDeactivateUser();

        return group;
    }

    private static void MapCreateUser(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateUserCommand command, IMediator mediator) =>
            {
                try
                {
                    await mediator.Send(command);
                    return Results.Created("/api/users/0", null);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
            })
            .WithName("CreateUser")
            .WithSummary("Create user")
            .WithDescription("Creates a new user")
            .Accepts<CreateUserCommand>("application/json")
            .Produces(201)
            .Produces(400);
    }

    private static void MapCreateUserWithId(this RouteGroupBuilder group)
    {
        group.MapPost("/with-id", async (CreateUserWithIdCommand command, IMediator mediator) =>
            {
                try
                {
                    var userId = await mediator.Send(command);
                    return Results.Created($"/api/users/{userId}", userId);
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
            })
            .WithName("CreateUserWithId")
            .WithSummary("Create user with ID")
            .WithDescription("Creates a new user and returns the generated ID")
            .Accepts<CreateUserWithIdCommand>("application/json")
            .Produces<int>(201)
            .Produces(400);
    }

    private static void MapUpdateUser(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}", async (int id, UpdateUserCommand command, IMediator mediator) =>
            {
                if (id != command.UserId)
                    return Results.BadRequest("ID mismatch");

                try
                {
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
                catch (ValidationException ex)
                {
                    return Results.BadRequest(ex.Errors.Select(e => e.ErrorMessage));
                }
            })
            .WithName("UpdateUser")
            .WithSummary("Update user")
            .WithDescription("Updates an existing user")
            .Accepts<UpdateUserCommand>("application/json")
            .Produces(204)
            .Produces(400)
            .Produces(404);
    }

    private static void MapUpdateUserWithResult(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}/with-result", async (int id, UpdateUserWithResultCommand command, IMediator mediator) =>
            {
                if (id != command.UserId)
                    return Results.BadRequest("ID mismatch");

                var result = await mediator.Send(command);

                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("UpdateUserWithResult")
            .WithSummary("Update user with result")
            .WithDescription("Updates an existing user and returns operation result")
            .Accepts<UpdateUserWithResultCommand>("application/json")
            .Produces<OperationResult>()
            .Produces(400);
    }

    private static void MapDeleteUser(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:int}", async (int id, IMediator mediator, string reason = "") =>
            {
                try
                {
                    var command = new DeleteUserCommand { UserId = id, Reason = reason };
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Deletes a user")
            .Produces(204)
            .Produces(404);
    }

    private static void MapActivateUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/activate", async (int id, IMediator mediator) =>
            {
                try
                {
                    var command = new ActivateUserAccountCommand { UserId = id };
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("ActivateUser")
            .WithSummary("Activate user")
            .WithDescription("Activates a user account")
            .Produces(204)
            .Produces(404);
    }

    private static void MapDeactivateUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/deactivate", async (int id, IMediator mediator) =>
            {
                try
                {
                    var command = new DeactivateUserAccountCommand { UserId = id };
                    await mediator.Send(command);
                    return Results.NoContent();
                }
                catch (NotFoundException)
                {
                    return Results.NotFound();
                }
            })
            .WithName("DeactivateUser")
            .WithSummary("Deactivate user")
            .WithDescription("Deactivates a user account")
            .Produces(204)
            .Produces(404);
    }
}
