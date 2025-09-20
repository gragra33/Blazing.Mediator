using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for UpdateUserCommand using Entity Framework Core.
/// </summary>
public sealed class UpdateUserHandler(ApplicationDbContext context)
    : IRequestHandler<UpdateUserCommand>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(UpdateUserHandler)}";

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken = default)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", "UpdateUserHandler.Handle");
        activity?.SetTag("user.id", request.UserId);

        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 300), cancellationToken);
        
        // Find the user to update - EXPLICITLY ENABLE TRACKING for updates
        var user = await context.Users
            .AsTracking() // Override the default NoTracking behavior for updates
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (user == null)
        {
            var message = $"User with ID {request.UserId} not found";
            activity?.SetStatus(ActivityStatusCode.Error, message);
            activity?.SetTag("handler.not_found", true);

            throw new NotFoundException(message);
        }

        // Update the user properties
        user.Name = request.Name;
        user.Email = request.Email;

        // Save changes to database
        var changesSaved = await context.SaveChangesAsync(cancellationToken);

        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("handler.updated", true);
        activity?.SetTag("handler.changes_saved", changesSaved);
        
        Console.WriteLine($"Updated user {user.Id}: {user.Name} ({user.Email}) - {changesSaved} changes saved");
    }
}