using TypedMiddlewareExample.Commands;

namespace TypedMiddlewareExample.Handlers;

/// <summary>
/// Handler for updating customer details.
/// Handles custom ICustomerRequest interface with response.
/// </summary>
public class UpdateCustomerDetailsCommandHandler(ILogger<UpdateCustomerDetailsCommandHandler> logger)
    : IRequestHandler<UpdateCustomerDetailsCommand, bool>
{
    public Task<bool> Handle(UpdateCustomerDetailsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(".. Updating customer details for ID: {CustomerId}", request.CustomerId);

        // Simulate customer details update
        logger.LogInformation("-- Customer details updated successfully for {CustomerId}", request.CustomerId);

        return Task.FromResult(true);
    }
}