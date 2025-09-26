namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Represents analysis information for a notification type, including handler and subscriber details.
/// Contains metadata about the type, assembly information, and the status of both automatic handlers
/// and manual subscribers for the notification.
/// </summary>
/// <param name="Type">The actual notification Type being analyzed.</param>
/// <param name="ClassName">The name of the notification class without generic parameters.</param>
/// <param name="TypeParameters">String representation of generic type parameters (e.g., "&lt;T, U&gt;").</param>
/// <param name="Assembly">The name of the assembly containing this notification type.</param>
/// <param name="Namespace">The namespace of the notification type.</param>
/// <param name="PrimaryInterface">The primary interface implemented (typically INotification).</param>
/// <param name="HandlerStatus">The status of automatic handlers (INotificationHandler&lt;T&gt;) for this notification type.</param>
/// <param name="HandlerDetails">Detailed information about the automatic handlers.</param>
/// <param name="Handlers">List of automatic handler types registered for this notification.</param>
/// <param name="SubscriberStatus">The status of manual subscribers for this notification type.</param>
/// <param name="SubscriberDetails">Detailed information about the manual subscribers.</param>
/// <param name="EstimatedSubscribers">Estimated number of manual subscribers (may not be exact due to dynamic registration).</param>
public sealed record NotificationAnalysis(
    Type Type,
    string ClassName,
    string TypeParameters,
    string Assembly,
    string Namespace,
    string PrimaryInterface,
    HandlerStatus HandlerStatus,
    string HandlerDetails,
    IReadOnlyList<Type> Handlers,
    SubscriberStatus SubscriberStatus,
    string SubscriberDetails,
    int EstimatedSubscribers
);

/// <summary>
/// Represents the status of subscribers for a notification type.
/// </summary>
public enum SubscriberStatus
{
    /// <summary>
    /// No subscribers are currently registered for this notification type.
    /// </summary>
    None = 0,

    /// <summary>
    /// One or more subscribers are registered for this notification type.
    /// </summary>
    Present = 1,

    /// <summary>
    /// Cannot determine subscriber status (may be due to dynamic registration).
    /// </summary>
    Unknown = 2
}