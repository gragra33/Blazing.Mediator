using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Commands;

/// <summary>
/// Handler for DeleteUserCommand using Entity Framework Core.
/// </summary>
public sealed class DeleteUserHandler(ApplicationDbContext context)
    : IRequestHandler<DeleteUserCommand>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(DeleteUserHandler)}";

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken = default)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", "DeleteUserHandler.Handle");
        activity?.SetTag("user.id", request.UserId);

        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(25, 150), cancellationToken);
        // Find the user to delete
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            var message = $"User with ID {request.UserId} not found";
            activity?.SetStatus(ActivityStatusCode.Error, message);
            activity?.SetTag("handler.not_found", true);

            throw new NotFoundException(message);
        }
        // Remove the user from database
        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("handler.deleted", true);
        Console.WriteLine($"Deleted user {user.Id}: {user.Name} ({user.Email})");
    }
}