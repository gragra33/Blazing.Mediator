using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Notifications;

/// <summary>
/// Advanced opt-in notification publisher that wraps <see cref="ConcurrentNotificationPublisher"/>
/// and opens a child <see cref="Activity"/> span per handler invocation on the concurrent path.
/// <para>
/// Used only when <see cref="Configuration.TelemetryOptions.CreateHandlerChildSpans"/> is <see langword="true"/>;
/// registered by the generated <c>AddMediator()</c> in place of <see cref="ConcurrentNotificationPublisher"/>.
/// </para>
/// </summary>
public sealed class TelemetryConcurrentPublisher : INotificationPublisher
{
    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="TelemetryConcurrentPublisher"/>.
    /// </summary>
    /// <param name="options">Telemetry options (must have <see cref="TelemetryOptions.CreateHandlerChildSpans"/> true).</param>
    public TelemetryConcurrentPublisher(TelemetryOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask Publish<TNotification>(
        NotificationHandlers<TNotification> handlers,
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var arr = handlers.Handlers;
        if (arr.Length == 0) return default;

        return _options.Enabled && _options.CreateHandlerChildSpans
            ? PublishWithSpans(arr, notification, cancellationToken)
            : PublishConcurrent(arr, notification, cancellationToken);
    }

    private static async ValueTask PublishWithSpans<TNotification>(
        INotificationHandler<TNotification>[] arr,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;

        // Start all handlers concurrently — each wrapped in its own child span.
        Task[]? pending = null;
        var count = 0;

        foreach (var handler in arr)
        {
            var handlerName = handler.GetType().Name;
            // Capture for lambda — each handler gets its own span
            var h = handler;
            var hn = handlerName;

            var task = Task.Run(async () =>
            {
                using var childSpan = Mediator.ActivitySource.StartActivity(
                    $"Mediator.Handler.{notificationName}.{hn}",
                    ActivityKind.Internal);
                childSpan?.SetTag("handler.type", hn);
                childSpan?.SetTag("notification.type", notificationName);

                try
                {
                    await h.Handle(notification, ct).ConfigureAwait(false);
                    childSpan?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    childSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    throw;
                }
            }, ct);

            pending ??= new Task[arr.Length];
            pending[count++] = task;
        }

        if (pending is null) return;

        List<Exception>? exceptions = null;
        for (int i = 0; i < count; i++)
        {
            try { await pending[i].ConfigureAwait(false); }
            catch (Exception ex) { (exceptions ??= new List<Exception>()).Add(ex); }
        }

        if (exceptions is { Count: > 0 })
            throw new AggregateException("One or more notification handlers threw an exception.", exceptions);
    }

    private static async ValueTask PublishConcurrent<TNotification>(
        INotificationHandler<TNotification>[] arr,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        Task[]? pending = null;
        var pendingCount = 0;

        foreach (var handler in arr)
        {
            var vt = handler.Handle(notification, ct);
            if (vt.IsCompletedSuccessfully) continue;
            pending ??= new Task[arr.Length];
            pending[pendingCount++] = vt.AsTask();
        }

        if (pendingCount == 0) return;

        List<Exception>? exceptions = null;
        for (int i = 0; i < pendingCount; i++)
        {
            try { await pending![i].ConfigureAwait(false); }
            catch (Exception ex) { (exceptions ??= new List<Exception>()).Add(ex); }
        }

        if (exceptions is { Count: > 0 })
            throw new AggregateException("One or more notification handlers threw an exception.", exceptions);
    }
}
