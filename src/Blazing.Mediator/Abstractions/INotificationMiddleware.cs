namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Middleware interface for processing notifications in the pipeline.
/// Unlike request middleware, notification middleware is one-way (sender to receiver).
/// </summary>
public interface INotificationMiddleware
{
    /// <summary>
    /// Gets the execution order for this middleware. Lower numbers execute first.
    /// Default is 0 if not specified (neutral order).
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Invokes the middleware for the specified notification.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification</typeparam>
    /// <param name="notification">The notification being processed</param>
    /// <param name="next">The next delegate in the pipeline</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification;
}
