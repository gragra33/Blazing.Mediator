using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for inactive user notifications
/// </summary>
public class InactiveUserNotificationHandler : INotificationHandler<InactiveUserNotification>
{
    private readonly ILogger<InactiveUserNotificationHandler> _logger;

    public InactiveUserNotificationHandler(ILogger<InactiveUserNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InactiveUserNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("User {UserId} ({FullName}) has been inactive for {DaysInactive} days. Last login: {LastLoginAt}",
            notification.UserId, notification.FullName, notification.DaysInactive, notification.LastLoginAt);

        await SendReEngagementEmail(notification, cancellationToken);
        await ScheduleFollowUpReminder(notification, cancellationToken);
        await UpdateUserSegmentation(notification, cancellationToken);
    }

    private async Task SendReEngagementEmail(InactiveUserNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending re-engagement email to {Email} for user {UserId}", 
            notification.Email, notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task ScheduleFollowUpReminder(InactiveUserNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Scheduling follow-up reminder for inactive user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task UpdateUserSegmentation(InactiveUserNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating user segmentation for inactive user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }
}