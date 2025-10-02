using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Security handler for user login events
/// </summary>
public class UserLoginSecurityHandler : INotificationHandler<UserLoginEvent>
{
    private readonly ILogger<UserLoginSecurityHandler> _logger;

    public UserLoginSecurityHandler(ILogger<UserLoginSecurityHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserLoginEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing security checks for user {UserId} login from {IpAddress}", 
            notification.UserId, notification.IpAddress);

        await DetectAnomalousLogin(notification, cancellationToken);
        await UpdateSecurityProfile(notification, cancellationToken);
        await CheckForSuspiciousActivity(notification, cancellationToken);
    }

    private async Task DetectAnomalousLogin(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Detecting anomalous login patterns for user {UserId}", notification.UserId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task UpdateSecurityProfile(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating security profile for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task CheckForSuspiciousActivity(UserLoginEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking for suspicious activity for user {UserId} from {IpAddress}", 
            notification.UserId, notification.IpAddress);
        await Task.Delay(25, cancellationToken);
    }
}