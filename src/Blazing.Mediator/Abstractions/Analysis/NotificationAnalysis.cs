namespace Blazing.Mediator;

/// <summary>
/// Represents the notification processing pattern in use.
/// </summary>
public enum NotificationPattern
{
    /// <summary>
    /// No handlers or subscribers detected for this notification type.
    /// </summary>
    None = 0,

    /// <summary>
    /// Uses automatic handlers registered in DI container (INotificationHandler&lt;T&gt; pattern).
    /// Handlers are discovered and invoked automatically by the mediator.
    /// </summary>
    AutomaticHandlers = 1,

    /// <summary>
    /// Uses manual subscribers registered at runtime (INotificationSubscriber&lt;T&gt; pattern).
    /// Subscribers actively subscribe/unsubscribe to notifications they're interested in.
    /// </summary>
    ManualSubscribers = 2,

    /// <summary>
    /// Uses both automatic handlers and manual subscribers (hybrid pattern).
    /// Combines DI-registered handlers with runtime-registered subscribers.
    /// </summary>
    Hybrid = 3
}

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
)
{
    // New properties for enhanced pattern support (computed properties for backward compatibility)

    /// <summary>
    /// The notification processing pattern detected for this notification type.
    /// </summary>
    public NotificationPattern Pattern => DetectPattern();

    /// <summary>
    /// The number of active subscribers (same as EstimatedSubscribers for backward compatibility).
    /// </summary>
    public int ActiveSubscriberCount => EstimatedSubscribers;

    /// <summary>
    /// List of subscriber type names for display purposes.
    /// </summary>
    public IReadOnlyList<string> SubscriberTypes => ExtractSubscriberTypes();

    /// <summary>
    /// The total number of handlers (automatic handlers count).
    /// </summary>
    public int HandlerCount => Handlers.Count;

    /// <summary>
    /// Type name for display (TypeName property for backward compatibility).
    /// </summary>
    public string TypeName => $"{ClassName}{TypeParameters}";

    /// <summary>
    /// Handler name for display (HandlerName property for backward compatibility).
    /// </summary>
    public string HandlerName => HandlerDetails;

    /// <summary>
    /// Handler status for display (Status property for backward compatibility).
    /// </summary>
    public HandlerStatus Status => HandlerStatus;

    /// <summary>
    /// Assembly name for display (AssemblyName property for backward compatibility).
    /// </summary>
    public string AssemblyName => Assembly;

    /// <summary>
    /// Whether this notification type supports broadcast (multiple processors).
    /// Returns true if there are multiple handlers, multiple subscribers, or both.
    /// </summary>
    public bool SupportsBroadcast => Pattern switch
    {
        NotificationPattern.AutomaticHandlers => HandlerCount > 1,
        NotificationPattern.ManualSubscribers => ActiveSubscriberCount > 1,
        NotificationPattern.Hybrid => HandlerCount + ActiveSubscriberCount > 1,
        NotificationPattern.None => false,
        _ => false
    };

    /// <summary>
    /// Whether this notification uses a result type (always false for notifications).
    /// Included for consistency with QueryCommandAnalysis.
    /// </summary>
    public bool IsResultType => false;

    /// <summary>
    /// Detects the notification pattern based on handler and subscriber status.
    /// </summary>
    private NotificationPattern DetectPattern()
    {
        var hasHandlers = HandlerStatus != HandlerStatus.Missing;
        var hasSubscribers = SubscriberStatus == SubscriberStatus.Present;

        return (hasHandlers, hasSubscribers) switch
        {
            (true, true) => NotificationPattern.Hybrid,
            (true, false) => NotificationPattern.AutomaticHandlers,
            (false, true) => NotificationPattern.ManualSubscribers,
            (false, false) => NotificationPattern.None
        };
    }

    /// <summary>
    /// Extracts subscriber type names from the subscriber details string.
    /// </summary>
    private IReadOnlyList<string> ExtractSubscriberTypes()
    {
        if (SubscriberStatus != SubscriberStatus.Present || string.IsNullOrEmpty(SubscriberDetails))
        {
            return Array.Empty<string>();
        }

        // Try to extract subscriber types from details string
        // This is a best-effort extraction based on the current format
        var details = SubscriberDetails;
        
        // Look for patterns like "(Type1, Type2)" or "Type1, Type2"
        var parenStart = details.IndexOf('(');
        var parenEnd = details.IndexOf(')', parenStart + 1);
        
        if (parenStart >= 0 && parenEnd > parenStart)
        {
            var typesPart = details.Substring(parenStart + 1, parenEnd - parenStart - 1);
            return typesPart.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(t => t.Trim())
                           .ToArray();
        }

        // Fallback: return empty array
        return Array.Empty<string>();
    }
};

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