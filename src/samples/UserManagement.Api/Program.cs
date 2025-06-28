using Blazing.Mediator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Application.Commands;
using UserManagement.Api.Application.DTOs;
using UserManagement.Api.Application.Exceptions;
using UserManagement.Api.Application.Queries;
using UserManagement.Api.Infrastructure.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
if (builder.Environment.IsDevelopment())
    // Use In-Memory database for development/demo
    builder.Services.AddDbContext<UserManagementDbContext>(options =>
        options.UseInMemoryDatabase("UserManagementDb"));
else
    // Use SQL Server for production
    builder.Services.AddDbContext<UserManagementDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register Mediator with CQRS handlers
builder.Services.AddMediator(typeof(Program).Assembly);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure database is created and seeded in development
    using IServiceScope scope = app.Services.CreateScope();
    UserManagementDbContext context = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();

// Define minimal API endpoints
RouteGroupBuilder api = app.MapGroup("/api/users").WithTags("Users");

// CQRS Query endpoints
api.MapGet("/{id:int}", async (int id, IMediator mediator) =>
    {
        try
        {
            GetUserByIdQuery query = new() { UserId = id };
            UserDto user = await mediator.Send(query);
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

api.MapGet("/", async (
        IMediator mediator,
        int page = 1,
        int pageSize = 10,
        string searchTerm = "",
        bool includeInactive = false) =>
    {
        GetUsersQuery query = new()
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IncludeInactive = includeInactive
        };

        PagedResult<UserDto> result = await mediator.Send(query);
        return Results.Ok(result);
    })
    .WithName("GetUsers")
    .WithSummary("Get paginated users")
    .WithDescription("Retrieves a paginated list of users with optional filtering")
    .Produces<PagedResult<UserDto>>();

api.MapGet("/active", async (IMediator mediator) =>
    {
        GetActiveUsersQuery query = new();
        List<UserDto> users = await mediator.Send(query);
        return Results.Ok(users);
    })
    .WithName("GetActiveUsers")
    .WithSummary("Get active users")
    .WithDescription("Retrieves all active users")
    .Produces<List<UserDto>>();

api.MapGet("/{id:int}/statistics", async (int id, IMediator mediator) =>
    {
        try
        {
            GetUserStatisticsQuery query = new() { UserId = id };
            UserStatisticsDto statistics = await mediator.Send(query);
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

// CQRS Command endpoints
api.MapPost("/", async (CreateUserCommand command, IMediator mediator) =>
    {
        try
        {
            await mediator.Send(command);
            return Results.Created("/api/users/0", null);
        }
        catch (UserManagement.Api.Application.Exceptions.ValidationException ex)
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

api.MapPost("/with-id", async (CreateUserWithIdCommand command, IMediator mediator) =>
    {
        try
        {
            int userId = await mediator.Send(command);
            return Results.Created($"/api/users/{userId}", userId);
        }
        catch (UserManagement.Api.Application.Exceptions.ValidationException ex)
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

api.MapPut("/{id:int}", async (int id, UpdateUserCommand command, IMediator mediator) =>
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
        catch (UserManagement.Api.Application.Exceptions.ValidationException ex)
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

api.MapPut("/{id:int}/with-result", async (int id, UpdateUserWithResultCommand command, IMediator mediator) =>
    {
        if (id != command.UserId)
            return Results.BadRequest("ID mismatch");

        OperationResult result = await mediator.Send(command);

        if (result.Success)
            return Results.Ok(result);
        return Results.BadRequest(result);
    })
    .WithName("UpdateUserWithResult")
    .WithSummary("Update user with result")
    .WithDescription("Updates an existing user and returns operation result")
    .Accepts<UpdateUserWithResultCommand>("application/json")
    .Produces<OperationResult>()
    .Produces(400);

api.MapDelete("/{id:int}", async (int id, IMediator mediator, string reason = "") =>
    {
        try
        {
            DeleteUserCommand command = new() { UserId = id, Reason = reason };
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

api.MapPost("/{id:int}/activate", async (int id, IMediator mediator) =>
    {
        try
        {
            ActivateUserAccountCommand command = new() { UserId = id };
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

api.MapPost("/{id:int}/deactivate", async (int id, IMediator mediator) =>
    {
        try
        {
            DeactivateUserAccountCommand command = new() { UserId = id };
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

app.Run();

/// <summary>
/// Represents the entry point for the User Management API application.
/// This application demonstrates the Blazing.Mediator library implementing CQRS pattern
/// with command and query handlers for user management operations.
/// </summary>
public partial class Program
{
    // Make Program class accessible for testing
}