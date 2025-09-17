using Microsoft.AspNetCore.Mvc;
using Blazing.Mediator;
using OpenTelemetryExample.Application.Commands;
using OpenTelemetryExample.Application.Queries;
using OpenTelemetryExample.Shared.DTOs;
using OpenTelemetryExample.Exceptions;

namespace OpenTelemetryExample.Controllers;

/// <summary>
/// Controller for user management operations demonstrating OpenTelemetry integration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController(IMediator mediator, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user data.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var query = new GetUserQuery { UserId = id };
            var user = await mediator.Send(query);
            return Ok(user);
        }
        catch (NotFoundException)
        {
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
        try
        {
            var query = new GetUsersQuery 
            { 
                IncludeInactive = includeInactive, 
                SearchTerm = searchTerm 
            };
            var users = await mediator.Send(query);
            return Ok(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users");
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
        try
        {
            var userId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage });
            return BadRequest(new { Message = "Validation failed", Errors = errors });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
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
        if (id != command.UserId)
        {
            return BadRequest("ID mismatch between route and body");
        }

        try
        {
            await mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound($"User with ID {id} not found");
        }
        catch (ValidationException ex)
        {
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
        try
        {
            var command = new DeleteUserCommand { UserId = id };
            await mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException)
        {
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
    public async Task<ActionResult> SimulateError()
    {
        try
        {
            // This will trigger an error in the handler for telemetry testing
            var command = new CreateUserCommand { Name = "Error User", Email = "error@example.com" };
            await mediator.Send(command);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Simulated error occurred");
            return StatusCode(500, new { Message = "Simulated error for telemetry testing", Details = ex.Message });
        }
    }

    /// <summary>
    /// Simulate validation error for testing telemetry.
    /// </summary>
    /// <returns>Validation error response.</returns>
    [HttpPost("simulate-validation-error")]
    public async Task<ActionResult> SimulateValidationError()
    {
        try
        {
            // This will trigger validation errors for telemetry testing
            var command = new CreateUserCommand { Name = "", Email = "invalid-email" };
            await mediator.Send(command);
            return Ok();
        }
        catch (ValidationException ex)
        {
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