using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user registration completed events
/// </summary>
public class UserRegistrationCompletedEventHandler : INotificationHandler<UserRegistrationCompletedEvent>
{
    private readonly ILogger<UserRegistrationCompletedEventHandler> _logger;

    public UserRegistrationCompletedEventHandler(ILogger<UserRegistrationCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registration completed for user {UserId} ({FirstName} {LastName}) at {Email}",
            notification.UserId, notification.FirstName, notification.LastName, notification.Email);

        await SendWelcomeEmail(notification, cancellationToken);
        await CreateUserProfile(notification, cancellationToken);
        await SetupDefaultPreferences(notification, cancellationToken);
        await TriggerOnboardingFlow(notification, cancellationToken);
    }

    private async Task SendWelcomeEmail(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending welcome email to {Email}", notification.Email);
        await Task.Delay(30, cancellationToken);
    }

    private async Task CreateUserProfile(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating user profile for user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task SetupDefaultPreferences(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Setting up default preferences for user {UserId}", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task TriggerOnboardingFlow(UserRegistrationCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Triggering onboarding flow for user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }
}