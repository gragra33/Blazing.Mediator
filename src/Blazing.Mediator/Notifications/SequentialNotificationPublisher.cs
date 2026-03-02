using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Notifications;

/// <summary>
/// Default notification publisher that invokes handlers sequentially via <c>foreach await</c>.
/// <para>
/// <strong>Performance fast paths:</strong>
/// <list type="bullet">
///   <item>0 handlers — returns <see langword="default"/> <see cref="ValueTask"/> immediately (no allocation)</item>
///   <item>1 handler — returns the handler's <see cref="ValueTask"/> directly for sync-completing handlers</item>
///   <item>2 handlers — unrolled two-step await; no loop overhead</item>
///   <item>3 handlers — unrolled three-step await; no loop overhead</item>
///   <item>4+ handlers — foreach loop over the cached array</item>
/// </list>
/// </para>
/// </summary>
public sealed class SequentialNotificationPublisher : INotificationPublisher
{
    /// <summary>
    /// Invokes all handlers sequentially, awaiting each before the next.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="handlers">Pre-resolved handler array.</param>
    /// <param name="notification">The notification to deliver.</param>
    /// <param name="cancellationToken">Token that can cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that completes when all handlers have finished.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var arr = handlers.Handlers;
        return arr.Length switch
        {
            0 => default,
            1 => arr[0].Handle(notification, cancellationToken),
            2 => PublishTwo(arr[0].Handle(notification, cancellationToken),
                            arr[1].Handle(notification, cancellationToken)),
            3 => PublishThree(arr[0].Handle(notification, cancellationToken),
                              arr[1].Handle(notification, cancellationToken),
                              arr[2].Handle(notification, cancellationToken)),
            _ => PublishAll(arr, notification, cancellationToken),
        };
    }

    private static async ValueTask PublishTwo(ValueTask first, ValueTask second)
    {
        await first.ConfigureAwait(false);
        await second.ConfigureAwait(false);
    }

    private static async ValueTask PublishThree(ValueTask first, ValueTask second, ValueTask third)
    {
        await first.ConfigureAwait(false);
        await second.ConfigureAwait(false);
        await third.ConfigureAwait(false);
    }

    private static async ValueTask PublishAll<TNotification>(
        INotificationHandler<TNotification>[] arr,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        foreach (var handler in arr)
        {
            await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
