using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Analytics handler for user created events
/// </summary>
public class UserCreatedAnalyticsHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedAnalyticsHandler> _logger;

    public UserCreatedAnalyticsHandler(ILogger<UserCreatedAnalyticsHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing analytics for new user {UserId} ({FirstName} {LastName})", 
            notification.UserId, notification.FirstName, notification.LastName);

        await UpdateUserGrowthMetrics(notification, cancellationToken);
        await TrackRegistrationSource(notification, cancellationToken);
        await UpdateDemographicAnalytics(notification, cancellationToken);
    }

    private async Task UpdateUserGrowthMetrics(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating user growth metrics for new user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task TrackRegistrationSource(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Tracking registration source for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateDemographicAnalytics(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating demographic analytics for user {UserId}", notification.UserId);
        await Task.Delay(30, cancellationToken);
    }
}