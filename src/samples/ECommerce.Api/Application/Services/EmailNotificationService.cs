using Blazing.Mediator;
using ECommerce.Api.Application.Notifications;
using ECommerce.Api.Domain.Entities;

namespace ECommerce.Api.Application.Services;

/// <summary>
/// Background service that handles email notifications for order-related events.
/// This service demonstrates the observer pattern by listening to order notifications
/// and sending mock emails (logged to console for demonstration purposes).
/// </summary>
public class EmailNotificationService : BackgroundService,
    INotificationSubscriber<OrderCreatedNotification>,
    INotificationSubscriber<OrderStatusChangedNotification>
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the EmailNotificationService.
    /// </summary>
    /// <param name="logger">The logger instance for logging email operations.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public EmailNotificationService(ILogger<EmailNotificationService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Handles order created notifications by sending order confirmation emails.
    /// </summary>
    /// <param name="notification">The order created notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(OrderCreatedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email sending delay
            await Task.Delay(100, cancellationToken);

            // Log the mock email to console (in real-world, this would send actual email)
            _logger.LogInformation("📧 ORDER CONFIRMATION EMAIL SENT");
            _logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
            _logger.LogInformation("   Subject: Order Confirmation - Order #{OrderId}", notification.OrderId);
            _logger.LogInformation("   Order Total: ${TotalAmount:F2}", notification.TotalAmount);
            _logger.LogInformation("   Items: {ItemCount}", notification.Items.Count);
            
            foreach (var item in notification.Items)
            {
                _logger.LogInformation("   - {ProductName} x{Quantity} @ ${UnitPrice:F2}", 
                    item.ProductName, item.Quantity, item.UnitPrice);
            }

            _logger.LogInformation("   Created: {CreatedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.CreatedAt);
            _logger.LogInformation("   Thank you for your order!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", notification.OrderId);
        }
    }

    /// <summary>
    /// Handles order status changed notifications by sending status update emails.
    /// </summary>
    /// <param name="notification">The order status changed notification.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnNotification(OrderStatusChangedNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            // Only send emails for specific status changes
            if (ShouldSendEmailForStatus(notification.NewStatus))
            {
                // Simulate email sending delay
                await Task.Delay(100, cancellationToken);

                var emailSubject = GetEmailSubjectForStatus(notification.NewStatus, notification.OrderId);
                var statusMessage = GetStatusMessage(notification.NewStatus);

                _logger.LogInformation("📧 ORDER STATUS UPDATE EMAIL SENT");
                _logger.LogInformation("   To: {CustomerEmail}", notification.CustomerEmail);
                _logger.LogInformation("   Subject: {Subject}", emailSubject);
                _logger.LogInformation("   Order #{OrderId} Status: {PreviousStatus} → {NewStatus}", 
                    notification.OrderId, notification.PreviousStatus, notification.NewStatus);
                _logger.LogInformation("   Message: {StatusMessage}", statusMessage);
                _logger.LogInformation("   Order Total: ${TotalAmount:F2}", notification.TotalAmount);
                _logger.LogInformation("   Updated: {ChangedAt:yyyy-MM-dd HH:mm:ss} UTC", notification.ChangedAt);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order status update email for order {OrderId}", notification.OrderId);
        }
    }

    /// <summary>
    /// Determines if an email should be sent for the given order status.
    /// </summary>
    /// <param name="status">The order status.</param>
    /// <returns>True if an email should be sent, false otherwise.</returns>
    private static bool ShouldSendEmailForStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Processing => true,
            OrderStatus.Shipped => true,
            OrderStatus.Delivered => true,
            OrderStatus.Cancelled => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the email subject for the given order status.
    /// </summary>
    /// <param name="status">The order status.</param>
    /// <param name="orderId">The order ID.</param>
    /// <returns>The email subject.</returns>
    private static string GetEmailSubjectForStatus(OrderStatus status, int orderId)
    {
        return status switch
        {
            OrderStatus.Processing => $"Order #{orderId} is Now Being Processed",
            OrderStatus.Shipped => $"Order #{orderId} Has Been Shipped",
            OrderStatus.Delivered => $"Order #{orderId} Has Been Delivered",
            OrderStatus.Cancelled => $"Order #{orderId} Has Been Cancelled",
            _ => $"Order #{orderId} Status Update"
        };
    }

    /// <summary>
    /// Gets the status message for the given order status.
    /// </summary>
    /// <param name="status">The order status.</param>
    /// <returns>The status message.</returns>
    private static string GetStatusMessage(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Processing => "Your order is being prepared for shipment.",
            OrderStatus.Shipped => "Your order has been shipped and is on its way to you.",
            OrderStatus.Delivered => "Your order has been successfully delivered. Thank you for your business!",
            OrderStatus.Cancelled => "Your order has been cancelled. If you have questions, please contact customer service.",
            _ => "Your order status has been updated."
        };
    }

    /// <summary>
    /// Executes the background service. This method subscribes to notifications via the mediator.
    /// </summary>
    /// <param name="stoppingToken">A cancellation token to stop the service.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Notification Service started");

        // Subscribe to notifications through the mediator
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        mediator.Subscribe<OrderCreatedNotification>(this);
        mediator.Subscribe<OrderStatusChangedNotification>(this);

        _logger.LogInformation("Email Notification Service subscribed to order notifications");

        // Keep the service running
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email Notification Service is stopping");
        }
    }

    /// <summary>
    /// Disposes the service and unsubscribes from notifications.
    /// </summary>
    public override void Dispose()
    {
        try
        {
            // Unsubscribe from notifications
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            mediator.Unsubscribe<OrderCreatedNotification>(this);
            mediator.Unsubscribe<OrderStatusChangedNotification>(this);

            _logger.LogInformation("Email Notification Service unsubscribed from notifications");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Email Notification Service disposal");
        }
        finally
        {
            base.Dispose();
        }
    }
}
