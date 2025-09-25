using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Infrastructure.Telemetry;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUserQuery using Entity Framework Core.
/// Demonstrates OpenTelemetry best practices with static ActivitySource usage.
/// </summary>
internal sealed class GetUserHandler(ApplicationDbContext context) : IRequestHandler<GetUserQuery, UserDto>
{
    private const string ActivityName = "GetUserHandler.Handle";

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken = default)
    {
        // Use static ActivitySource for optimal performance
        using var activity = ApplicationActivitySources.Handlers.StartActivity(ActivityName);

        // Set comprehensive telemetry tags
        activity?.SetTag("handler.name", nameof(GetUserHandler));
        activity?.SetTag("handler.type", "QueryHandler");
        activity?.SetTag("user.id", request.UserId);
        activity?.SetTag("operation.type", "get_user");
        activity?.SetTag("data.source", "database");

        try
        {
            // Add operation start event
            activity?.AddEvent(new ActivityEvent("handler.execution.started", DateTimeOffset.UtcNow));

            // Simulate some processing delay (not security-sensitive)
            var delay = (int)(DateTime.UtcNow.Ticks % 91) + 10; // 10-100ms, deterministic, not Random
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

            // Database query with automatic EF Core telemetry
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken).ConfigureAwait(false);

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

            // Success telemetry
            activity?.SetTag("handler.result", "success");
            activity?.SetTag("user.name", user.Name);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddEvent(new ActivityEvent("handler.execution.completed", DateTimeOffset.UtcNow,
                new ActivityTagsCollection
                {
                    ["user.id"] = user.Id,
                    ["user.found"] = true
                }));

            return user.ToDto();
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
