using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for registering new customers.
/// </summary>
public class RegisterCustomerCommandHandler : ICommandHandler<RegisterCustomerCommand>
{
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;

    public RegisterCustomerCommandHandler(ILogger<RegisterCustomerCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(".. Registering customer: {FullName} ({Email})", request.FullName, request.Email);

        // Simulate customer registration processing
        await Task.Delay(50, cancellationToken);

        _logger.LogInformation("-- Customer registered successfully: {FullName}", request.FullName);
    }
}