using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;
using System.Diagnostics;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetUsersQuery using Entity Framework Core.
/// </summary>
public sealed class GetUsersHandler(ApplicationDbContext context, ILogger<GetUsersHandler> logger) : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private const string AppSourceName = "OpenTelemetryExample";
    private const string ActivitySourceName = $"{AppSourceName}.Handler";
    private const string HandlerName = $"{AppSourceName}.{nameof(GetUsersHandler)}";

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing GetUsersQuery with IncludeInactive: {IncludeInactive}, SearchTerm: {SearchTerm}", 
            request.IncludeInactive, request.SearchTerm ?? "none");
        
        var activitySource = new ActivitySource(ActivitySourceName);
        using var activity = activitySource.StartActivity($"{HandlerName}.Handle");

        activity?.SetTag("handler.method", $"{HandlerName}.Handle");
        activity?.SetTag("includeInactive", request.IncludeInactive);
        activity?.SetTag("searchTerm", request.SearchTerm);

        try
        {
            // Simulate some processing delay
            var delay = Random.Shared.Next(50, 200);
            logger.LogDebug("Simulating database processing delay of {DelayMs}ms", delay);
            await Task.Delay(delay, cancellationToken);
            
            var query = context.Users.AsNoTracking(); // Use AsNoTracking for read-only queries
            logger.LogDebug("Building user query with filters");
            
            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
                logger.LogDebug("Applied active users filter");
            }
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query = query.Where(u => u.Name.Contains(request.SearchTerm)
                                         || u.Email.Contains(request.SearchTerm));
                logger.LogDebug("Applied search term filter: {SearchTerm}", request.SearchTerm);
            }
            
            logger.LogDebug("Executing database query to retrieve users");
            var users = await query.ToListAsync(cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("handler.user_count", users.Count);

            logger.LogInformation("Successfully retrieved {UserCount} users from database", users.Count);
            
            var userDtos = users.Select(u => u.ToDto()).ToList();
            logger.LogDebug("Converted {UserCount} users to DTOs", userDtos.Count);
            
            return userDtos;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("GetUsersQuery operation was cancelled");
            activity?.SetStatus(ActivityStatusCode.Error, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing GetUsersQuery");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}