using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user updated events
/// </summary>
public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
    private readonly ILogger<UserUpdatedEventHandler> _logger;

    public UserUpdatedEventHandler(ILogger<UserUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} ({FirstName} {LastName}) updated by {UpdatedBy}. Changes: {ChangeCount}",
            notification.UserId, notification.FirstName, notification.LastName, 
            notification.UpdatedBy, notification.Changes.Count);

        await LogAuditTrail(notification, cancellationToken);
        await UpdateSearchIndex(notification, cancellationToken);
        await NotifyRelatedSystems(notification, cancellationToken);
    }

    private async Task LogAuditTrail(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Logging audit trail for user {UserId} update", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task UpdateSearchIndex(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating search index for user {UserId}", notification.UserId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task NotifyRelatedSystems(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying related systems of user {UserId} update", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }
}