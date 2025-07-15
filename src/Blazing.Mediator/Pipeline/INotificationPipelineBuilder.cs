namespace Blazing.Mediator.Pipeline;

/// <summary>
/// Configuration interface for building notification middleware pipeline.
/// This is part of the core Blazing.Mediator infrastructure and contains no business logic.
/// </summary>
public interface INotificationPipelineBuilder
{
    /// <summary>
    /// Adds a notification middleware type to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The middleware type that implements INotificationMiddleware</typeparam>
    /// <returns>The pipeline builder for chaining</returns>
    INotificationPipelineBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, INotificationMiddleware;

    /// <summary>
    /// Adds a notification middleware type to the pipeline using a Type parameter.
    /// </summary>
    /// <param name="middlewareType">The notification middleware type</param>
    /// <returns>The pipeline builder for chaining</returns>
    INotificationPipelineBuilder AddMiddleware(Type middlewareType);

    /// <summary>
    /// Builds the notification middleware pipeline for the specified notification type.
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <param name="serviceProvider">Service provider for resolving middleware instances</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <returns>A delegate that executes the complete pipeline</returns>
    NotificationDelegate<TNotification> Build<TNotification>(
        IServiceProvider serviceProvider, 
        NotificationDelegate<TNotification> finalHandler)
        where TNotification : INotification;

    /// <summary>
    /// Executes the notification middleware pipeline for the specified notification.
    /// </summary>
    /// <typeparam name="TNotification">The notification type</typeparam>
    /// <param name="notification">The notification to process</param>
    /// <param name="serviceProvider">The service provider for dependency resolution</param>
    /// <param name="finalHandler">The final handler to execute after all middleware</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task ExecutePipeline<TNotification>(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationDelegate<TNotification> finalHandler,
        CancellationToken cancellationToken)
        where TNotification : INotification;
}
