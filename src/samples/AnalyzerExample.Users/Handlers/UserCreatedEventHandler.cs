using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user creation events
/// </summary>
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(ILogger<UserCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} '{Email}' created", notification.UserId, notification.Email);

        await CreateUserProfile(notification, cancellationToken);
        await SetupDefaultPreferences(notification, cancellationToken);
        await InitializeUserMetrics(notification, cancellationToken);
    }

    private async Task CreateUserProfile(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating user profile for user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task SetupDefaultPreferences(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Setting up default preferences for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task InitializeUserMetrics(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing user metrics for user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }
}