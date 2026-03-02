using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Notifications;

/// <summary>
/// Advanced opt-in notification publisher that wraps <see cref="SequentialNotificationPublisher"/>
/// and opens a child <see cref="Activity"/> span per handler invocation.
/// <para>
/// Used only when <see cref="Configuration.TelemetryOptions.CreateHandlerChildSpans"/> is <see langword="true"/>;
/// registered by the generated <c>AddMediator()</c> in place of <see cref="SequentialNotificationPublisher"/>.
/// When <c>CreateHandlerChildSpans</c> is <see langword="false"/>, <see cref="Middleware.TelemetryNotificationMiddleware"/>
/// provides a single outer span for the entire publish operation at lower cost.
/// </para>
/// </summary>
public sealed class TelemetrySequentialPublisher : INotificationPublisher
{
    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="TelemetrySequentialPublisher"/>.
    /// </summary>
    /// <param name="options">Telemetry options (must have <see cref="TelemetryOptions.CreateHandlerChildSpans"/> true).</param>
    public TelemetrySequentialPublisher(TelemetryOptions options)
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
            : PublishSequential(arr, notification, cancellationToken);
    }

    private static async ValueTask PublishWithSpans<TNotification>(
        INotificationHandler<TNotification>[] arr,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        var notificationName = typeof(TNotification).Name;

        foreach (var handler in arr)
        {
            var handlerName = handler.GetType().Name;
            using var childSpan = Mediator.ActivitySource.StartActivity(
                $"Mediator.Handler.{notificationName}.{handlerName}",
                ActivityKind.Internal);

            childSpan?.SetTag("handler.type", handlerName);
            childSpan?.SetTag("notification.type", notificationName);

            try
            {
                await handler.Handle(notification, ct).ConfigureAwait(false);
                childSpan?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                childSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }
    }

    private static async ValueTask PublishSequential<TNotification>(
        INotificationHandler<TNotification>[] arr,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        foreach (var handler in arr)
            await handler.Handle(notification, ct).ConfigureAwait(false);
    }
}
