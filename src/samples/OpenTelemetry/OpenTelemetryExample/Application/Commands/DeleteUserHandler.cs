using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for DeleteUserCommand using Entity Framework Core.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
public sealed class DeleteUserHandler(ApplicationDbContext context)
    : IRequestHandler<DeleteUserCommand>
{
    private const string ActivityName = "DeleteUserHandler.Handle";

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        // Use static ActivitySource for optimal performance
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);

        // Set comprehensive telemetry tags
        activity?.SetTag("handler.name", nameof(DeleteUserHandler));
        activity?.SetTag("handler.type", "CommandHandler");
        activity?.SetTag("user.id", request.UserId);
        activity?.SetTag("operation.type", "delete_user");
        activity?.SetTag("data.source", "database");

        try
        {
            // Add operation start event
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));

            // Simulate some processing delay
            await Task.Delay(Random.Shared.Next(25, 150), cancellationToken);

            // Find the user to delete - EXPLICITLY ENABLE TRACKING for deletes
            var user = await context.Users
                .AsTracking() // Override the default NoTracking behavior for deletes
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

            // Store user info for telemetry before deletion
            var deletedUserName = user.Name;
            var deletedUserEmail = user.Email;

            // Remove the user from database
            context.Users.Remove(user);
            var changesSaved = await context.SaveChangesAsync(cancellationToken);

            // Success telemetry
            activity?.SetTag("handler.result", "success");
            activity?.SetTag("user.deleted", true);
            activity?.SetTag("user.changes_saved", changesSaved);
            activity?.SetTag("user.deleted_name", deletedUserName);
            activity?.SetTag("user.deleted_email", deletedUserEmail);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["user.id"] = request.UserId,
                    ["changes_saved"] = changesSaved,
                    ["user.deleted"] = true
                }));

            Console.WriteLine($"Deleted user {request.UserId}: {deletedUserName} ({deletedUserEmail}) - {changesSaved} changes saved");
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