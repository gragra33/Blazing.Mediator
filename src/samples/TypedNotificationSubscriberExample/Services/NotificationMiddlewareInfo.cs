namespace TypedNotificationSubscriberExample.Services;

/// <summary>
/// Contains detailed information about a notification middleware component.
/// </summary>
public record NotificationMiddlewareInfo(
    int Order,
    string OrderDisplay,
    string ClassName,
    string TypeParameters,
    string GenericConstraints
);