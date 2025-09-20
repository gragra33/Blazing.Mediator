using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUsersQuery using Entity Framework Core.
/// </summary>
public sealed class GetUsersHandler(ApplicationDbContext context) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(GetUsersHandler)}";

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", $"{HandlerName}.Handle");
        activity?.SetTag("includeInactive", request.IncludeInactive);
        activity?.SetTag("searchTerm", request.SearchTerm);

        // Simulate some processing delay
        await Task.Delay(Random.Shared.Next(50, 200), cancellationToken);
        var query = context.Users.AsQueryable();
        if (!request.IncludeInactive)
        {
            query = query.Where(u => u.IsActive);
        }
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(u => u.Name.Contains(request.SearchTerm)
                                     || u.Email.Contains(request.SearchTerm));
        }
        var users = await query.ToListAsync(cancellationToken);

        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("handler.user_count", users.Count);

        return users.Select(u => u.ToDto()).ToList();
    }
}