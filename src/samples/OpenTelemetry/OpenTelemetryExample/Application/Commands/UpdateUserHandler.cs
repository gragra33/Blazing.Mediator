using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for UpdateUserCommand using Entity Framework Core.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
public sealed class UpdateUserHandler(ApplicationDbContext context)
    : IRequestHandler<UpdateUserCommand>
{
    private const string ActivityName = "UpdateUserHandler.Handle";

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        // Use static ActivitySource for optimal performance
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);

        // Set comprehensive telemetry tags
        activity?.SetTag("handler.name", nameof(UpdateUserHandler));
        activity?.SetTag("handler.type", "CommandHandler");
        activity?.SetTag("user.id", request.UserId);
        activity?.SetTag("user.name", request.Name);
        activity?.SetTag("user.email", request.Email);
        activity?.SetTag("operation.type", "update_user");
        activity?.SetTag("data.source", "database");

        try
        {
            // Add operation start event
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));

            // Simulate some processing delay
            await Task.Delay(Random.Shared.Next(50, 300), cancellationToken);

            // Find the user to update - EXPLICITLY ENABLE TRACKING for updates
            var user = await context.Users
                .AsTracking() // Override the default NoTracking behavior for updates
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                var message = $"User with ID {request.UserId} not found";

                // Structured error telemetry
                activity?.SetTag("handler.result", "not_found");
                activity?.SetTag("error.type", "EntityNotFound");
                activity?.SetStatus(ActivityStatusCode.Error, message);
                activity?.AddEvent(new ActivityEvent("handler.error.user_not_found", DateTimeOffset.UtcNow,
                    new ActivityTagsCollection { ["user.id"] = request.UserId }));

                throw new NotFoundException(message);
            }

            // Store original values for change tracking
            var originalName = user.Name;
            var originalEmail = user.Email;

            // Update the user properties
            user.Name = request.Name;
            user.Email = request.Email;

            // Save changes to database
            var changesSaved = await context.SaveChangesAsync(cancellationToken);

            // Success telemetry
            activity?.SetTag("handler.result", "success");
            activity?.SetTag("user.updated", true);
            activity?.SetTag("user.changes_saved", changesSaved);
            activity?.SetTag("user.name_changed", originalName != request.Name);
            activity?.SetTag("user.email_changed", originalEmail != request.Email);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["user.id"] = user.Id,
                    ["changes_saved"] = changesSaved,
                    ["user.updated"] = true
                }));

            Console.WriteLine($"Updated user {user.Id}: {user.Name} ({user.Email}) - {changesSaved} changes saved");
        }
        catch (Exception ex) when (ex is not NotFoundException)
        {
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