namespace MiddlewareExample.Handlers;

/// <summary>
/// Handles <see cref="RegisterCustomerCommand"/> requests for registering new customers.
/// </summary>
public class RegisterCustomerCommandHandler(ILogger<RegisterCustomerCommandHandler> logger)
    : IRequestHandler<RegisterCustomerCommand>
{
    /// <inheritdoc />
    public Task Handle(RegisterCustomerCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(".. Registering new customer: {FullName} with email: {Email}, preferred contact: {ContactMethod}",
            request.FullName, request.Email, request.ContactMethod);

        // Simulate customer registration
        logger.LogInformation("-- Customer registration completed successfully for {Email}", request.Email);

        return Task.CompletedTask;
    }
}
