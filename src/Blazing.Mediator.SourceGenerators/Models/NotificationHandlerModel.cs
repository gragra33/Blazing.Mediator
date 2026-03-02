using System.Collections.Generic;

namespace Blazing.Mediator.SourceGenerators.Models;

/// <summary>
/// Describes a single notification type and all handlers discovered for it at compile time.
/// </summary>
internal sealed class NotificationHandlerModel
{
    public NotificationHandlerModel(
        string safeNotificationName,
        string notificationType,
        List<string> handlerTypes,
        bool isInterface = false,
        List<MiddlewareModel>? applicableNotificationMiddleware = null)
    {
        SafeNotificationName = safeNotificationName;
        NotificationType = notificationType;
        HandlerTypes = handlerTypes;
        IsInterface = isInterface;
        ApplicableNotificationMiddleware = applicableNotificationMiddleware ?? new List<MiddlewareModel>();
    }

    /// <summary>
    /// Identifier-safe name derived from the notification type, used as a suffix for generated class names.
    /// </summary>
    public string SafeNotificationName { get; }

    /// <summary>Fully qualified notification type.</summary>
    public string NotificationType { get; }

    /// <summary>
    /// True when the notification type is an interface.
    /// Interface-typed arms must appear after all concrete notification type arms in the
    /// generated switch expression — otherwise the compiler emits CS8510 (unreachable pattern)
    /// for every sealed concrete type that implements the interface.
    /// </summary>
    public bool IsInterface { get; }

    /// <summary>
    /// Fully qualified handler type names, in discovery order.
    /// All of these implement <c>INotificationHandler&lt;TNotification&gt;</c>.
    /// </summary>
    public List<string> HandlerTypes { get; }

    /// <summary>
    /// Notification middleware that applies to this specific notification type.
    /// Constrained middleware (<c>INotificationMiddleware&lt;T&gt;</c>) is only included when
    /// the notification type satisfies the constraint. Non-constrained middleware is always included.
    /// </summary>
    public List<MiddlewareModel> ApplicableNotificationMiddleware { get; }
}
