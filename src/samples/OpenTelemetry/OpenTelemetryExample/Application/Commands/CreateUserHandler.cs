using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Domain.Entities;
using OpenTelemetryExample.Infrastructure.Data;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for CreateUserCommand using Entity Framework Core.
/// </summary>
public sealed class CreateUserHandler(ApplicationDbContext context, ILogger<CreateUserHandler> logger)
    : IRequestHandler<CreateUserCommand, int>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(CreateUserHandler)}";

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing CreateUserCommand for user: {UserName} with email: {UserEmail}",
            request.Name, request.Email);

        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", "CreateUserHandler.Handle");
        activity?.SetTag("user.name", request.Name);
        activity?.SetTag("user.email", request.Email);

        try
        {
            // Simulate some processing delay
            var delay = Random.Shared.Next(100, 500);
            logger.LogDebug("Simulating user creation processing delay of {DelayMs}ms", delay);
            await Task.Delay(delay, cancellationToken);

            // Simulate potential failures for testing
            if (request.Name.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                var message = "Simulated CreateUser error for testing telemetry";
                logger.LogWarning("Simulating error for user creation with name containing 'error': {UserName}", request.Name);
                activity?.SetStatus(ActivityStatusCode.Error, message);
                activity?.SetTag("handler.simulated_error", true);

                throw new InvalidOperationException(message);
            }

            // Check for duplicate email
            var existingUser = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                logger.LogWarning("Attempted to create user with duplicate email: {UserEmail}", request.Email);
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
            await context.SaveChangesAsync(cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("handler.created", true);
            activity?.SetTag("user.id", user.Id);

            logger.LogInformation("Successfully created user {UserId} with name: {UserName} and email: {UserEmail}",
                user.Id, user.Name, user.Email);

            return user.Id;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("CreateUserCommand operation was cancelled for user: {UserName}", request.Name);
            activity?.SetStatus(ActivityStatusCode.Error, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while creating user: {UserName} with email: {UserEmail}",
                request.Name, request.Email);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}