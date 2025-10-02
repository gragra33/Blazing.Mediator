using AnalyzerExample.Users.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Users.Handlers;

/// <summary>
/// Handler for user role assignment events
/// </summary>
public class UserRoleAssignedEventHandler : INotificationHandler<UserRoleAssignedEvent>
{
    private readonly ILogger<UserRoleAssignedEventHandler> _logger;

    public UserRoleAssignedEventHandler(ILogger<UserRoleAssignedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserRoleAssignedEvent notification, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} assigned role '{RoleName}'", 
            notification.UserId, notification.RoleName);

        await UpdateUserPermissions(notification, cancellationToken);
        await NotifyRoleManagers(notification, cancellationToken);
        await AuditRoleChange(notification, cancellationToken);
    }

    private async Task UpdateUserPermissions(UserRoleAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Updating permissions for user {UserId}", notification.UserId);
        await Task.Delay(30, cancellationToken);
    }

    private async Task NotifyRoleManagers(UserRoleAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Notifying role managers about user {UserId} role assignment", notification.UserId);
        await Task.Delay(20, cancellationToken);
    }

    private async Task AuditRoleChange(UserRoleAssignedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Auditing role change for user {UserId}", notification.UserId);
        await Task.Delay(15, cancellationToken);
    }
}