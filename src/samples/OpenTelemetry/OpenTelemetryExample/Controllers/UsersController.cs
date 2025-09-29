using Blazing.Mediator;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetryExample.Application.Commands;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Controllers;

/// <summary>
/// Controller for user management operations demonstrating OpenTelemetry integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator, ILogger<UsersController> logger) : ControllerBase
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string ControllerName = $"{AppSourceName}.{nameof(UsersController)}";

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user data.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        logger.LogInformation("Getting user with ID: {UserId}", id);

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.GetUserById", ActivityKind.Server);
        activity?.SetTag("controller.method", "GetUserById");
        activity?.SetTag("user.id", id);
        try
        {
            var query = new GetUserQuery { UserId = id };
            var user = await mediator.Send(query);

            logger.LogInformation("Successfully retrieved user {UserId} with name: {UserName}", id, user.Name);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "User {UserId} not found: {Message}", id, ex.Message);
            return NotFound($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all users with optional filtering.
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive users.</param>
    /// <param name="searchTerm">Optional search term to filter users.</param>
    /// <returns>List of users.</returns>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers(
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? searchTerm = null)
    {
        logger.LogInformation("Getting users with filters - IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}",
            includeInactive, searchTerm ?? "none");

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.GetUsers", ActivityKind.Server);
        activity?.SetTag("controller.method", "GetUsers");
        activity?.SetTag("includeInactive", includeInactive);
        activity?.SetTag("searchTerm", searchTerm);
        try
        {
            var query = new GetUsersQuery
            {
                IncludeInactive = includeInactive,
                SearchTerm = searchTerm
            };
            var users = await mediator.Send(query);

            logger.LogInformation("Successfully retrieved {UserCount} users", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users with filters - IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}",
                includeInactive, searchTerm);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    /// <param name="command">The user creation data.</param>
    /// <returns>The created user ID.</returns>
    [HttpPost]
    public async Task<ActionResult<int>> CreateUser([FromBody] CreateUserCommand command)
    {
        logger.LogInformation("Creating new user with name: {UserName}, email: {UserEmail}",
            command.Name, command.Email);

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.CreateUser", ActivityKind.Server);
        activity?.SetTag("controller.method", "CreateUser");
        try
        {
            var userId = await mediator.Send(command);

            logger.LogInformation("Successfully created user {UserId} with name: {UserName}", userId, command.Name);
            return CreatedAtAction(nameof(GetUserById), new { id = userId }, userId);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed for user creation: {ValidationErrors}",
                string.Join(", ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            var errors = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage });
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user with name: {UserName}, email: {UserEmail}",
                command.Name, command.Email);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="command">The user update data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserCommand command)
    {
        logger.LogInformation("Updating user {UserId} with new name: {UserName}, email: {UserEmail}",
            id, command.Name, command.Email);

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.UpdateUser", ActivityKind.Server);
        activity?.SetTag("controller.method", "UpdateUser");
        activity?.SetTag("user.id", id);

        if (id != command.UserId)
        {
            logger.LogWarning("ID mismatch in user update - Route ID: {RouteId}, Command ID: {CommandId}",
                id, command.UserId);
            return BadRequest("ID mismatch between route and body");
        }

        try
        {
            await mediator.Send(command);

            logger.LogInformation("Successfully updated user {UserId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "User {UserId} not found for update: {Message}", id, ex.Message);
            return NotFound($"User with ID {id} not found");
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation failed for user {UserId} update: {ValidationErrors}",
                id, string.Join(", ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            var errors = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage });
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        logger.LogInformation("Deleting user {UserId}", id);

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.DeleteUser", ActivityKind.Server);
        activity?.SetTag("controller.method", "DeleteUser");
        activity?.SetTag("user.id", id);
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            await mediator.Send(command);

            logger.LogInformation("Successfully deleted user {UserId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "User {UserId} not found for deletion: {Message}", id, ex.Message);
            return NotFound($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Simulate error for testing telemetry.
    /// </summary>
    /// <returns>Error response.</returns>
    [HttpPost("simulate-error")]
    public Task<ActionResult> SimulateError()
    {
        logger.LogInformation("Simulating error for telemetry testing");

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.SimulateError", ActivityKind.Server);
        activity?.SetTag("controller.method", "SimulateError");
        try
        {
            // This will trigger an error in the handler for telemetry testing
            logger.LogDebug("About to throw simulated exception");
            throw new InvalidOperationException("This is a simulated error for testing OpenTelemetry integration");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Simulated error occurred for telemetry testing");
            return Task.FromResult<ActionResult>(StatusCode(500, new { Message = "Simulated error for telemetry testing", Details = ex.Message }));
        }
    }

    /// <summary>
    /// Simulate validation error for testing telemetry.
    /// </summary>
    /// <returns>Validation error response.</returns>
    [HttpPost("simulate-validation-error")]
    public async Task<ActionResult> SimulateValidationError()
    {
        logger.LogInformation("Simulating validation error for telemetry testing");

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{ControllerName}.SimulateValidationError", ActivityKind.Server);
        activity?.SetTag("controller.method", "SimulateValidationError");
        try
        {
            // This will trigger validation errors for telemetry testing
            var command = new CreateUserCommand { Name = "", Email = "invalid-email" };
            logger.LogDebug("Creating user with invalid data to trigger validation errors");
            await mediator.Send(command);
            return Ok();
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Validation errors triggered during simulation: {ValidationErrors}",
                string.Join(", ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));

            var errors = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage });
            return BadRequest(new { Message = "Validation failed (simulated)", Errors = errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during validation simulation");
            return StatusCode(500, "Internal server error");
        }
    }
}
