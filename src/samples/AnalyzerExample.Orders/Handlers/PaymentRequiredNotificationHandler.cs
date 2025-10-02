using AnalyzerExample.Orders.Notifications;
using Blazing.Mediator;
using Microsoft.Extensions.Logging;

namespace AnalyzerExample.Orders.Handlers;

/// <summary>
/// Handler for payment required notifications
/// </summary>
public class PaymentRequiredNotificationHandler : INotificationHandler<PaymentRequiredNotification>
{
    private readonly ILogger<PaymentRequiredNotificationHandler> _logger;

    public PaymentRequiredNotificationHandler(ILogger<PaymentRequiredNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PaymentRequiredNotification notification, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Payment required for order {OrderId} ({OrderNumber}). Amount: {Amount:C}, Due: {PaymentDueDate}",
            notification.OrderId, notification.OrderNumber, notification.Amount, notification.PaymentDueDate);

        await SendPaymentReminder(notification, cancellationToken);
        await CreatePaymentLink(notification, cancellationToken);
        await ScheduleFollowUp(notification, cancellationToken);
    }

    private async Task SendPaymentReminder(PaymentRequiredNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending payment reminder to {UserEmail} for order {OrderId}", 
            notification.UserEmail, notification.OrderId);
        await Task.Delay(15, cancellationToken);
    }

    private async Task CreatePaymentLink(PaymentRequiredNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating payment link for order {OrderId}", notification.OrderId);
        await Task.Delay(25, cancellationToken);
    }

    private async Task ScheduleFollowUp(PaymentRequiredNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Scheduling payment follow-up for order {OrderId}", notification.OrderId);
        await Task.Delay(10, cancellationToken);
    }
}