namespace Blazing.Mediator;

/// <summary>
/// Handler interface for processing notifications with automatic discovery.
/// Handlers are automatically discovered and registered during service registration.
/// Unlike subscribers, handlers don't need manual subscription - they're invoked automatically.
/// </summary>
/// <remarks>
/// <para>
/// The INotificationHandler interface provides an automatic discovery pattern for notification handling.
/// When you implement this interface, handlers are automatically discovered during service registration
/// and invoked when matching notifications are published.
/// </para>
/// <para>
/// Key differences from INotificationSubscriber:
/// - Automatic discovery: No need for manual subscription/unsubscription
/// - Service-based: Resolved from DI container when notifications are published
/// - Multiple handlers: Multiple handlers can handle the same notification type
/// - Lifecycle: Follows DI container lifecycle (typically scoped)
/// </para>
/// <para>
/// Example usage:
/// <code>
/// public class OrderCreatedNotificationHandler : INotificationHandler&lt;OrderCreatedNotification&gt;
/// {
///     private readonly IEmailService _emailService;
///     
///     public OrderCreatedNotificationHandler(IEmailService emailService)
///     {
///         _emailService = emailService;
///     }
///     
///     public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
///     {
///         await _emailService.SendOrderConfirmationAsync(notification.CustomerEmail, notification.OrderId);
///     }
/// }
/// </code>
/// </para>
/// <para>
/// The handler will be automatically discovered during service registration and invoked
/// whenever an OrderCreatedNotification is published via IMediator.Publish().
/// </para>
/// </remarks>
/// <typeparam name="TNotification">The type of notification to handle</typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called automatically when a matching notification is published.
    /// The handler should process the notification and perform any necessary side effects.
    /// </para>
    /// <para>
    /// Exception handling guidelines:
    /// - Exceptions thrown from handlers will not prevent other handlers from executing
    /// - Consider logging exceptions rather than throwing them to avoid disrupting the notification pipeline
    /// - Use the cancellationToken to support operation cancellation
    /// </para>
    /// <para>
    /// Performance considerations:
    /// - Handlers execute sequentially, so avoid long-running operations
    /// - Consider using background services for heavy processing
    /// - Use async/await properly to avoid blocking
    /// </para>
    /// </remarks>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <example>
    /// <code>
    /// public async Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    /// {
    ///     try
    ///     {
    ///         await _emailService.SendOrderConfirmationAsync(
    ///             notification.CustomerEmail, 
    ///             notification.OrderId,
    ///             cancellationToken);
    ///             
    ///         _logger.LogInformation("Order confirmation sent for order {OrderId}", notification.OrderId);
    ///     }
    ///     catch (Exception ex)
    ///     {
    ///         _logger.LogError(ex, "Failed to send order confirmation for order {OrderId}", notification.OrderId);
    ///         // Don't rethrow to avoid disrupting other handlers
    ///     }
    /// }
    /// </code>
    /// </example>
    Task Handle(TNotification notification, CancellationToken cancellationToken = default);
}