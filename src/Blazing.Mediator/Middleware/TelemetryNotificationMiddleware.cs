using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace Blazing.Mediator.Middleware;

/// <summary>
/// Optional OpenTelemetry tracing and metrics middleware for notification publish operations.
/// Implements <see cref="INotificationMiddleware"/> so it wraps <strong>all</strong> notifications.
/// Opens a <c>Mediator.Publish:{NotificationName}</c> Activity and records per-Publish duration
/// and success/failure counters on <see cref="Mediator.Meter"/>.
/// <para>
/// Register via the generated <c>AddMediator(MediatorConfiguration config)</c> when
/// <c>config.TelemetryOptions != null</c>. When not registered, zero overhead is incurred.
/// </para>
/// </summary>
public sealed class TelemetryNotificationMiddleware : INotificationMiddleware
{
    private static readonly Histogram<double> _durationHistogram =
        Mediator.Meter.CreateHistogram<double>(
            "mediator.publish.duration", unit: "ms",
            description: "Duration of mediator publish operations");

    private static readonly Counter<long> _successCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.success",
            description: "Number of successful mediator publish operations");

    private static readonly Counter<long> _failureCounter =
        Mediator.Meter.CreateCounter<long>(
            "mediator.publish.failure",
            description: "Number of failed mediator publish operations");

    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initialises a new <see cref="TelemetryNotificationMiddleware"/>.
    /// </summary>
    /// <param name="options">Telemetry options controlling capture behaviour.</param>
    public TelemetryNotificationMiddleware(TelemetryOptions options)
        => _options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask InvokeAsync<TNotification>(
        TNotification notification,
        NotificationDelegate<TNotification> next,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        if (!_options.Enabled)
        {
            await next(notification, cancellationToken).ConfigureAwait(false);
            return;
        }

        var notificationName = typeof(TNotification).Name;

        using var activity = Mediator.ActivitySource.StartActivity($"Mediator.Publish.{notificationName}");
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetTag("notification.type", notificationName);
        activity?.SetTag("operation", "publish");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Exception? exception = null;

        try
        {
            await next(notification, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            exception = ex;
            throw;
        }
        finally
        {
            sw.Stop();
            var tags = new TagList { { "notification_type", notificationName } };

            _durationHistogram.Record(sw.Elapsed.TotalMilliseconds, tags);

            if (exception == null)
            {
                _successCounter.Add(1, tags);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                _failureCounter.Add(1, tags);
            }
        }
    }
}
