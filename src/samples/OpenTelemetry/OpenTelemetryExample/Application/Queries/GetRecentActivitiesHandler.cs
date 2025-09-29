using Blazing.Mediator;
using Microsoft.EntityFrameworkCore;
using OpenTelemetryExample.Infrastructure.Data;
using OpenTelemetryExample.Shared.Models;

namespace OpenTelemetryExample.Application.Queries;

/// <summary>
/// Handler for GetRecentActivitiesQuery that retrieves real activity data from the database.
/// </summary>
internal sealed class GetRecentActivitiesHandler(ApplicationDbContext context, ILogger<GetRecentActivitiesHandler> logger)
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
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!recentActivities.Any())
            {
                LogNoActivities(logger, request.TimeWindow.TotalMinutes, null);
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

            LogActivitiesRetrieved(logger, activityDtos.Count, null);
            return result;
        }
        catch (Exception ex)
        {
            LogActivitiesError(logger, ex, null);
            return new RecentActivitiesDto
            {
                Timestamp = DateTime.UtcNow,
                Activities = [],
                Message = $"Error retrieving activities: {ex.Message}"
            };
        }
    }

    // LoggerMessage delegates for CA1848
    private static readonly Action<ILogger, double, Exception?> LogNoActivities =
        LoggerMessage.Define<double>(
            LogLevel.Information,
            new EventId(1, nameof(LogNoActivities)),
            "No telemetry activities found in the last {TimeWindow} minutes");
    private static readonly Action<ILogger, int, Exception?> LogActivitiesRetrieved =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(2, nameof(LogActivitiesRetrieved)),
            "Retrieved {ActivityCount} recent activities");
    private static readonly Action<ILogger, Exception, Exception?> LogActivitiesError =
        LoggerMessage.Define<Exception>(
            LogLevel.Error,
            new EventId(3, nameof(LogActivitiesError)),
            "Error retrieving recent activities: {Exception}");
}
