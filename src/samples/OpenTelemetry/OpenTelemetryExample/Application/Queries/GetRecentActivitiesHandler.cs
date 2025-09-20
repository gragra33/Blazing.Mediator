using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetRecentActivitiesQuery that retrieves real activity data from the database.
/// </summary>
public sealed class GetRecentActivitiesHandler(ApplicationDbContext context, ILogger<GetRecentActivitiesHandler> logger)
    : IRequestHandler<GetRecentActivitiesQuery, RecentActivitiesDto>
{
    public async Task<RecentActivitiesDto> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow - request.TimeWindow;

        try
        {
            // Get recent activities from the database
            var recentActivities = await context.TelemetryActivities
                .Where(a => a.StartTime >= cutoffTime)
                .OrderByDescending(a => a.StartTime)
                .Take(request.MaxRecords)
                .ToListAsync(cancellationToken);

            if (!recentActivities.Any())
            {
                logger.LogInformation("No telemetry activities found in the last {TimeWindow} minutes", request.TimeWindow.TotalMinutes);
                return new RecentActivitiesDto
                {
                    Timestamp = DateTime.UtcNow,
                    Activities = [],
                    Message = "No activity data available yet. Try making some requests to the API first."
                };
            }

            var activityDtos = recentActivities.Select(activity => new ActivityDto
            {
                Id = activity.ActivityId,
                OperationName = activity.OperationName,
                StartTime = activity.StartTime,
                Duration = activity.Duration,
                Status = activity.Status,
                Kind = activity.Kind,
                Tags = activity.Tags
            }).ToList();

            var result = new RecentActivitiesDto
            {
                Timestamp = DateTime.UtcNow,
                Activities = activityDtos,
                Message = $"Recent activities from the last {request.TimeWindow.TotalMinutes:F0} minutes - showing {activityDtos.Count} activities"
            };

            logger.LogInformation("Retrieved {ActivityCount} recent activities", activityDtos.Count);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent activities");

            return new RecentActivitiesDto
            {
                Timestamp = DateTime.UtcNow,
                Activities = [],
                Message = $"Error retrieving activities: {ex.Message}"
            };
        }
    }
}