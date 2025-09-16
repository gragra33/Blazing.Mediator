namespace MiddlewareExample.Handlers;

/// <summary>
/// Handles <see cref="SendOrderConfirmationCommand"/> requests to send email confirmations.
/// </summary>
public class SendOrderConfirmationCommandHandler(ILogger<SendOrderConfirmationCommandHandler> logger)
    : IRequestHandler<SendOrderConfirmationCommand>
{
    /// <inheritdoc />
    public Task Handle(SendOrderConfirmationCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogDebug(".. Sending order confirmation for order: {OrderId} to: {CustomerEmail}",
            request.OrderId, request.CustomerEmail);

        // Simulate sending email
        logger.LogInformation("-- Order confirmation email sent successfully for order {OrderId} to {CustomerEmail}",
            request.OrderId, request.CustomerEmail);

        return Task.CompletedTask;
    }
}
