using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for sending order confirmation emails.
/// Handles custom IOrderRequest interface.
/// </summary>
public class SendOrderConfirmationCommandHandler(ILogger<SendOrderConfirmationCommandHandler> logger)
    : IRequestHandler<SendOrderConfirmationCommand>
{
    public Task Handle(SendOrderConfirmationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(".. Sending order confirmation for order: {OrderId} to: {CustomerEmail}",
            request.OrderId, request.CustomerEmail);

        // Simulate email sending
        logger.LogInformation("-- Order confirmation email sent successfully for order {OrderId}", request.OrderId);

        return Task.CompletedTask;
    }
}