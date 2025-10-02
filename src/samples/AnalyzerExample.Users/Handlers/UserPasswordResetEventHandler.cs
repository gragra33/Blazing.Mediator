using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user password reset events
/// </summary>
public class UserPasswordResetEventHandler : INotificationHandler<UserPasswordResetEvent>
{
    private readonly ILogger<UserPasswordResetEventHandler> _logger;

    public UserPasswordResetEventHandler(ILogger<UserPasswordResetEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserPasswordResetEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Password reset for user {UserId} at {Email} by {RequestedBy}",
            notification.UserId, notification.Email, notification.RequestedBy);

        await SendPasswordResetConfirmation(notification, cancellationToken);
        await LogSecurityEvent(notification, cancellationToken);
        await InvalidateExistingSessions(notification, cancellationToken);
    }

    private async Task SendPasswordResetConfirmation(UserPasswordResetEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending password reset confirmation to {Email}", notification.Email);
        await Task.Delay(20, cancellationToken);
    }

    private async Task LogSecurityEvent(UserPasswordResetEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Logging security event for password reset of user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task InvalidateExistingSessions(UserPasswordResetEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Invalidating existing sessions for user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }
}