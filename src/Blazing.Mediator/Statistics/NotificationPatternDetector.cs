using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Blazing.Mediator.Statistics;

/// <summary>
/// Detects notification patterns by analysing registered handlers and active subscribers.
/// <para>
/// When an <see cref="IMediatorTypeCatalog"/> is available in DI (registered by the source
/// generator's <c>AddMediator()</c>), handler detection uses the compile-time catalog — zero
/// reflection, zero <c>MakeGenericType</c>, fully AOT-compatible.
/// When the catalog is absent (non-source-gen apps), falls back to the
/// <c>INotificationHandler&lt;T&gt;</c> DI probe path.
/// </para>
/// </summary>
public sealed class NotificationPatternDetector : INotificationPatternDetector
{
    private readonly ISubscriberTracker? _subscriberTracker;
    private readonly IMediatorTypeCatalog? _catalog;

    /// <summary>
    /// Initialises a new instance of <see cref="NotificationPatternDetector"/>.
    /// </summary>
    /// <param name="subscriberTracker">Optional subscriber tracker for detecting active subscribers.</param>
    /// <param name="catalog">
    /// Optional compile-time catalog emitted by Blazing.Mediator.SourceGenerators.
    /// When provided, handler detection is AOT-clean (no <c>MakeGenericType</c>).
    /// </param>
    public NotificationPatternDetector(
        ISubscriberTracker? subscriberTracker = null,
        IMediatorTypeCatalog? catalog = null)
    {
        _subscriberTracker = subscriberTracker;
        _catalog = catalog;
    }

    /// <summary>
    /// Detects the notification pattern for a specific notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to analyse.</param>
    /// <param name="serviceProvider">Service provider used as a fallback when no catalog is available.</param>
    /// <returns>The detected <see cref="NotificationPattern"/>.</returns>
    public NotificationPattern DetectPattern(Type notificationType, IServiceProvider serviceProvider)
    {
        var hasHandlers = HasRegisteredHandlers(notificationType, serviceProvider);
        var hasSubscribers = HasActiveSubscribers(notificationType);

        return (hasHandlers, hasSubscribers) switch
        {
            (true, true) => NotificationPattern.Hybrid,
            (true, false) => NotificationPattern.AutomaticHandlers,
            (false, true) => NotificationPattern.ManualSubscribers,
            _ => NotificationPattern.None,
        };
    }

    /// <summary>
    /// Checks whether automatic handlers are registered for a notification type.
    /// Uses the compile-time catalog when available (AOT-clean); otherwise falls back to
    /// <c>MakeGenericType</c> + <c>GetServices</c>.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <param name="serviceProvider">Service provider for the reflection fallback path.</param>
    /// <returns><see langword="true"/> if at least one handler is registered.</returns>
    public bool HasRegisteredHandlers(Type notificationType, IServiceProvider serviceProvider)
    {
        // Fast path: use compile-time catalog — zero reflection.
        if (_catalog is not null)
        {
            foreach (var entry in _catalog.NotificationHandlers)
            {
                if (entry.NotificationType == notificationType && entry.HandlerTypes.Count > 0)
                    return true;
            }
            return false;
        }

        // Reflection fallback for non-source-gen apps.
        return HasRegisteredHandlersReflection(notificationType, serviceProvider);
    }

    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2055",
        Justification = "Fallback path only used in non-AOT / non-source-gen scenarios.")]
    private static bool HasRegisteredHandlersReflection(Type notificationType, IServiceProvider serviceProvider)
    {
        try
        {
            var handlerInterfaceType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
            var handlerServices = serviceProvider.GetServices(handlerInterfaceType);
            return handlerServices.Any(h => h != null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking handlers for notification {notificationType.Name}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Checks whether active manual subscribers exist for a notification type.
    /// </summary>
    /// <param name="notificationType">The notification type to check.</param>
    /// <returns><see langword="true"/> if at least one active subscriber exists.</returns>
    public bool HasActiveSubscribers(Type notificationType)
    {
        if (_subscriberTracker == null)
            return false;

        try
        {
            return _subscriberTracker.GetActiveSubscribers(notificationType).Count > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking subscribers for notification {notificationType.Name}: {ex.Message}");
            return false;
        }
    }
}