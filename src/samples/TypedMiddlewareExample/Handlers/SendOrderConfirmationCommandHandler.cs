using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for sending order confirmation emails.
/// </summary>
public class SendOrderConfirmationCommandHandler : ICommandHandler<SendOrderConfirmationCommand>
{
    private readonly ILogger<SendOrderConfirmationCommandHandler> _logger;

    public SendOrderConfirmationCommandHandler(ILogger<SendOrderConfirmationCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(SendOrderConfirmationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(".. Sending order confirmation for order: {OrderId} to: {CustomerEmail}",
            request.OrderId, request.CustomerEmail);

        // Simulate email sending
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("-- Order confirmation email sent successfully for order {OrderId}", request.OrderId);
    }
}