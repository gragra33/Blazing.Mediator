using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Exceptions;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUserQuery using Entity Framework Core.
/// </summary>
public sealed class GetUserHandler(ApplicationDbContext context) : IRequestHandler<GetUserQuery, UserDto>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(GetUserHandler)}";

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", "GetUserHandler.Handle");
        activity?.SetTag("user.id", request.UserId);

        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(10, 100), cancellationToken);
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            var message = $"User with ID {request.UserId} not found";
            activity?.SetTag("handler.not_found", true);
            activity?.SetStatus(ActivityStatusCode.Error, message);

            throw new NotFoundException(message);
        }

        activity?.SetStatus(ActivityStatusCode.Ok, $"User with ID {request.UserId} found");
        activity?.SetTag("handler.found", true);

        return user.ToDto();
    }
}