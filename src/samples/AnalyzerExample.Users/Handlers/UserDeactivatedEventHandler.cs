using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user deactivated events
/// </summary>
public class UserDeactivatedEventHandler : INotificationHandler<UserDeactivatedEvent>
{
    private readonly ILogger<UserDeactivatedEventHandler> _logger;

    public UserDeactivatedEventHandler(ILogger<UserDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserDeactivatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} ({FullName}) deactivated by {DeactivatedBy}. Reason: {Reason}",
            notification.UserId, notification.FullName, notification.DeactivatedBy, notification.Reason);

        await RevokeUserSessions(notification, cancellationToken);
        await NotifyUserOfDeactivation(notification, cancellationToken);
        await UpdateAccessControls(notification, cancellationToken);
    }

    private async Task RevokeUserSessions(UserDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Revoking all active sessions for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task NotifyUserOfDeactivation(UserDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending deactivation notification to {Email}", notification.Email);
        await Task.Delay(30, cancellationToken);
    }

    private async Task UpdateAccessControls(UserDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating access controls for deactivated user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }
}