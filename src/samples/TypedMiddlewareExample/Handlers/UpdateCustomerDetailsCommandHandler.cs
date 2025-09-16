using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for updating customer details.
/// </summary>
public class UpdateCustomerDetailsCommandHandler : ICommandHandler<UpdateCustomerDetailsCommand, bool>
{
    private readonly ILogger<UpdateCustomerDetailsCommandHandler> _logger;

    public UpdateCustomerDetailsCommandHandler(ILogger<UpdateCustomerDetailsCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCustomerDetailsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(".. Updating customer details for ID: {CustomerId}", request.CustomerId);

        // Simulate customer details update processing
        await Task.Delay(75, cancellationToken);

        _logger.LogInformation("-- Customer details updated successfully for {CustomerId}", request.CustomerId);

        return true;
    }
}