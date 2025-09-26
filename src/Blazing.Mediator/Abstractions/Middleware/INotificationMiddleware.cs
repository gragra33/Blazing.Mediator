namespace Blazing.Mediator.Abstractions;

/// <summary>
/// Middleware interface for processing notifications in the pipeline.
/// Notification middleware is two-way: supports both preprocessing and postprocessing around handler execution.
/// Unlike request middleware, notifications follow a one-to-many pattern where multiple handlers process the same notification.
/// Uses InvokeAsync to distinguish from request middleware's HandleAsync pattern.
/// </summary>
public interface INotificationMiddleware
{
    /// <summary>
    /// Gets the execution order for this middleware. Lower numbers execute first.
    /// Default is 0 if not specified (neutral order).
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Invokes the middleware processing for the specified notification.
    /// Middleware can perform preprocessing, call next(), then perform postprocessing.
    /// The next() delegate contains the downstream middleware pipeline and ultimately the handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification</typeparam>
    /// <param name="notification">The notification being processed</param>
    /// <param name="next">The next delegate in the pipeline (contains downstream middleware and handlers)</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InvokeAsync<TNotification>(TNotification notification, NotificationDelegate<TNotification> next, CancellationToken cancellationToken)
        where TNotification : INotification;
}

/// <summary>
/// Generic middleware interface for processing type-constrained notifications in the pipeline.
/// This provides compile-time type safety and performance optimization by enabling the pipeline 
/// to skip middleware that don't match the notification type constraint.
/// Middleware can perform both preprocessing and postprocessing around handler execution.
/// </summary>
/// <typeparam name="TNotification">The constrained notification type that this middleware processes</typeparam>
public interface INotificationMiddleware<TNotification> : INotificationMiddleware
    where TNotification : INotification
{
    /// <summary>
    /// Invokes the notification processing with type-specific constraints.
    /// This method provides compile-time type safety and allows middleware
    /// to work only with notifications of the specified type or its subtypes.
    /// The pipeline execution logic will automatically skip this middleware
    /// if the notification doesn't match the TNotification constraint.
    /// </summary>
    /// <param name="notification">The notification to process</param>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task InvokeAsync(
        TNotification notification,
        NotificationDelegate<TNotification> next,
        CancellationToken cancellationToken);
}
