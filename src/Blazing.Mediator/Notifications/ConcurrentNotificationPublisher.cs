using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Notifications;

/// <summary>
/// Optional parallel notification publisher that starts <strong>all</strong> handlers synchronously
/// before the first <c>await</c>, then collects and awaits only the non-completed tasks.
/// <para>
/// Key properties:
/// <list type="bullet">
///   <item>All handlers are started synchronously in a single <c>foreach</c> pass — no per-handler allocation</item>
///   <item>Handlers that complete synchronously (<see cref="ValueTask.IsCompletedSuccessfully"/>) are skipped completely — zero overhead on the fast path</item>
///   <item>Pending tasks are aggregated and waited; exceptions from <strong>all</strong> handlers are collected into an <see cref="AggregateException"/> rather than stopping on the first failure</item>
///   <item>Does <strong>not</strong> use <c>Task.WhenAll</c> — avoids its array allocation on common paths</item>
/// </list>
/// </para>
/// Configure via <see cref="Configuration.MediatorOptions.NotificationPublisher"/> =
/// <see cref="Configuration.NotificationPublisherType.Concurrent"/>.
/// </summary>
public sealed class ConcurrentNotificationPublisher : INotificationPublisher
{
    /// <summary>
    /// Invokes all handlers concurrently, starting each synchronously before yielding.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="handlers">Pre-resolved handler array.</param>
    /// <param name="notification">The notification to deliver.</param>
    /// <param name="cancellationToken">Token that can cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes when all handlers have finished.
    /// Throws <see cref="AggregateException"/> if one or more handlers threw.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var arr = handlers.Handlers;
        if (arr.Length == 0) return default;

        // Start all handlers synchronously before the first await.
        // Only collect tasks that are not yet complete — sync-completing handlers cost nothing.
        Task[]? pending = null;
        var pendingCount = 0;

        foreach (var handler in arr)
        {
            var handlerTask = handler.Handle(notification, cancellationToken);
            if (handlerTask.IsCompletedSuccessfully) continue; // fast path

            pending ??= new Task[arr.Length];
            pending[pendingCount++] = handlerTask.AsTask();
        }

        return pendingCount == 0 ? default : AwaitPending(pending!, pendingCount);
    }

    private static async ValueTask AwaitPending(Task[] pending, int count)
    {
        List<Exception>? exceptions = null;

        for (int i = 0; i < count; i++)
        {
            try
            {
                await pending[i].ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(count);
                exceptions.Add(ex);
            }
        }

        if (exceptions is { Count: > 0 })
        {
            throw new AggregateException(
                "One or more notification handlers threw an exception.",
                exceptions);
        }
    }
}
