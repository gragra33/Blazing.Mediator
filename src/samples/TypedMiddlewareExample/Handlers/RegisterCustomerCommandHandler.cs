using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for registering new customers.
/// </summary>
public class RegisterCustomerCommandHandler(ILogger<RegisterCustomerCommandHandler> logger)
    : IRequestHandler<RegisterCustomerCommand>
{
    public Task Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(".. Registering customer: {FullName} ({Email})", request.FullName, request.Email);

        // Simulate customer registration
        logger.LogInformation("-- Customer registered successfully: {FullName}", request.FullName);
        
        return Task.CompletedTask;
    }
}