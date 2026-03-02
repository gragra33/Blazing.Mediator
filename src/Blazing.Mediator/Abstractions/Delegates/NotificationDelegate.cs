namespace Blazing.Mediator;

/// <summary>
/// Delegate for the notification pipeline execution
/// </summary>
/// <typeparam name="TNotification">The type of notification</typeparam>
/// <param name="notification">The notification to process</param>
/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation</param>
/// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation</returns>
public delegate ValueTask NotificationDelegate<in TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification;
