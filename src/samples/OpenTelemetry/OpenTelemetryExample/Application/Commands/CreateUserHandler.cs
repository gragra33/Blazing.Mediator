using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for CreateUserCommand using Entity Framework Core.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
public sealed class CreateUserHandler(ApplicationDbContext context, ILogger<CreateUserHandler> logger)
    : IRequestHandler<CreateUserCommand, int>
{
    private const string ActivityName = "CreateUserHandler.Handle";

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing CreateUserCommand for user: {UserName} with email: {UserEmail}",
            request.Name, request.Email);

        // Use static ActivitySource for optimal performance
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);

        // Set comprehensive telemetry tags
        activity?.SetTag("handler.name", nameof(CreateUserHandler));
        activity?.SetTag("handler.type", "CommandHandler");
        activity?.SetTag("user.name", request.Name);
        activity?.SetTag("user.email", request.Email);
        activity?.SetTag("operation.type", "create_user");
        activity?.SetTag("data.source", "database");

        try
        {
            // Add operation start event
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));

            // Simulate some processing delay
            var delay = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100, 500);
            logger.LogDebug("Simulating user creation processing delay of {DelayMs}ms", delay);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            // Simulate potential failures for testing
            if (request.Name.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                var message = "Simulated CreateUser error for testing telemetry";
                logger.LogWarning("Simulating error for user creation with name containing 'error': {UserName}", request.Name);

                // Structured error telemetry
                activity?.SetTag("handler.result", "simulated_error");
                activity?.SetTag("error.type", "SimulatedError");
                activity?.SetStatus(ActivityStatusCode.Error, message);
                activity?.AddEvent(new ActivityEvent("handler.error.simulated", DateTimeOffset.UtcNow,
                    new ActivityTagsCollection { ["reason"] = "Name contains 'error'" }));

                throw new InvalidOperationException(message);
            }

            // Check for duplicate email
            var existingUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken).ConfigureAwait(false);

            if (existingUser != null)
            {
                logger.LogWarning("Attempted to create user with duplicate email: {UserEmail}", request.Email);

                // Structured validation error telemetry
                activity?.SetTag("handler.result", "duplicate_email");
                activity?.SetTag("error.type", "DuplicateEmail");
                activity?.SetStatus(ActivityStatusCode.Error, "Duplicate email");
                activity?.AddEvent(new ActivityEvent("handler.error.duplicate_email", DateTimeOffset.UtcNow,
                    new ActivityTagsCollection { ["email"] = request.Email }));

                throw new InvalidOperationException($"User with email {request.Email} already exists");
            }

            logger.LogDebug("Creating new user entity");

            // Create new user entity
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Add to database
            logger.LogDebug("Adding user to database context");
            context.Users.Add(user);

            logger.LogDebug("Saving changes to database");
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Success telemetry
            activity?.SetTag("handler.result", "success");
            activity?.SetTag("user.id", user.Id);
            activity?.SetTag("user.created_at", user.CreatedAt.ToString("O"));
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["user.id"] = user.Id,
                    ["user.created"] = true
                }));

            logger.LogInformation("Successfully created user {UserId} with name: {UserName} and email: {UserEmail}",
                user.Id, user.Name, user.Email);

            return user.Id;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("CreateUserCommand operation was cancelled for user: {UserName}", request.Name);

            // Cancellation telemetry
            activity?.SetTag("handler.result", "cancelled");
            activity?.SetStatus(ActivityStatusCode.Error, "Operation cancelled");
            activity?.AddEvent(new ActivityEvent("handler.cancelled", DateTimeOffset.UtcNow));

            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Error occurred while creating user: {UserName} with email: {UserEmail}",
                request.Name, request.Email);

            // Comprehensive error telemetry
            activity?.SetTag("handler.result", "error");
            activity?.SetTag("exception.type", ex.GetType().Name);
            activity?.SetTag("exception.message", ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("handler.exception", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["exception.type"] = ex.GetType().Name,
                    ["exception.source"] = ex.Source ?? "unknown"
                }));

            throw;
        }
    }
}
