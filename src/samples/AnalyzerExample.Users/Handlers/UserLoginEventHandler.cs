using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user login events
/// </summary>
public class UserLoginEventHandler : INotificationHandler<UserLoginEvent>
{
    private readonly ILogger<UserLoginEventHandler> _logger;

    public UserLoginEventHandler(ILogger<UserLoginEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserLoginEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} logged in from {IpAddress}", 
            notification.UserId, notification.IpAddress);

        await UpdateLastLoginTime(notification, cancellationToken);
        await CheckSecurityThresholds(notification, cancellationToken);
        await UpdateActivityMetrics(notification, cancellationToken);
    }

    private async Task UpdateLastLoginTime(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating last login time for user {UserId}", notification.UserId);
        await Task.Delay(10, cancellationToken);
    }

    private async Task CheckSecurityThresholds(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking security thresholds for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task UpdateActivityMetrics(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating activity metrics for user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }
}