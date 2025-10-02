using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user email verification events
/// </summary>
public class UserEmailVerifiedEventHandler : INotificationHandler<UserEmailVerifiedEvent>
{
    private readonly ILogger<UserEmailVerifiedEventHandler> _logger;

    public UserEmailVerifiedEventHandler(ILogger<UserEmailVerifiedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserEmailVerifiedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email verified for user {UserId}", notification.UserId);

        await ActivateUserAccount(notification, cancellationToken);
        await GrantVerifiedUserBenefits(notification, cancellationToken);
        await UpdateTrustScore(notification, cancellationToken);
    }

    private async Task ActivateUserAccount(UserEmailVerifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Activating account for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task GrantVerifiedUserBenefits(UserEmailVerifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Granting verified user benefits for user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task UpdateTrustScore(UserEmailVerifiedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating trust score for user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }
}